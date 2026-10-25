using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    /// <summary>Authoritative annual state. No Unity, network, clock, or random dependencies.</summary>
    public sealed class AnnualTown
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
        public long AvailableCents => checked(CashCents - CommittedCents - QueuedCommitmentCents);
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
        }

        static string Key(string siteId, string actionId) => siteId + ":" + actionId;
        TownActionDefinition Action(string siteId, string actionId)
        {
            Require(actions.TryGetValue(Key(siteId, actionId), out var action), "Unknown or ineligible site/action.");
            return action;
        }
        void Planning() { if (Phase != TownPhase.Planning) throw new InvalidOperationException("This year is not open for planning."); }
        void Current(int expectedRevision) { if (expectedRevision != Revision) throw new InvalidOperationException("Stale town revision. Refresh the state."); }

        // Previews and queue changes reserve the full public commitment, not future appropriations.
        public long Preview(string siteId, string actionId)
        {
            Planning();
            var next = ProposedQueue(siteId, actionId);
            return checked(CashCents - CommittedCents - NewCommitments(next));
        }
        TownPlannedAction[] ProposedQueue(string siteId, string actionId)
        {
            Action(siteId, actionId);
            Require(!assets.Any(a => a.siteId == siteId), "This site is already committed. Redevelopment is outside this foundation.");
            return queue.Where(q => q.siteId != siteId).Concat(new[] { new TownPlannedAction { siteId = siteId, actionId = actionId } }).ToArray();
        }
        public void QueueAction(string siteId, string actionId, int expectedRevision)
        {
            Planning(); Current(expectedRevision);
            var next = ProposedQueue(siteId, actionId);
            Require(checked(CashCents - CommittedCents - NewCommitments(next)) >= 0, "Insufficient uncommitted public money for the full commitment.");
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
            Require(source != destination && !queue.Any(q => q.siteId == destination) && !assets.Any(a => a.siteId == destination), "Choose an unoccupied destination.");
            Action(destination, proposal.actionId);
            return queue.Where(q => q.siteId != source).Concat(new[] { new TownPlannedAction { siteId = destination, actionId = proposal.actionId } }).ToArray();
        }
        public long PreviewMove(string source, string destination)
        { Planning(); return checked(CashCents - CommittedCents - NewCommitments(MovedQueue(source, destination))); }
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
                runId = RunId, requestId = requestId, definitionHash = DefinitionHash, year = Year, revision = Revision,
                actions = TownCopies.Copy(queue), assets = TownCopies.Copy(assets), driver = TownCopies.Copy(definition.drivers[Year - 1])
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
            var nextAssets = TownCopies.Copy(assets).Concat(pending.actions.Select(q => new TownAsset {
                siteId = q.siteId, actionId = q.actionId, startedYear = Year, condition = 1
            })).ToArray();
            var ledger = new TownYearLedger { currency = definition.currency, priceYear = definition.priceYear,
                openingCashCents = CashCents, openingArrearsCents = ArrearsCents };
            long cash = CashCents;
            long Pay(long due) { long paid = Math.Min(cash, due); cash = checked(cash - paid); return paid; }
            long upfront = Sum(pending.actions.Select(q => Action(q.siteId, q.actionId).upfrontPublicCostCents));
            Require(upfront <= cash, "The committed plan is not affordable.");
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
                var effect = consequences.effects.FirstOrDefault(e => e.siteId == asset.siteId);
                if (effect != null)
                {
                    Require(effect.conditionDelta <= 0 || funded[asset.siteId], "An unfunded intervention cannot gain condition from a provider.");
                    asset.condition = Clamp01(asset.condition + effect.conditionDelta);
                }
                if (IsOperating(asset, Year) && funded[asset.siteId])
                    ledger.publicReceiptsCents = checked(ledger.publicReceiptsCents + recipe.annualPublicReceiptsCents);
            }
            ledger.repairDueCents = pending.driver.unexpectedPublicRepairCents;
            ledger.repairPaidCents = Pay(ledger.repairDueCents);
            ledger.closingArrearsCents = checked(ArrearsCents - ledger.arrearsPaidCents + ledger.programDueCents - ledger.programPaidCents
                + ledger.operatingDueCents - ledger.operatingPaidCents + ledger.repairDueCents - ledger.repairPaidCents);
            var outcomes = EvaluateOutcomes(nextAssets, funded, consequences, ledger);
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
                acceptedRequest = TownCopies.Copy(pending), acceptedConsequences = TownCopies.Copy(consequences) };

            assets = nextAssets; CashCents = ledger.closingCashCents; ArrearsCents = ledger.closingArrearsCents;
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
                if (asset != null)
                {
                    var recipe = Action(asset.siteId, asset.actionId);
                    int operationYear = asset.startedYear + recipe.constructionYears;
                    row.maturity = Clamp01((Year - operationYear + 1.0) / recipe.establishmentYears);
                    bool active = IsOperating(asset, Year) && funded[asset.siteId];
                    row.phase = Year < operationYear ? TownAssetPhase.Building : Year >= operationYear + recipe.operatingYears ? TownAssetPhase.Retired : !active ? TownAssetPhase.Suspended
                        : row.maturity < 1 ? TownAssetPhase.Establishing : TownAssetPhase.Operating;
                    row.productionUnits = active ? recipe.productionUnits * row.maturity * row.condition : 0;
                    row.serviceUnits = active ? recipe.serviceUnits * row.maturity * row.condition : 0;
                    row.vegetatedAreaM2 = site.areaM2 * (site.baselineVegetatedFraction
                        + (recipe.vegetatedFraction - site.baselineVegetatedFraction) * row.maturity * row.condition);
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
            var a = Action(asset.siteId, asset.actionId);
            return year >= asset.startedYear && year < asset.startedYear + a.paymentYears ? a.annualPublicPaymentCents : 0;
        }
        bool IsOperating(TownAsset asset, int year)
        {
            var a = Action(asset.siteId, asset.actionId); int start = asset.startedYear + a.constructionYears;
            return year >= start && year < start + a.operatingYears;
        }
        long OperatingDue(TownAsset asset, int year) => IsOperating(asset, year) ? Action(asset.siteId, asset.actionId).annualPublicOperatingCents : 0;
        long FutureCosts(TownAsset[] values, int fromYear)
        {
            long total = 0;
            foreach (var asset in values)
            {
                var a = Action(asset.siteId, asset.actionId);
                int payments = Math.Max(0, asset.startedYear + a.paymentYears - Math.Max(fromYear, asset.startedYear));
                int start = asset.startedYear + a.constructionYears;
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
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var effect in result.effects)
            {
                Require(effect != null && !string.IsNullOrEmpty(effect.siteId) && sites.ContainsKey(effect.siteId) && ids.Add(effect.siteId), "Unknown or duplicate effect site.");
                Range(effect.productionFactor, 0, 2, "production factor"); Range(effect.conditionDelta, -.25, .15, "condition delta");
                Require(!string.IsNullOrWhiteSpace(effect.reason) && effect.reason.Length <= 1000 && !string.IsNullOrWhiteSpace(effect.sourceId), "Each effect needs a bounded reason and source/assumption ID.");
                Require(effect.sourceId == pending.driver.id || effect.sourceId == definition.id || effect.sourceId == definition.rubric.version
                    || definition.actions.Any(a => a.sourceId == effect.sourceId), "Unknown effect source/assumption ID.");
                var asset = assets.FirstOrDefault(a => a.siteId == effect.siteId);
                var planned = pending.actions.FirstOrDefault(a => a.siteId == effect.siteId);
                int operationYear = asset != null ? asset.startedYear + Action(asset.siteId, asset.actionId).constructionYears
                    : planned != null ? Year + Action(planned.siteId, planned.actionId).constructionYears : int.MaxValue;
                int operatingYears = asset != null ? Action(asset.siteId, asset.actionId).operatingYears
                    : planned != null ? Action(planned.siteId, planned.actionId).operatingYears : 0;
                Require(effect.conditionDelta == 0 || (Year >= operationYear && Year < (long)operationYear + operatingYears), "Condition changes require a currently operational intervention, not an invented, unfinished or retired asset.");
            }
        }

        public AnnualTownSnapshot Snapshot() => new AnnualTownSnapshot {
            runId = RunId, definitionHash = DefinitionHash, phase = Phase, year = Year, revision = Revision,
            cashCents = CashCents, arrearsCents = ArrearsCents, queue = Queue, assets = Assets,
            undo = TownCopies.Copy(undo.Reverse().ToArray()), redo = TownCopies.Copy(redo.Reverse().ToArray()), history = History, pendingRequest = PendingRequest
        };

        // Replays accepted evidence, never calls the provider. Derived scores/ledgers in a save are not trusted.
        public static AnnualTown Restore(AnnualTownDefinition rules, AnnualTownSnapshot save)
        {
            Require(save != null && save.schemaVersion == 1, "Unsupported annual save.");
            save = TownCopies.Copy(save);
            // Unity's inline serializer may expand an absent DTO into empty fields.
            if (save.pendingRequest != null && string.IsNullOrEmpty(save.pendingRequest.requestId)) save.pendingRequest = null;
            var town = new AnnualTown(rules, save.runId);
            Require(save.definitionHash == town.DefinitionHash && save.history != null && save.queue != null && save.assets != null && save.undo != null, "Save belongs to different rules/data, or is incomplete.");
            Require(save.history.Length <= rules.years, "Too many saved years.");
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
                town.CompleteResolution(record.acceptedConsequences);
            }
            if (town.Phase == TownPhase.Reviewing && (save.phase == TownPhase.Planning || save.phase == TownPhase.Resolving)) town.ContinuePlanning(town.Revision);
            Require(save.year == town.Year && save.revision >= town.Revision && save.cashCents == town.CashCents
                && save.arrearsCents == town.ArrearsCents && TownCopies.Hash(save.assets) == TownCopies.Hash(town.assets), "Saved state disagrees with committed history.");
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
            foreach (var q in plan) { Require(q != null && used.Add(q.siteId) && !assets.Any(a => a.siteId == q.siteId), "Invalid saved plan."); Action(q.siteId, q.actionId); }
            Require(plan.Length == 0 || checked(CashCents - CommittedCents - NewCommitments(plan)) >= 0, "Saved plan is unaffordable.");
        }

        internal static void Require(bool valid, string message) { if (!valid) throw new ArgumentException(message); }
        static void Range(double value, double min, double max, string name) => Require(!double.IsNaN(value) && !double.IsInfinity(value) && value >= min && value <= max, "Invalid " + name + ".");
        static double Clamp01(double value) => Math.Max(0, Math.Min(1, value));
        public static void ValidateDefinition(AnnualTownDefinition d)
        {
            Require(d != null && (d.schemaVersion == 1 || d.schemaVersion == 2) && !string.IsNullOrWhiteSpace(d.id) && !string.IsNullOrWhiteSpace(d.version) && !string.IsNullOrWhiteSpace(d.worldPackId), "Versioned town definition is required.");
            Require(d.currency == "USD" && d.priceYear >= 2000 && d.priceYear <= 2200 && d.years >= 1 && d.years <= 100, "Invalid currency/year/horizon.");
            Money(d.startingCashCents);
            if (d.schemaVersion == 2) AnnualEconomy.Validate(d);
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
            var r = d.rubric; Require(!string.IsNullOrWhiteSpace(r.version) && !string.IsNullOrWhiteSpace(r.explanation), "Scores need a versioned explanation.");
            Range(r.productionTargetUnits, .000001, 1e12, "production target"); Range(r.serviceTargetUnits, .000001, 1e12, "service target"); Range(r.retentionTargetM3, .000001, 1e12, "retention target");
            Range(r.economicProductionWeight, 0, 1, "economic weight"); Range(r.socialServiceWeight, 0, 1, "social weight"); Range(r.environmentalVegetationWeight, 0, 1, "environmental weight");
        }
        static void Money(long value) => Require(value >= 0 && value <= 1000000000000000L, "Money must be nonnegative bounded integer cents.");
    }
}
