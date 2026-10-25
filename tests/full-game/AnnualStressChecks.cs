using System.Text.Json;
using Terra.Contracts;
using Terra.Core;

// Real-map strategy and interruption checks. These are model evidence, not browser or novice tests.
static class AnnualStressChecks
{
    static readonly JsonSerializerOptions Json = new() { IncludeFields = true, WriteIndented = true };
    static string Encode<T>(T value) => JsonSerializer.Serialize(value, Json);
    static T Copy<T>(T value) => JsonSerializer.Deserialize<T>(Encode(value), Json);

    public static int Run(AnnualTownDefinition definition, string evidence)
    {
        int checks = 0;
        void Check(bool value, string message)
        {
            checks++;
            if (!value) throw new Exception(message);
        }
        var provider = new AuthoredAnnualConsequenceProvider();
        double[] Scores(AnnualTown town)
        {
            var result = town.History.Last().outcomes;
            return new[] { result.economicScore, result.socialScore, result.environmentalScore };
        }
        var rubric = definition.economy;
        var targets = new[] { rubric.finalEconomicTarget, rubric.finalSocialTarget, rubric.finalEnvironmentalTarget };
        int TargetsMet(AnnualTown town) => Scores(town).Zip(targets, (score, target) => score >= target ? 1 : 0).Sum();
        var baseline = new AnnualTown(definition, "stress-no-action");
        AnnualTownDemo.Finish(baseline, provider);
        var strategies = new List<object>();

        // Fill every affordable eligible site from one family, then repeat under different site
        // priorities. Also test the three regenerative recipes as one choice pool; their
        // existing recipe family IDs differ. Wind and solar already share one family ID.
        foreach (var family in definition.actions.Select(a => a.services.familyId).Append("all-regenerative-variants").Distinct().OrderBy(x => x, StringComparer.Ordinal))
        foreach (string order in new[] { "site-id", "smallest-first", "largest-first" })
        {
            var town = new AnnualTown(definition, "stress-" + family + "-" + order);
            var offers = definition.actions.Where(a => family == "all-regenerative-variants"
                ? new[] { "cover-crop", "cultivate-margin", "conservation-rotation" }.Contains(a.actionId)
                : a.services.familyId == family);
            offers = order switch {
                "smallest-first" => offers.OrderBy(a => definition.sites.First(s => s.id == a.siteId).areaM2),
                "largest-first" => offers.OrderByDescending(a => definition.sites.First(s => s.id == a.siteId).areaM2),
                _ => offers.OrderBy(a => a.siteId, StringComparer.Ordinal)
            };
            var ordered = offers.ToArray();
            for (int year = 1; year <= definition.years; year++)
            {
                if (town.Phase == TownPhase.Reviewing) town.ContinuePlanning(town.Revision);
                foreach (var offer in ordered)
                {
                    if (town.Assets.Any(a => a.siteId == offer.siteId) || town.Queue.Any(q => q.siteId == offer.siteId)) continue;
                    if (town.Preview(offer.siteId, offer.actionId) >= 0) town.QueueAction(offer.siteId, offer.actionId, town.Revision);
                }
                var report = AnnualTownDemo.Advance(town, town.RunId + "/year-" + year, provider);
                var ledger=report.ledger;
                long expectedArrears=Math.Max(0,ledger.openingArrearsCents+ledger.programDueCents+ledger.operatingDueCents+ledger.repairDueCents-(ledger.openingCashCents-ledger.upfrontPaidCents));
                Check(ledger.closingArrearsCents==expectedArrears, "Current-year planning must retain unfunded later obligations: " + town.RunId);
                Check(report.ledger.closingCashCents >= 0, "Negative treasury: " + town.RunId);
                var economy = report.outcomes.economy;
                Check(economy.occupiedHomes <= rubric.householdDemand && economy.recreation <= rubric.recreationTarget
                    && economy.localServices <= rubric.localServicesTarget, "Demand saturation failed: " + town.RunId);
            }
            Check(TargetsMet(town) < 3, "A repeated single family meets all objectives: inspect balance for " + town.RunId);
            Check(town.Assets.Length == town.Assets.Select(a => a.siteId).Distinct().Count(), "Site counted twice");
            Check(town.History.Zip(baseline.History, (ours, reference) => Encode(ours.acceptedRequest.driver) == Encode(reference.acceptedRequest.driver)
                && ours.ledger.appropriationCents == reference.ledger.appropriationCents).All(equal => equal), "Comparison changed shared conditions");
            strategies.Add(new { family, order, projects = town.Assets.Length, scores = Scores(town), targetsMet = TargetsMet(town),
                cashCents = town.CashCents, commitmentsCents = town.CommittedCents,
                actions = town.History.Select(r => new { r.year, actions = r.acceptedRequest.actions, closingCashCents = r.ledger.closingCashCents }) });
        }

        // Fail each fast-forward step before acceptance, including Year 10. JSON reload must
        // retain the identical request; retry must reproduce the uninterrupted entire snapshot.
        var seed = new AnnualTown(definition, "interrupted-future");
        var home = definition.actions.First(a => a.actionId == "high-density-housing");
        seed.QueueAction(home.siteId, home.actionId, seed.Revision);
        var uninterrupted = AnnualTown.Restore(definition, seed.Snapshot());
        AnnualTownDemo.Finish(uninterrupted, provider);
        var interruptions = new List<object>();
        for (int year = 1; year <= definition.years; year++)
        {
            var interrupted = AnnualTown.Restore(definition, seed.Snapshot());
            var failing = new FailAtYear(year);
            bool failed = false;
            try { AnnualTownDemo.Finish(interrupted, failing); }
            catch (IOException) { failed = true; }
            Check(failed && interrupted.Phase == TownPhase.Resolving && interrupted.Year == year, "Failure must retain resolving year " + year);
            Check(interrupted.History.Length == year - 1, "Failed year was accepted");
            var frozen = Encode(interrupted.PendingRequest);
            var snapshot = Encode(interrupted.Snapshot());
            var invalid = provider.Resolve(interrupted.PendingRequest);
            invalid.inputHash = "mismatched-input";
            bool rejected = false;
            try { interrupted.CompleteResolution(invalid); }
            catch (ArgumentException) { rejected = true; }
            Check(rejected && Encode(interrupted.Snapshot()) == snapshot, "Invalid response changed frozen state");
            interrupted = AnnualTown.Restore(definition, Copy(interrupted.Snapshot()));
            Check(Encode(interrupted.PendingRequest) == frozen, "Reload changed the frozen request");
            AnnualTownDemo.Finish(interrupted, provider);
            Check(Encode(interrupted.Snapshot()) == Encode(uninterrupted.Snapshot()), "Retry differs from uninterrupted future at year " + year);
            var final = Encode(interrupted.Snapshot());
            Check(AnnualTownDemo.Finish(interrupted, provider).Length == 0 && Encode(interrupted.Snapshot()) == final, "Repeated finish changed completed history");
            // Re-delivery is harmless even after the entire decade has completed.
            foreach (var report in interrupted.History)
                interrupted.CompleteResolution(report.acceptedConsequences);
            Check(Encode(interrupted.Snapshot()) == final, "Duplicate delivery spent money or changed the ending");
            interruptions.Add(new { failedYear = year, acceptedYearsBeforeFailure = year - 1, frozenRequestPreserved = true, finalSnapshotIdentical = true });
        }

        // Known future upkeep remains reserved past the ending, including a project queued in
        // Year 10 that has not opened. Expected dues are calculated from the named recipe terms.
        var late = new AnnualTown(definition, "late-housing");
        while (late.Year < definition.years)
        {
            AnnualTownDemo.Advance(late, "late-year-" + late.Year, provider);
            late.ContinuePlanning(late.Revision);
        }
        late.QueueAction(home.siteId, home.actionId, late.Revision);
        var lateEnd = AnnualTownDemo.Advance(late, "late-final", provider);
        Check(lateEnd.outcomes.sites.First(s => s.siteId == home.siteId).phase == TownAssetPhase.Building, "Late housing should still be building");
        Check(lateEnd.ledger.publicReceiptsCents == 0, "Unopened housing paid tax");
        long unpaidTerms = home.annualPublicOperatingCents * home.operatingYears
            + home.annualPublicPaymentCents * Math.Max(0, home.paymentYears - 1);
        Check(lateEnd.ledger.futureCommitmentsCents == unpaidTerms && late.CommittedCents == unpaidTerms, "Ending hid beyond-horizon commitments");

        // Unexpected repair overwhelms reserved money. Waiting must remain possible, preserve
        // debt and end truthfully; useful affordable choices must return when funds recover.
        var stressedDefinition = Copy(definition);
        stressedDefinition.drivers[1].unexpectedPublicRepairCents = 2000000000;
        var stressed = new AnnualTown(stressedDefinition, "maintenance-recovery");
        var farm = stressedDefinition.actions.First(a => a.actionId == "cultivate-standard");
        stressed.QueueAction(farm.siteId, farm.actionId, stressed.Revision);
        var recovery = new List<object>();
        bool sawSuspended = false, sawRecovery = false;
        for (int year = 1; year <= definition.years; year++)
        {
            if (stressed.Phase == TownPhase.Reviewing) stressed.ContinuePlanning(stressed.Revision);
            var affordable = stressedDefinition.actions.Where(a => !stressed.Assets.Any(asset => asset.siteId == a.siteId))
                .Where(a => stressed.Preview(a.siteId, a.actionId) >= 0).ToArray();
            var report = AnnualTownDemo.Advance(stressed, "recovery-" + year, provider);
            var phase = report.outcomes.sites.First(s => s.siteId == farm.siteId).phase;
            sawSuspended |= phase == TownAssetPhase.Suspended;
            sawRecovery |= sawSuspended && affordable.Length > 0;
            long unpaid = report.ledger.openingArrearsCents - report.ledger.arrearsPaidCents
                + report.ledger.operatingDueCents - report.ledger.operatingPaidCents
                + report.ledger.programDueCents - report.ledger.programPaidCents
                + report.ledger.repairDueCents - report.ledger.repairPaidCents;
            Check(report.ledger.closingArrearsCents == unpaid, "Waiting erased unpaid debt");
            recovery.Add(new { year, phase = phase.ToString(), affordableOffers = affordable.Length, cashCents = stressed.CashCents, arrearsCents = stressed.ArrearsCents });
        }
        Check(sawSuspended, "Stress fixture did not suspend operation");
        // Recovery can extend past Year 10; this fixture must not invent an extra turn to clear it.
        Check(stressed.Phase == TownPhase.Completed && stressed.History.Length == definition.years, "Shortfall blocked ending");
        var stressedRestored = AnnualTown.Restore(stressedDefinition, Copy(stressed.Snapshot()));
        Check(Encode(stressedRestored.Snapshot()) == Encode(stressed.Snapshot()), "Shortfall ending did not survive reload");

        // A smaller (still deliberately extreme) repair also proves the other side of the
        // recovery policy: paying arrears eventually restores useful discretionary choices.
        var recoverableDefinition = Copy(definition);
        recoverableDefinition.drivers[1].unexpectedPublicRepairCents = 1200000000;
        var recoverable = new AnnualTown(recoverableDefinition, "recoverable-shortfall");
        recoverable.QueueAction(farm.siteId, farm.actionId, 0);
        int firstSuspendedYear = 0, firstAffordableRecoveryYear = 0;
        string recoveryAction = null, recoverySite = null;
        for (int year = 1; year <= definition.years; year++)
        {
            if (recoverable.Phase == TownPhase.Reviewing) recoverable.ContinuePlanning(recoverable.Revision);
            if (firstSuspendedYear > 0 && firstAffordableRecoveryYear == 0)
            {
                var choice = recoverableDefinition.actions.FirstOrDefault(a => a.actionId == "public-park"
                    && !recoverable.Assets.Any(asset => asset.siteId == a.siteId) && recoverable.Preview(a.siteId, a.actionId) >= 0);
                if (choice != null)
                {
                    recoveryAction = choice.actionId; recoverySite = choice.siteId;
                    recoverable.QueueAction(choice.siteId, choice.actionId, recoverable.Revision);
                    firstAffordableRecoveryYear = year;
                }
            }
            var beforeForecast = Encode(recoverable.Snapshot());
            var forecast = AnnualGameplay.Forecast(recoverable);
            Check(Encode(recoverable.Snapshot()) == beforeForecast, "Forecast changed the recovering run");
            var report = AnnualTownDemo.Advance(recoverable, "recoverable-" + year, provider);
            Check(Encode(forecast.ledger) == Encode(report.ledger) && Encode(forecast.outcomes) == Encode(report.outcomes), "Preview disagrees with authored acceptance");
            if (firstSuspendedYear == 0 && report.outcomes.sites.Any(s => s.phase == TownAssetPhase.Suspended)) firstSuspendedYear = year;
        }
        Check(firstSuspendedYear > 0 && firstAffordableRecoveryYear > firstSuspendedYear, "Useful action never returned after recoverable shortfall");
        Check(recoverable.ArrearsCents == 0 && recoverable.Assets.Any(a => a.siteId == recoverySite && a.actionId == recoveryAction), "Recovery plan did not commit after paying debt");

        File.WriteAllText(Path.Combine(evidence, "stress-checks.json"), Encode(new {
            definitionHash = seed.DefinitionHash, checks, targets, baselineScores = Scores(baseline),
            scope = "Deterministic real-map model checks; no browser, human or global balance optimality claim.",
            repeatedFamilies = strategies, interruptedFastForward = interruptions,
            lateProjectCommitmentsCents = unpaidTerms, maintenanceRecovery = recovery,
            affordableChoiceReturnedByEnding = sawRecovery,
            recoverableShortfall = new { firstSuspendedYear, firstAffordableRecoveryYear, recoveryAction, recoverySite,
                finalCashCents = recoverable.CashCents, finalArrearsCents = recoverable.ArrearsCents, scores = Scores(recoverable) }
        }) + "\n");
        Console.WriteLine("Annual stress checks passed: " + checks + "; " + strategies.Count + " family/order routes; 10 interrupted futures");
        return checks;
    }

    sealed class FailAtYear(int year) : IAnnualConsequenceProvider
    {
        public TownYearConsequences Resolve(TownYearRequest request)
        {
            if (request.year == year) throw new IOException("Deliberate provider failure before acceptance");
            return new AuthoredAnnualConsequenceProvider().Resolve(request);
        }
    }
}
