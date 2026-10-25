using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    /// <summary>Authoritative annual state. No Unity, network, clock, or random dependencies.</summary>
    public sealed partial class AnnualTown
    {
        readonly AnnualTownDefinition definition;
        readonly Dictionary<string, TownSiteDefinition> sites;
        readonly Dictionary<string, TownActionDefinition> actions;
        readonly List<TownYearReport> history = new List<TownYearReport>();
        readonly Stack<TownPlanSnapshot> undo = new Stack<TownPlanSnapshot>();
        readonly Stack<TownPlanSnapshot> redo = new Stack<TownPlanSnapshot>();
        TownPlannedAction[] queue = Array.Empty<TownPlannedAction>();
        TownAsset[] assets = Array.Empty<TownAsset>();
        TownYearRequest pending;
        TownSimulationState numericState;
        public string RunId { get; }
        public string DefinitionHash { get; }
        public TownPhase Phase { get; private set; } = TownPhase.Planning;
        public int Year { get; private set; } = 1;
        public int Revision { get; private set; }
        public long CashCents { get; private set; }
        public long ArrearsCents { get; private set; }
        public bool CanRedo => Phase == TownPhase.Planning && redo.Count > 0;
        public bool CanUndo => Phase == TownPhase.Planning && undo.Count > 0;
        public long CommittedCents => checked(ArrearsCents + FutureCosts(assets, Phase == TownPhase.Reviewing || Phase == TownPhase.Completed ? Year + 1 : Year));
        public long QueuedCommitmentCents => NewCommitments(queue);
        public long AvailableCents => checked(CashCents - DueForPlan(queue));
        public AnnualTownDefinition Definition => TownCopies.Copy(definition);
        public TownPlannedAction[] Queue => TownCopies.Copy(queue);
        public TownAsset[] Assets => TownCopies.Copy(assets);
        public TownYearReport[] History => TownCopies.Copy(history.ToArray());
        public TownYearRequest PendingRequest => TownCopies.Copy(pending);

        public AnnualTown(AnnualTownDefinition rules, string runId)
        {
            ValidateDefinition(rules);
            Require(!string.IsNullOrWhiteSpace(runId), "A stable run ID is required.");
            definition = TownCopies.Copy(rules);
            DefinitionHash = TownCopies.Hash(definition);
            RunId = runId;
            sites = definition.sites.ToDictionary(s => s.id, StringComparer.Ordinal);
            actions = definition.actions.ToDictionary(a => Key(a.siteId, a.actionId), StringComparer.Ordinal);
            CashCents = definition.startingCashCents;
            numericState = AnnualSimulation.Initial(definition);
        }

        static string Key(string siteId, string actionId) => siteId + ":" + actionId;
        TownActionDefinition Action(string siteId, string actionId)
        {
            Require(actions.TryGetValue(Key(siteId, actionId), out var action), "Unknown or ineligible site/action.");
            return action;
        }
        void Planning() { if (Phase != TownPhase.Planning) throw new InvalidOperationException("This year is not open for planning."); }
        void Current(int expectedRevision) { if (expectedRevision != Revision) throw new InvalidOperationException("Stale town revision. Refresh the state."); }

        // Reserve current-year costs only; future obligations remain visible separately.
        public long Preview(string siteId, string actionId)
        {
            Planning();
            var next = ProposedQueue(siteId, actionId);
            return checked(CashCents - DueForPlan(next));
        }
        TownPlannedAction[] ProposedQueue(string siteId, string actionId)
        {
            Action(siteId, actionId);
            Require(Year <= PlanningYears, "The decision years are complete. Advance to the Year 10 review.");
            if(PlayerRulesActive && actionId==AnnualPlayerRules.CancelAction)
                CancellationRefund(siteId);
            else
            {
                Require(!BuildingBlockedByDebt,"Repay the negative public budget before starting another project. You can cancel projects or wait for funding.");
                var existing = assets.FirstOrDefault(a => a.siteId == siteId && a.cancelledYear == 0);
                Require(existing == null || existing.actionId != actionId, "This land already has that use. Choose a different land use.");
                Require(existing == null || definition.simulation == null, "Conversion is unavailable in the experimental numeric rules.");
            }
            return queue.Where(q => q.siteId != siteId).Concat(new[] { new TownPlannedAction { siteId = siteId, actionId = actionId } }).ToArray();
        }
        public void QueueAction(string siteId, string actionId, int expectedRevision)
        {
            Planning(); Current(expectedRevision);
            var next = ProposedQueue(siteId, actionId);
            Require(PlayerRulesActive && actionId == AnnualPlayerRules.CancelAction || checked(CashCents - DueForPlan(next)) >= 0, "Not enough cash for this year’s projects, conversion work, upkeep and unpaid bills.");
            if (queue.Any(q => q.siteId == siteId && q.actionId == actionId)) return;
            RememberQueue(); queue = next; Revision++;
        }
        public void RemoveQueuedAction(string siteId, int expectedRevision)
        {
            Planning(); Current(expectedRevision);
            Require(queue.Any(q => q.siteId == siteId), "Only an unsubmitted action can be removed/refunded.");
            RememberQueue(); queue = queue.Where(q => q.siteId != siteId).ToArray(); Revision++;
        }
        void RememberQueue() { undo.Push(new TownPlanSnapshot { actions = TownCopies.Copy(queue) }); redo.Clear(); }
        public void ClearPlan(int expectedRevision)
        {
            Planning(); Current(expectedRevision); if (queue.Length == 0) return;
            RememberQueue(); queue = Array.Empty<TownPlannedAction>(); Revision++;
        }
        TownPlannedAction[] MovedQueue(string source, string destination)
        {
            var proposal = queue.FirstOrDefault(q => q.siteId == source);
            Require(proposal != null, "Only this year's queued projects can move.");
            Require(!BuildingBlockedByDebt&&proposal.actionId!=AnnualPlayerRules.CancelAction,"Cancellation cannot move, and debt blocks new construction.");
            Require(source != destination && !queue.Any(q => q.siteId == destination) && !assets.Any(a => a.siteId == destination&&a.cancelledYear==0), "Choose an unoccupied destination.");
            Action(destination, proposal.actionId);
            return queue.Where(q => q.siteId != source).Concat(new[] { new TownPlannedAction { siteId = destination, actionId = proposal.actionId } }).ToArray();
        }
        public long PreviewMove(string source, string destination)
        { Planning(); return checked(CashCents - DueForPlan(MovedQueue(source, destination))); }
        public void MoveQueuedAction(string source, string destination, int expectedRevision)
        {
            Planning(); Current(expectedRevision); var next = MovedQueue(source, destination);
            ValidateSavedQueue(next); RememberQueue(); queue = next; Revision++;
        }
        public bool RedoPlan(int expectedRevision)
        {
            Planning(); Current(expectedRevision); if (redo.Count == 0) return false;
            undo.Push(new TownPlanSnapshot { actions = TownCopies.Copy(queue) });
            queue = redo.Pop().actions; Revision++; return true;
        }
        public bool UndoPlan(int expectedRevision)
        {
            Planning(); Current(expectedRevision);
            if (undo.Count == 0) return false;
            redo.Push(new TownPlanSnapshot { actions = TownCopies.Copy(queue) });
            queue = undo.Pop().actions; Revision++; return true;
        }

        public TownYearRequest BeginResolution(string requestId, int expectedRevision)
        {
            Require(!string.IsNullOrWhiteSpace(requestId), "A stable resolution request ID is required.");
            var previous = history.FirstOrDefault(r => r.acceptedRequest.requestId == requestId);
            if (previous != null)
            {
                Require(previous.acceptedRequest.revision == expectedRevision, "Request ID reused for a different transition.");
                return TownCopies.Copy(previous.acceptedRequest);
            }
            if (Phase == TownPhase.Resolving)
            {
                Require(pending.requestId == requestId && pending.revision == expectedRevision, "Another year resolution is already in progress.");
                return TownCopies.Copy(pending);
            }
            Planning(); Current(expectedRevision);
            pending = new TownYearRequest {
                schemaVersion = definition.simulation == null ? 1 : 2,
                runId = RunId, requestId = requestId, definitionHash = DefinitionHash, year = Year, revision = Revision,
                actions = TownCopies.Copy(queue), assets = TownCopies.Copy(assets), driver = TownCopies.Copy(definition.drivers[Year - 1]),
                simulation = definition.simulation == null ? null : new TownSimulationRequest { rulesVersion=definition.simulation.version,
                    driver=TownCopies.Copy(definition.simulation.drivers[Year-1]),openingState=TownCopies.Copy(numericState) }
            };
            pending.inputHash = TownCopies.Hash(pending); // inputHash is null while hashing.
            Phase = TownPhase.Resolving;
            return TownCopies.Copy(pending);
        }

        // A provider timeout/failure leaves Resolving + the frozen request intact for retry/save.
        public void CancelResolution(int expectedRevision)
        {
            if (Phase != TownPhase.Resolving) throw new InvalidOperationException("No year is resolving.");
            Current(expectedRevision); pending = null; Phase = TownPhase.Planning; Revision++;
        }

        public TownYearReport CompleteResolution(TownYearConsequences consequences)
        {
            Require(consequences != null, "Consequences are required.");
            var previous = history.FirstOrDefault(r => r.acceptedRequest.requestId == consequences.requestId);
            if (previous != null)
            {
                Require(TownCopies.Hash(previous.acceptedConsequences) == TownCopies.Hash(consequences), "Committed request ID reused with different consequences.");
                return TownCopies.Copy(previous);
            }
            if (Phase != TownPhase.Resolving) throw new InvalidOperationException("Begin a year resolution first.");
            ValidateConsequences(consequences);

            // Everything below is staged. Overflow, invalid effects, or computation failure cannot partially publish a year.
            var nextAssets = AssetsForPlan(pending.actions);
            if(numericState!=null)
                foreach(var asset in nextAssets)asset.condition=numericState.sites.Single(s=>s.siteId==asset.siteId).condition;
            var ledger = new TownYearLedger { currency = definition.currency, priceYear = definition.priceYear,
                openingCashCents = CashCents, openingArrearsCents = ArrearsCents };
            ledger.capitalRefundCents=PlayerRulesActive?Sum(pending.actions.Where(q=>q.actionId==AnnualPlayerRules.CancelAction).Select(q=>CancellationRefund(q.siteId))):0;
            long cash = checked(CashCents+ledger.capitalRefundCents);
            long Pay(long due) { long paid = PlayerRulesActive ? due : Math.Min(cash, due); cash = checked(cash - paid); return paid; }
            long upfront = Sum(pending.actions.Where(q => q.actionId != AnnualPlayerRules.CancelAction).Select(q => checked(Action(q.siteId, q.actionId).upfrontPublicCostCents + ConversionCost(q.siteId))));
            Require(upfront==0 || upfront <= cash, "The committed plan is not affordable.");
            ledger.upfrontPaidCents = Pay(upfront);
            ledger.arrearsPaidCents = Pay(ArrearsCents);
            var funded = new Dictionary<string, bool>();
            foreach (var asset in nextAssets.OrderBy(a => a.startedYear).ThenBy(a => a.siteId, StringComparer.Ordinal))
            {
                var recipe = Action(asset.siteId, asset.actionId);
                long program = ProgramDue(asset, Year), operating = OperatingDue(asset, Year);
                long paidProgram = Pay(program), paidOperating = Pay(operating);
                ledger.programDueCents = checked(ledger.programDueCents + program);
                ledger.programPaidCents = checked(ledger.programPaidCents + paidProgram);
                ledger.operatingDueCents = checked(ledger.operatingDueCents + operating);
                ledger.operatingPaidCents = checked(ledger.operatingPaidCents + paidOperating);
                funded[asset.siteId] = paidProgram == program && paidOperating == operating;
                if (!funded[asset.siteId]) asset.condition = Math.Max(0, asset.condition - .1);
                if(PlayerRulesActive)asset.condition=Clamp01(asset.condition-RegionalDamage(asset));
                var effect = consequences.effects.FirstOrDefault(e => e.siteId == asset.siteId);
                if (effect != null)
                {
                    Require(effect.conditionDelta <= 0 || funded[asset.siteId], "An unfunded intervention cannot gain condition from a provider.");
                    asset.condition = Clamp01(asset.condition + effect.conditionDelta);
                }
                if (IsOperating(asset, Year) && funded[asset.siteId])
                    ledger.publicReceiptsCents = checked(ledger.publicReceiptsCents + recipe.annualPublicReceiptsCents);
            }
            var numeric = definition.simulation == null ? null : PrepareNumeric(nextAssets,funded,consequences,ledger);
            ledger.repairDueCents = numeric == null ? checked(pending.driver.unexpectedPublicRepairCents+(PlayerRulesActive?RegionalRepairs(nextAssets):0)) : numeric.newRepairDue;
            ledger.repairPaidCents = Pay(ledger.repairDueCents);
            ledger.closingArrearsCents = checked(ArrearsCents - ledger.arrearsPaidCents + ledger.programDueCents - ledger.programPaidCents
                + ledger.operatingDueCents - ledger.operatingPaidCents + ledger.repairDueCents - ledger.repairPaidCents);
            var outcomes = numeric == null ? EvaluateOutcomes(nextAssets, funded, consequences, ledger) : SettleNumeric(numeric,nextAssets,funded,ledger);
            if (definition.economy != null)
            {
                ledger.publicReceiptsCents = 0;
                double offeredHomes = outcomes.sites.Where(r => r.actionId != null).Sum(r => Action(r.siteId,r.actionId).services.homes * ActiveShare(r));
                double occupancy = offeredHomes == 0 ? 0 : Math.Min(1, Math.Max(0, definition.economy.householdDemand-definition.economy.baselineHomes)/offeredHomes);
                foreach (var row in outcomes.sites.Where(r => r.actionId != null))
                {
                    var recipe = Action(row.siteId,row.actionId);
                    ledger.publicReceiptsCents = checked(ledger.publicReceiptsCents + (long)Math.Floor(recipe.annualPublicReceiptsCents * ActiveShare(row) * occupancy * outcomes.economy.resourceFraction));
                }
            }
            ledger.appropriationCents = definition.appropriationsCents[Year - 1];
            ledger.closingCashCents = checked(cash + ledger.appropriationCents + ledger.publicReceiptsCents);
            ledger.futureCommitmentsCents = checked(FutureCosts(nextAssets, Year + 1) + ledger.closingArrearsCents);
            var report = new TownYearReport { year = Year, ledger = ledger,
                outcomes = outcomes,
                acceptedRequest = TownCopies.Copy(pending), acceptedConsequences = TownCopies.Copy(consequences), simulation=numeric?.report };
            if(numeric!=null)
            {
                var before=history.LastOrDefault()?.outcomes??AnnualSimulation.OpeningOutcomes(definition);
                report.simulation.economicDelta=outcomes.economicScore-before.economicScore;
                report.simulation.socialDelta=outcomes.socialScore-before.socialScore;
                report.simulation.environmentalDelta=outcomes.environmentalScore-before.environmentalScore;
                AnnualSimulation.ValidateReport(definition,report);
                report.simulation.transitionHash=AnnualSimulation.ReportHash(report);
            }

            assets = nextAssets; CashCents = ledger.closingCashCents; ArrearsCents = ledger.closingArrearsCents;
            if(numeric!=null)numericState=numeric.report.closingState;
            history.Add(report); queue = Array.Empty<TownPlannedAction>(); undo.Clear(); redo.Clear(); pending = null; Revision++;
            Phase = Year == definition.years ? TownPhase.Completed : TownPhase.Reviewing;
            return TownCopies.Copy(report);
        }

        public void ContinuePlanning(int expectedRevision)
        {
            if (Phase != TownPhase.Reviewing) throw new InvalidOperationException("Review a completed year before continuing.");
            Current(expectedRevision); Year++; Phase = TownPhase.Planning; Revision++;
        }

        TownYearOutcomes EvaluateOutcomes(TownAsset[] nextAssets, Dictionary<string, bool> funded, TownYearConsequences consequences, TownYearLedger ledger)
        {
            var rows = new List<TownSiteYearResult>();
            foreach (var site in definition.sites)
            {
                var asset = nextAssets.FirstOrDefault(a => a.siteId == site.id);
                var effect = consequences.effects.FirstOrDefault(e => e.siteId == site.id);
                var row = new TownSiteYearResult { siteId = site.id, actionId = asset?.actionId,
                    condition = asset?.condition ?? 1, maturity = 1, phase = TownAssetPhase.Operating,
                    productionUnits = site.baselineProductionUnits, serviceUnits = site.baselineServiceUnits,
                    vegetatedAreaM2 = site.areaM2 * site.baselineVegetatedFraction };
                if (asset != null && asset.cancelledYear>0)
                {
                    row.actionId=null;row.phase=TownAssetPhase.Retired;row.productionUnits=0;row.serviceUnits=0;
                    row.vegetatedAreaM2=site.areaM2*asset.clearedVegetatedFraction;row.retentionCapacityM3=0;
                    row.explanation="Cancelled project; vacant disturbed land. Existing condition is not instantly restored.";
                    rows.Add(row);continue;
                }
                if (asset != null)
                {
                    var recipe = Action(asset.siteId, asset.actionId);
                    int operationYear = asset.startedYear + recipe.constructionYears + asset.conversionDelayYears;
                    row.maturity = Clamp01((Year - operationYear + 1.0) / recipe.establishmentYears);
                    bool active = IsOperating(asset, Year) && funded[asset.siteId];
                    row.phase = Year < operationYear ? TownAssetPhase.Building : Year >= operationYear + recipe.operatingYears ? TownAssetPhase.Retired : !active ? TownAssetPhase.Suspended
                        : row.maturity < 1 ? TownAssetPhase.Establishing : TownAssetPhase.Operating;
                    row.productionUnits = active ? recipe.productionUnits * row.maturity * row.condition : 0;
                    row.serviceUnits = active ? recipe.serviceUnits * row.maturity * row.condition : 0;
                    double openingCover = string.IsNullOrEmpty(asset.previousActionId) ? site.baselineVegetatedFraction : asset.conversionOpeningVegetation;
                    row.vegetatedAreaM2 = site.areaM2 * (openingCover
                        + (recipe.vegetatedFraction - openingCover) * row.maturity * row.condition);
                    row.retentionCapacityM3 = active ? recipe.retentionCapacityM3 * row.maturity * row.condition : 0;
                }
                if (asset != null && definition.economy != null)
                {
                    var recipe = Action(asset.siteId,asset.actionId);
                    row.productionUnits += site.baselineProductionUnits*(1-recipe.services.occupiedAreaM2/site.areaM2)*(1-ActiveShare(row));
                }
                bool growing = asset == null || definition.economy == null || new[]{"cultivate-standard","cultivate-margin","conservation-rotation","cover-crop"}.Contains(asset.actionId);
                row.productionUnits *= (growing ? pending.driver.productionFactor : 1) * (effect?.productionFactor ?? 1);
                if (asset == null || Action(asset.siteId, asset.actionId).servicesFollowProduction)
                    row.serviceUnits *= pending.driver.productionFactor * (effect?.productionFactor ?? 1);
                row.disrupted = effect?.disrupted ?? false;
                row.explanation = effect?.reason ?? (asset == null ? "Existing use under the saved annual scenario." : "Lifecycle and funding follow the configured action recipe.");
                rows.Add(row);
            }
            var result = new TownYearOutcomes { rubricVersion = definition.rubric.version, sites = rows.ToArray(),
                productionUnits = rows.Sum(r => r.productionUnits), serviceUnits = rows.Sum(r => r.serviceUnits),
                vegetatedAreaM2 = rows.Sum(r => r.vegetatedAreaM2), managedAreaM2 = definition.sites.Sum(s => s.areaM2),
                retentionCapacityM3 = rows.Sum(r => r.retentionCapacityM3), disruptedSites = rows.Count(r => r.disrupted) };
            var rubric = definition.rubric;
            double operatingFunding = ledger.operatingDueCents == 0 ? 1 : (double)ledger.operatingPaidCents / ledger.operatingDueCents;
            result.economicScore = 100 * (rubric.economicProductionWeight * Clamp01(result.productionUnits / rubric.productionTargetUnits)
                + (1 - rubric.economicProductionWeight) * operatingFunding);
            result.socialScore = 100 * (rubric.socialServiceWeight * Clamp01(result.serviceUnits / rubric.serviceTargetUnits)
                + (1 - rubric.socialServiceWeight) * (1.0 - (double)result.disruptedSites / definition.sites.Length));
            result.environmentalScore = 100 * (rubric.environmentalVegetationWeight * Clamp01(result.vegetatedAreaM2 / result.managedAreaM2)
                + (1 - rubric.environmentalVegetationWeight) * Clamp01(result.retentionCapacityM3 / rubric.retentionTargetM3));
            result.economicExplanation = "Illustrative production against target plus funded public operation; private production is not treasury income.";
            result.socialExplanation = "Illustrative community service target and sites without disruption; not measured employment, health or households.";
            result.environmentalExplanation = "Vegetated share of management sites plus configured operational retention capacity; not a flood, biodiversity or water-quality forecast.";
            if (definition.economy != null) AnnualEconomy.Evaluate(definition, result, ledger, pending.driver);
            return result;
        }

        static double ActiveShare(TownSiteYearResult row) => row.phase == TownAssetPhase.Operating || row.phase == TownAssetPhase.Establishing ? row.maturity * row.condition : 0;

        long ProgramDue(TownAsset asset, int year)
        {
            if(asset.cancelledYear>0)return 0;
            var a = Action(asset.siteId, asset.actionId);
            return year >= asset.startedYear && year < asset.startedYear + a.paymentYears ? a.annualPublicPaymentCents : 0;
        }
        bool IsOperating(TownAsset asset, int year)
        {
            if(asset.cancelledYear>0)return false;
            var a = Action(asset.siteId, asset.actionId); int start = asset.startedYear + a.constructionYears + asset.conversionDelayYears;
            return year >= start && year < start + a.operatingYears;
        }
        long OperatingDue(TownAsset asset, int year) => IsOperating(asset, year) ? Action(asset.siteId, asset.actionId).annualPublicOperatingCents : 0;
        long FutureCosts(TownAsset[] values, int fromYear)
        {
            long total = 0;
            foreach (var asset in values)
            {
                if(asset.cancelledYear>0)continue;
                var a = Action(asset.siteId, asset.actionId);
                int payments = Math.Max(0, asset.startedYear + a.paymentYears - Math.Max(fromYear, asset.startedYear));
                int start = asset.startedYear + a.constructionYears + asset.conversionDelayYears;
                int operations = Math.Max(0, start + a.operatingYears - Math.Max(fromYear, start));
                total = checked(total + a.annualPublicPaymentCents * payments + a.annualPublicOperatingCents * operations);
            }
            return total;
        }
        long NewCommitments(TownPlannedAction[] plan) => Sum(plan.Select(q => {
            var a = Action(q.siteId, q.actionId);
            return checked(a.upfrontPublicCostCents + a.annualPublicPaymentCents * a.paymentYears + a.annualPublicOperatingCents * a.operatingYears);
        }));
        static long Sum(IEnumerable<long> values) { long total = 0; foreach (long value in values) total = checked(total + value); return total; }

        void ValidateConsequences(TownYearConsequences result)
        {
            Require(result.schemaVersion == 1 && result.runId == RunId && result.requestId == pending.requestId
                && result.year == Year && result.inputHash == pending.inputHash, "Consequences do not match the frozen year request.");
            Require(result.effects != null, "Effects must be an explicit array (empty is valid).");
            if(definition.simulation!=null)Require(result.effects.Length==0,"Numeric-v4 effects are engine-owned; external effects cannot alter a saved driver.");
            if(PlayerRulesActive)Require(TownCopies.Hash(result.effects)==TownCopies.Hash(AnnualPlayerRules.Effects(pending)),"Regional effects are engine-owned; narration cannot change them.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var effect in result.effects)
            {
                Require(effect != null && !string.IsNullOrEmpty(effect.siteId) && sites.ContainsKey(effect.siteId) && ids.Add(effect.siteId), "Unknown or duplicate effect site.");
                Range(effect.productionFactor, 0, 2, "production factor"); Range(effect.conditionDelta, -.25, .15, "condition delta");
                Require(!string.IsNullOrWhiteSpace(effect.reason) && effect.reason.Length <= 1000 && !string.IsNullOrWhiteSpace(effect.sourceId), "Each effect needs a bounded reason and source/assumption ID.");
                Require(effect.sourceId == pending.driver.id || effect.sourceId == definition.id || effect.sourceId == definition.rubric.version
                    || definition.actions.Any(a => a.sourceId == effect.sourceId), "Unknown effect source/assumption ID.");
                var asset = AssetsForPlan(pending.actions).FirstOrDefault(a => a.siteId == effect.siteId);
                Require(effect.conditionDelta == 0 || (asset != null && IsOperating(asset,Year)), "Condition changes require a currently operational intervention, not an invented, unfinished or retired asset.");
            }
        }

        int planningYears;
        public int PlanningYears => planningYears == 0 ? definition.years : planningYears;
        public bool DecisionsFinished => Year > PlanningYears || (Year >= PlanningYears && Phase != TownPhase.Planning && Phase != TownPhase.Resolving);
        public bool AllPlotsAssigned => definition.actions.Length > 0 && definition.sites.Where(s => !s.protectedSite && definition.actions.Any(a => a.siteId == s.id && a.actionId != AnnualPlayerRules.CancelAction && a.actionId != "cultivate-margin"))
            .All(s => !queue.Any(q => q.siteId == s.id && q.actionId == AnnualPlayerRules.CancelAction) && (assets.Any(a => a.siteId == s.id && a.cancelledYear == 0) || queue.Any(q => q.siteId == s.id)));
        public long CurrentYearReservedCents => DueForPlan(queue);

        public void ConfigurePlanningYears(int years, int expectedRevision)
        {
            Planning(); Current(expectedRevision);
            Require(Year == 1 && history.Count == 0 && queue.Length == 0 && undo.Count == 0 && redo.Count == 0,
                "Choose the number of decision years before making a plan.");
            Require(years >= 1 && years <= definition.years, "Decision years must be within the ten-year horizon.");
            int value = years == definition.years ? 0 : years;
            if (value == planningYears) return;
            planningYears = value; Revision++;
        }

        // Removing a use never refunds an accepted payment. Remaining program payments
        // are settled, and removal/site preparation costs 10% of the old public price,
        // with a $25K minimum. These are disclosed authored game assumptions.
        public long ConversionCost(string siteId)
        {
            var old = assets.FirstOrDefault(a => a.siteId == siteId && a.cancelledYear == 0);
            if (old == null) return 0;
            var recipe = Action(old.siteId, old.actionId);
            int remaining = Math.Max(0, old.startedYear + recipe.paymentYears - Math.Max(Year, old.startedYear));
            return checked(Math.Max(2500000L, recipe.upfrontPublicCostCents / 10) + recipe.annualPublicPaymentCents * remaining);
        }

        TownAsset[] AssetsForPlan(TownPlannedAction[] plan)
        {
            var result = TownCopies.Copy(assets);
            foreach (var q in plan.Where(q => q.actionId == AnnualPlayerRules.CancelAction))
            {
                var cancelled = result.Single(a => a.siteId == q.siteId && a.cancelledYear == 0);
                var previous = history.LastOrDefault()?.outcomes.sites.FirstOrDefault(r => r.siteId == q.siteId);
                cancelled.cancelledYear = Year;
                cancelled.clearedVegetatedFraction = Math.Min(.25, previous == null ? sites[q.siteId].baselineVegetatedFraction : previous.vegetatedAreaM2/sites[q.siteId].areaM2);
            }
            var builds = plan.Where(q => q.actionId != AnnualPlayerRules.CancelAction).ToArray();
            return result.Where(a => !builds.Any(q => q.siteId == a.siteId)).Concat(builds.Select(q => {
                var old = assets.FirstOrDefault(a => a.siteId == q.siteId && a.cancelledYear == 0);
                var recipe = Action(q.siteId, q.actionId);
                var previous = history.LastOrDefault()?.outcomes.sites.FirstOrDefault(r => r.siteId == q.siteId);
                return new TownAsset { siteId=q.siteId, actionId=q.actionId, startedYear=Year, condition=1,
                    previousActionId=old?.actionId, conversionDelayYears=old==null?0:Math.Max(0,1-recipe.constructionYears),
                    conversionOpeningVegetation=old==null?0:(previous?.vegetatedAreaM2??0)/sites[q.siteId].areaM2 };
            })).ToArray();
        }

        long DueForPlan(TownPlannedAction[] plan)
        {
            int budgetYear = Phase == TownPhase.Reviewing || Phase == TownPhase.Completed ? Year + 1 : Year;
            if (budgetYear > definition.years) return ArrearsCents;
            var next = AssetsForPlan(plan);
            long upfront = Sum(plan.Where(q => q.actionId != AnnualPlayerRules.CancelAction).Select(q => checked(Action(q.siteId,q.actionId).upfrontPublicCostCents + ConversionCost(q.siteId))));
            long refunds = PlayerRulesActive ? Sum(plan.Where(q => q.actionId == AnnualPlayerRules.CancelAction).Select(q => CancellationRefund(q.siteId))) : 0;
            return checked(ArrearsCents + upfront - refunds + Sum(next.Select(a => checked(ProgramDue(a,budgetYear) + OperatingDue(a,budgetYear)))));
        }

        public AnnualTownSnapshot Snapshot() => new AnnualTownSnapshot {
            schemaVersion = definition.simulation == null ? 1 : 2,
            runId = RunId, definitionHash = DefinitionHash, phase = Phase, year = Year, revision = Revision, planningYears = planningYears,
            cashCents = CashCents, arrearsCents = ArrearsCents, queue = Queue, assets = Assets,
            undo = TownCopies.Copy(undo.Reverse().ToArray()), redo = TownCopies.Copy(redo.Reverse().ToArray()), history = History, pendingRequest = PendingRequest,
            simulation=TownCopies.Copy(numericState)
        };

        // Replays accepted evidence, never calls the provider. Derived scores/ledgers in a save are not trusted.
        public static AnnualTown Restore(AnnualTownDefinition rules, AnnualTownSnapshot save)
        {
            Require(save != null && save.schemaVersion == (rules?.schemaVersion==3?2:1), "Unsupported annual save.");
            save = TownCopies.Copy(save);
            // Unity's inline serializer may expand an absent DTO into empty fields.
            if (save.pendingRequest != null && string.IsNullOrEmpty(save.pendingRequest.requestId)) save.pendingRequest = null;
            var town = new AnnualTown(rules, save.runId);
            Require(save.definitionHash == town.DefinitionHash && save.history != null && save.queue != null && save.assets != null && save.undo != null, "Save belongs to different rules/data, or is incomplete.");
            Require(save.history.Length <= rules.years, "Too many saved years.");
            Require(save.planningYears >= 0 && save.planningYears <= rules.years, "Invalid decision horizon.");
            town.planningYears = save.planningYears;
            if(rules.schemaVersion==3)AnnualSimulation.ValidateState(rules,save.simulation,save.history.Length,save.arrearsCents);
            foreach (var record in save.history)
            {
                Require(record != null && record.acceptedRequest != null && record.acceptedConsequences != null, "Missing committed annual evidence.");
                if (town.Phase == TownPhase.Reviewing) town.ContinuePlanning(town.Revision);
                var req = record.acceptedRequest;
                Require(req.year == town.Year && req.runId == town.RunId && req.revision >= town.Revision, "Saved years are out of order.");
                Require(req.actions != null, "Missing submitted action list.");
                foreach (var q in req.actions) { Require(q != null, "Invalid submitted action."); town.QueueAction(q.siteId, q.actionId, town.Revision); }
                Require(req.revision >= town.Revision, "Saved revision predates its actions.");
                town.Revision = req.revision;
                var regenerated = town.BeginResolution(req.requestId, town.Revision);
                Require(TownCopies.Hash(regenerated) == TownCopies.Hash(req), "Committed input hash/state does not match saved history.");
                var replayed=town.CompleteResolution(record.acceptedConsequences);
                if(rules.schemaVersion==3||town.PlayerRulesActive)Require(TownCopies.Hash(replayed)==TownCopies.Hash(record),"Saved report differs from its committed replay.");
            }
            if (town.Phase == TownPhase.Reviewing && (save.phase == TownPhase.Planning || save.phase == TownPhase.Resolving)) town.ContinuePlanning(town.Revision);
            Require(save.year == town.Year && save.revision >= town.Revision && save.cashCents == town.CashCents
                && save.arrearsCents == town.ArrearsCents && TownCopies.Hash(save.assets) == TownCopies.Hash(town.assets), "Saved state disagrees with committed history.");
            Require(TownCopies.Hash(save.simulation)==TownCopies.Hash(town.numericState),"Saved site condition/repair state differs from committed history.");
            town.Revision = save.revision;
            if (save.phase == TownPhase.Planning || save.phase == TownPhase.Resolving)
            {
                Require(town.Phase == TownPhase.Planning, "Invalid saved phase.");
                town.ValidateSavedQueue(save.queue);
                town.queue = TownCopies.Copy(save.queue); town.undo.Clear();
                foreach (var frame in save.undo) { Require(frame != null && frame.actions != null, "Invalid undo frame."); town.ValidateSavedQueue(frame.actions); town.undo.Push(TownCopies.Copy(frame)); }
                foreach (var frame in save.redo ?? Array.Empty<TownPlanSnapshot>()) { Require(frame != null && frame.actions != null, "Invalid redo frame."); town.ValidateSavedQueue(frame.actions); town.redo.Push(TownCopies.Copy(frame)); }
                if (save.phase == TownPhase.Resolving)
                {
                    Require(save.pendingRequest != null, "Missing frozen request.");
                    var restoredRequest = town.BeginResolution(save.pendingRequest.requestId, town.Revision);
                    Require(TownCopies.Hash(restoredRequest) == TownCopies.Hash(save.pendingRequest), "Frozen request has changed.");
                }
                else Require(save.pendingRequest == null, "Planning cannot contain a resolving request.");
            }
            else Require(save.phase == town.Phase && save.queue.Length == 0 && save.undo.Length == 0 && (save.redo == null || save.redo.Length == 0) && save.pendingRequest == null, "Review/completed saves cannot contain a new plan.");
            return town;
        }
        void ValidateSavedQueue(TownPlannedAction[] plan)
        {
            var used = new HashSet<string>(StringComparer.Ordinal);
            foreach (var q in plan) { Require(q != null && used.Add(q.siteId), "Invalid saved plan."); ProposedQueue(q.siteId, q.actionId); }
            Require(plan.Length == 0 || PlayerRulesActive && plan.All(q => q.actionId == AnnualPlayerRules.CancelAction) || checked(CashCents - DueForPlan(plan)) >= 0, "Saved plan is unaffordable.");
        }

        internal static void Require(bool valid, string message) { if (!valid) throw new ArgumentException(message); }
        static void Range(double value, double min, double max, string name) => Require(!double.IsNaN(value) && !double.IsInfinity(value) && value >= min && value <= max, "Invalid " + name + ".");
        static double Clamp01(double value) => Math.Max(0, Math.Min(1, value));
        public static void ValidateDefinition(AnnualTownDefinition d)
        {
            d=TownCopies.Copy(d);
            Require(d != null && (d.schemaVersion == 1 || d.schemaVersion == 2 || d.schemaVersion == 3) && !string.IsNullOrWhiteSpace(d.id) && !string.IsNullOrWhiteSpace(d.version) && !string.IsNullOrWhiteSpace(d.worldPackId), "Versioned town definition is required.");
            Require(d.currency == "USD" && d.priceYear >= 2000 && d.priceYear <= 2200 && d.years >= 1 && d.years <= 100, "Invalid currency/year/horizon.");
            Money(d.startingCashCents);
            if(d.playerRules!=null&&!TownCopies.EmptyExtension(d.playerRules))
            {
                Require(AnnualPlayerRules.Enabled(d)&&d.version=="5.0.0"&&d.schemaVersion==2&&d.simulation==null,"Unsupported player-rule version.");
                Require(!string.IsNullOrWhiteSpace(d.playerRules.scenarioSeed)&&!string.IsNullOrWhiteSpace(d.playerRules.disclosure),"Player rules require a saved seed and disclosure.");
                Require(d.playerRules.hazards!=null&&d.playerRules.hazards.Length>0,"Regional profiles are required.");
                foreach(var h in d.playerRules.hazards){Require(h!=null&&!string.IsNullOrWhiteSpace(h.id)&&!string.IsNullOrWhiteSpace(h.evidence),"Missing inferred hazard evidence.");Range(h.annualChance,0,1,"hazard chance");Range(h.outputFactor,0,1,"hazard output");Range(h.affectedSiteChance,0,1,"hazard exposure");Range(h.conditionLoss,0,.25,"hazard condition");}
            }
            if (d.schemaVersion >= 2) AnnualEconomy.Validate(d);
            if (d.schemaVersion == 3) AnnualSimulation.Validate(d);
            else Require(d.simulation==null,"Legacy rules cannot contain numeric-v4 simulation data.");
            Require(d.appropriationsCents != null && d.appropriationsCents.Length == d.years, "Provide one explicit appropriation per year.");
            foreach (var amount in d.appropriationsCents) Money(amount);
            Require(d.sites != null && d.sites.Length > 0 && d.actions != null && d.drivers != null && d.drivers.Length == d.years && d.rubric != null, "Incomplete definition.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var s in d.sites) { Require(s != null && !string.IsNullOrWhiteSpace(s.id) && !s.id.Contains(":") && ids.Add(s.id), "Duplicate/invalid town site."); Range(s.areaM2, .0001, 1e12, "site area"); Range(s.baselineProductionUnits, 0, 1e12, "baseline production"); Range(s.baselineServiceUnits, 0, 1e12, "baseline service"); Range(s.baselineVegetatedFraction, 0, 1, "baseline cover"); }
            var offers = new HashSet<string>(StringComparer.Ordinal);
            foreach (var a in d.actions)
            {
                Require(a != null && ids.Contains(a.siteId) && !d.sites.First(s => s.id == a.siteId).protectedSite && !string.IsNullOrWhiteSpace(a.actionId) && !a.actionId.Contains(":") && offers.Add(Key(a.siteId, a.actionId)), "Duplicate, protected or invalid action offer.");
                Require(!string.IsNullOrWhiteSpace(a.sourceId) && !string.IsNullOrWhiteSpace(a.evidenceLevel) && !string.IsNullOrWhiteSpace(a.ownerOperator) && !string.IsNullOrWhiteSpace(a.contributionType) && !string.IsNullOrWhiteSpace(a.fundedUnit), "Action requires source, evidence, contribution, operator and units.");
                Range(a.fundedQuantity, .000001, 1e12, "funded quantity");
                Money(a.upfrontPublicCostCents); Money(a.annualPublicPaymentCents); Money(a.annualPublicOperatingCents); Money(a.annualPublicReceiptsCents); Money(a.totalProjectCostCents);
                Require(a.paymentYears >= 0 && a.paymentYears <= 100 && a.operatingYears >= 1 && a.operatingYears <= 100 && a.constructionYears >= 0 && a.constructionYears <= 100 && a.establishmentYears >= 1 && a.establishmentYears <= 100, "Invalid action lifecycle/term.");
                Require(a.annualPublicPaymentCents == 0 || a.paymentYears > 0, "Annual payment needs a term.");
                Require(!a.totalProjectCostKnown || a.totalProjectCostCents >= a.upfrontPublicCostCents, "Public upfront contribution exceeds stated total project cost.");
                Range(a.productionUnits, 0, 1e12, "production"); Range(a.serviceUnits, 0, 1e12, "services"); Range(a.vegetatedFraction, 0, 1, "cover"); Range(a.retentionCapacityM3, 0, 1e12, "retention capacity");
            }
            for (int i = 0; i < d.drivers.Length; i++) { var e = d.drivers[i]; Require(e != null && e.year == i + 1 && !string.IsNullOrWhiteSpace(e.id) && !string.IsNullOrWhiteSpace(e.evidenceLevel), "Drivers must explicitly cover each year in order."); Range(e.productionFactor, 0, 2, "annual driver factor"); Money(e.unexpectedPublicRepairCents); }
            if(AnnualPlayerRules.Enabled(d))AnnualPlayerRules.ValidateEvents(d);
            var r = d.rubric; Require(!string.IsNullOrWhiteSpace(r.version) && !string.IsNullOrWhiteSpace(r.explanation), "Scores need a versioned explanation.");
            Range(r.productionTargetUnits, .000001, 1e12, "production target"); Range(r.serviceTargetUnits, .000001, 1e12, "service target"); Range(r.retentionTargetM3, .000001, 1e12, "retention target");
            Range(r.economicProductionWeight, 0, 1, "economic weight"); Range(r.socialServiceWeight, 0, 1, "social weight"); Range(r.environmentalVegetationWeight, 0, 1, "environmental weight");
        }
        static void Money(long value) => Require(value >= 0 && value <= 1000000000000000L, "Money must be nonnegative bounded integer cents.");
    }
}
