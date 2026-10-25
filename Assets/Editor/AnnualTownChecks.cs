using System;
using System.Linq;
using Terra.Contracts;
using Terra.Core;
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
#endif

// One specification suite runs against the actual domain sources in .NET and Unity.
public static class AnnualTownChecks
{
    static int count;
    static void Check(bool value, string message) { if (!value) throw new Exception("FAILED: " + message); count++; }
    static void Near(double actual, double expected, string message) => Check(Math.Abs(actual - expected) < .000001, message);
    static void Reject(Action action, string message)
    {
        try { action(); } catch (ArgumentException) { count++; return; } catch (InvalidOperationException) { count++; return; }
        throw new Exception("FAILED to reject: " + message);
    }
    static TownYearReport Advance(AnnualTown town, string id) => AnnualTownDemo.Advance(town, id, new AuthoredAnnualConsequenceProvider());
    static TownYearConsequences Response(TownYearRequest request, params TownSiteEffect[] effects) => new TownYearConsequences {
        runId = request.runId, requestId = request.requestId, year = request.year, inputHash = request.inputHash, effects = effects
    };
    static TownActionDefinition Offer(string site, string id, long payment)
    {
        return new TownActionDefinition {
            siteId = site, actionId = id, title = id, sourceId = "authored-fixture", evidenceLevel = "scenario-assumption",
            ownerOperator = "Fictional participating farmer", contributionType = "Public assistance", fundedQuantity = site == "a" ? 10 : 5, fundedUnit = "acre",
            annualPublicPaymentCents = payment, paymentYears = 3, operatingYears = 10, constructionYears = 0, establishmentYears = 1,
            productionUnits = 5, serviceUnits = 5, vegetatedFraction = .8
        };
    }
    public static AnnualTownDefinition Fixture()
    {
        return new AnnualTownDefinition {
            id = "accounting-fixture", version = "1.0.0", worldPackId = "technical-fixture", years = 10, startingCashCents = 1000000,
            assumptions = "Authored example areas/terms. $600 + $375 annual assistance; not a local project quote.",
            appropriationsCents = new long[10],
            sites = new[] {
                new TownSiteDefinition { id = "a", name = "Ten-acre example", areaM2 = 40468.564224, baselineProductionUnits = 2, baselineServiceUnits = 2, baselineVegetatedFraction = .2 },
                new TownSiteDefinition { id = "b", name = "Five-acre example", areaM2 = 20234.282112, baselineProductionUnits = 1, baselineServiceUnits = 1, baselineVegetatedFraction = .2 },
                new TownSiteDefinition { id = "protected", name = "No edit", areaM2 = 1000, protectedSite = true, baselineVegetatedFraction = 1 }
            },
            actions = new[] { Offer("a", "single-species", 60000), Offer("b", "multi-species", 37500), Offer("a", "alternate", 50000) },
            drivers = Enumerable.Range(1, 10).Select(y => new TownYearDriver { year = y, id = "fixture-year-" + y, description = "Authored reference conditions", evidenceLevel = "scenario-assumption", productionFactor = y == 3 ? .8 : 1 }).ToArray(),
            rubric = new TownScoreRubric { version = "fixture-rubric-v1", explanation = "Authored 80/20 indicators; services are an illustrative production proxy.", productionTargetUnits = 10, serviceTargetUnits = 10, retentionTargetM3 = 100 }
        };
    }
    sealed class FailingProvider : IAnnualConsequenceProvider
    {
        public TownYearConsequences Resolve(TownYearRequest request) => throw new InvalidOperationException("Authored provider failure");
    }
    public static int Run()
    {
        count = 0;
        var rules = Fixture(); var town = new AnnualTown(rules, "accounting-run");
        Check(town.Phase == TownPhase.Planning && town.Year == 1, "Town opens in Year 1 Planning");
        long preview = town.Preview("a", "single-species");
        Check(preview == 940000 && town.CashCents == 1000000 && town.Queue.Length == 0 && town.Revision == 0, "Preview is pure and reserves the current installment");
        town.QueueAction("a", "single-species", town.Revision);
        town.QueueAction("b", "multi-species", town.Revision);
        Check(town.QueuedCommitmentCents == 292500 && town.AvailableCents == 902500 && town.CashCents == 1000000, "$2,925 lifetime obligation remains visible; $975 reserved now and cash stays $10,000");
        Reject(() => town.QueueAction("a", "alternate", 0), "Stale plan revision");
        Check(town.UndoPlan(town.Revision) && town.QueuedCommitmentCents == 180000, "Queue-only undo frees the second commitment");
        town.QueueAction("b", "multi-species", town.Revision);
        int submittedRevision = town.Revision;
        var request = town.BeginResolution("year-1", submittedRevision);
        Check(town.Phase == TownPhase.Resolving && town.CashCents == 1000000, "Beginning a year freezes without charging money");
        Reject(() => town.UndoPlan(town.Revision), "No undo while resolving");
        Reject(() => town.BeginResolution("other", town.Revision), "No concurrent year resolution");
        var clean = Response(request);
        var bad = Response(request, new TownSiteEffect { siteId = "invented", sourceId = "authored-fixture", reason = "No invented sites" });
        Reject(() => town.CompleteResolution(bad), "Unknown effect site");
        Check(town.CashCents == 1000000 && town.History.Length == 0 && town.Phase == TownPhase.Resolving, "Rejected effects publish neither money nor history");
        request.actions[0].actionId = "alternate";
        Check(town.PendingRequest.actions[0].actionId == "single-species", "Provider cannot mutate the frozen request");
        var first = town.CompleteResolution(clean);
        Check(town.Phase == TownPhase.Reviewing && first.ledger.programPaidCents == 97500 && town.CashCents == 902500, "Year 1 pays $975 exactly once");
        Check(first.ledger.futureCommitmentsCents == 195000 && town.CommittedCents == 195000 && town.AvailableCents == 805000, "Review shows $1,950 later obligations and reserves $975 for the coming year");
        Near(first.outcomes.productionUnits, 10, "Operational actions contribute illustrative production");
        Near(first.outcomes.economicScore, 100, "Economic rubric uses its named components");
        Near(first.outcomes.socialScore, 100, "Social rubric uses its named components");
        double expectedCover = (40468.564224 * .8 + 20234.282112 * .8 + 1000) / (40468.564224 + 20234.282112 + 1000);
        Near(first.outcomes.environmentalScore, 80 * expectedCover, "Environmental rubric uses area-weighted management-site cover");
        first.ledger.closingCashCents = 0; first.outcomes.productionUnits = 999;
        Check(town.History[0].ledger.closingCashCents == 902500 && town.History[0].outcomes.productionUnits == 10, "Returned reports cannot mutate history");
        Check(town.CompleteResolution(clean).ledger.programPaidCents == 97500 && town.CashCents == 902500 && town.History.Length == 1, "Repeated completion returns the committed result");
        Check(AnnualTownDemo.Advance(town, "year-1", new FailingProvider()).year == 1 && town.CashCents == 902500, "A committed helper retry never calls the provider");
        Reject(() => town.CompleteResolution(Response(town.History[0].acceptedRequest, new TownSiteEffect { siteId = "a", sourceId = "authored-fixture", reason = "Different payload" })), "Same committed ID cannot reroll a new response");
        Check(town.BeginResolution("year-1", submittedRevision).year == 1 && town.Phase == TownPhase.Reviewing, "Repeated begin does not open Year 2");
        Reject(() => town.RemoveQueuedAction("a", town.Revision), "No historical refund");

        town.ContinuePlanning(town.Revision);
        Reject(() => town.QueueAction("a", "alternate", town.Revision), "Committed site cannot be replaced/refunded as a new proposal");
        var second = Advance(town, "year-2");
        Check(second.ledger.programPaidCents == 97500 && town.CashCents == 805000 && town.CommittedCents == 97500, "Wait Year 2 pays only its installment");
        town.ContinuePlanning(town.Revision);
        var third = Advance(town, "year-3");
        Check(town.CashCents == 707500 && town.CommittedCents == 0 && third.ledger.programPaidCents == 97500, "Three installments total $2,925; obligations finish");
        Near(third.outcomes.productionUnits, 8, "Saved wet-year driver changes outcomes without changing contract rates");
        for (int y = 4; y <= 10; y++) { town.ContinuePlanning(town.Revision); Advance(town, "year-" + y); }
        Check(town.Phase == TownPhase.Completed && town.Year == 10 && town.History.Length == 10 && town.CashCents == 707500, "Ten years complete without an eleventh payment or silent new plan");
        Reject(() => town.ContinuePlanning(town.Revision), "No Year 11");
        Reject(() => town.QueueAction("b", "multi-species", town.Revision), "Completed towns cannot queue actions");
        var ff = new AnnualTown(Fixture(), "fast-forward");
        ff.QueueAction("a", "single-species", ff.Revision); ff.QueueAction("b", "multi-species", ff.Revision);
        var ffReports = AnnualTownDemo.Finish(ff, new AuthoredAnnualConsequenceProvider());
        Check(ffReports.Length == 10 && ff.CashCents == town.CashCents && ff.Phase == TownPhase.Completed, "Fast-forward uses the same annual ledger rules");
        Check(AnnualTownDemo.Finish(ff, new FailingProvider()).Length == 0 && ff.CashCents == town.CashCents, "Finishing a completed run is a no-op");
        var restored = AnnualTown.Restore(rules, town.Snapshot());
        Check(restored.CashCents == town.CashCents && restored.History.Length == 10 && restored.Phase == TownPhase.Completed, "Restore replays all accepted years without a provider");
        var tampered = town.Snapshot(); tampered.cashCents++;
        Reject(() => AnnualTown.Restore(rules, tampered), "Save cash must agree with committed evidence");
        var changedRules = Fixture(); changedRules.actions[0].annualPublicPaymentCents++;
        Reject(() => AnnualTown.Restore(changedRules, town.Snapshot()), "Rule/price changes cannot rewrite an accepted run");
        rules.startingCashCents = 1;
        Check(restored.Definition.startingCashCents == 1000000, "Definition is detached from caller mutation");

        var poorRules = Fixture(); poorRules.startingCashCents = 1000;
        var poor = new AnnualTown(poorRules, "poor");
        Reject(() => poor.QueueAction("a", "single-species", poor.Revision), "Cannot fund the current installment");
        Check(poor.Revision == 0 && poor.CashCents == 1000 && poor.Queue.Length == 0, "Overspend leaves state unchanged");
        Reject(() => poor.QueueAction("protected", "single-species", poor.Revision), "Protected site has no eligible action");
        Reject(() => poor.QueueAction("missing", "single-species", poor.Revision), "Unknown site rejected");

        var lifecycle = Fixture();
        lifecycle.actions[0].annualPublicPaymentCents = 0; lifecycle.actions[0].upfrontPublicCostCents = 10000;
        lifecycle.actions[0].constructionYears = 1; lifecycle.actions[0].establishmentYears = 2; lifecycle.actions[0].operatingYears = 2;
        lifecycle.actions[0].annualPublicOperatingCents = 1000; lifecycle.actions[0].annualPublicReceiptsCents = 500;
        var growing = new AnnualTown(lifecycle, "growing"); growing.QueueAction("a", "single-species", 0);
        var build = Advance(growing, "g1");
        Check(build.outcomes.sites[0].phase == TownAssetPhase.Building && build.ledger.operatingPaidCents == 0 && build.ledger.publicReceiptsCents == 0, "Construction yields no operations/receipts");
        growing.ContinuePlanning(growing.Revision); var establish = Advance(growing, "g2");
        Check(establish.outcomes.sites[0].phase == TownAssetPhase.Establishing && establish.ledger.operatingPaidCents == 1000 && establish.ledger.publicReceiptsCents == 500, "Operations and explicitly configured public receipts start on schedule");
        Near(establish.outcomes.sites[0].productionUnits, 2.5, "Half-established project has half its configured output");
        growing.ContinuePlanning(growing.Revision); var mature = Advance(growing, "g3");
        Check(mature.outcomes.sites[0].phase == TownAssetPhase.Operating, "Establishment continues while waiting");
        growing.ContinuePlanning(growing.Revision); var expired = Advance(growing, "g4");
        Check(expired.outcomes.sites[0].phase == TownAssetPhase.Retired && expired.ledger.publicReceiptsCents == 0 && expired.ledger.operatingPaidCents == 0, "Finite operating contract does not charge or earn forever");

        var unpaidRules = Fixture(); unpaidRules.startingCashCents = 1000;
        unpaidRules.drivers[0].unexpectedPublicRepairCents = 2000; unpaidRules.appropriationsCents[1] = 3000;
        var unpaid = new AnnualTown(unpaidRules, "arrears"); var loss = Advance(unpaid, "u1");
        Check(loss.ledger.repairPaidCents == 1000 && unpaid.CashCents == 0 && unpaid.ArrearsCents == 1000, "Unexpected shortfall records an obligation without inventing cash");
        unpaid.ContinuePlanning(unpaid.Revision); Advance(unpaid, "u2");
        Check(unpaid.CashCents == 3000 && unpaid.ArrearsCents == 1000, "Closing appropriation does not retroactively pay earlier obligations");
        unpaid.ContinuePlanning(unpaid.Revision); var paid = Advance(unpaid, "u3");
        Check(paid.ledger.arrearsPaidCents == 1000 && unpaid.ArrearsCents == 0 && unpaid.CashCents == 2000, "Outstanding obligations carry and are paid, never dropped");

        var failure = new AnnualTown(Fixture(), "retry"); failure.QueueAction("a", "single-species", 0);
        Reject(() => AnnualTownDemo.Advance(failure, "retry1", new FailingProvider()), "Provider failure surfaces");
        Check(failure.Phase == TownPhase.Resolving && failure.CashCents == 1000000 && failure.History.Length == 0, "Failure freezes the request without partial payment");
        var resumed = AnnualTown.Restore(Fixture(), failure.Snapshot());
        Check(resumed.Phase == TownPhase.Resolving && resumed.PendingRequest.requestId == "retry1", "In-flight resolution survives save/reload");
        var invalid = Response(resumed.PendingRequest, new TownSiteEffect { siteId = "a", sourceId = "authored-fixture", reason = "Invalid finite value", productionFactor = double.NaN });
        Reject(() => resumed.CompleteResolution(invalid), "NaN consequence");
        invalid.effects[0].productionFactor = 3; Reject(() => resumed.CompleteResolution(invalid), "Out-of-range consequence");
        invalid.effects[0].productionFactor = 1; invalid.effects[0].sourceId = "invented"; Reject(() => resumed.CompleteResolution(invalid), "Unknown causal source");
        invalid.effects[0].sourceId = "authored-fixture"; invalid.effects = new[] { invalid.effects[0], invalid.effects[0] }; Reject(() => resumed.CompleteResolution(invalid), "Duplicate site effects");
        Advance(resumed, "retry1");
        Check(resumed.CashCents == 940000 && resumed.History.Length == 1, "Retry charges only one first installment");
        var cancel = new AnnualTown(Fixture(), "cancel"); cancel.QueueAction("a", "single-species", 0); cancel.BeginResolution("c1", cancel.Revision);
        cancel.CancelResolution(cancel.Revision); Check(cancel.Phase == TownPhase.Planning && cancel.CanUndo && cancel.CashCents == 1000000, "Cancelling before publication returns the unfinished plan");
        var planned = AnnualTown.Restore(Fixture(), cancel.Snapshot());
        Check(planned.UndoPlan(planned.Revision) && planned.Queue.Length == 0, "Planning undo history survives restore");

        var affected = new AnnualTown(Fixture(), "context"); affected.QueueAction("a", "single-species", 0);
        var affectedRequest = affected.BeginResolution("context1", affected.Revision);
        var affectedReport = affected.CompleteResolution(Response(affectedRequest, new TownSiteEffect {
            siteId = "a", sourceId = affectedRequest.driver.id, productionFactor = .5, conditionDelta = -.1,
            disrupted = true, reason = "Authored local disruption with a persistent condition setback."
        }));
        Near(affectedReport.outcomes.productionUnits, 3.25, "Accepted context changes output using the operational asset's condition and factor");
        Check(affected.CashCents == 940000 && affectedReport.outcomes.disruptedSites == 1, "Context changes outcomes, not the public contract payment");
        affected.ContinuePlanning(affected.Revision); var recovering = Advance(affected, "context2");
        Near(recovering.outcomes.productionUnits, 5.5, "Condition persists into the next year when the temporary disruption ends");

        Check(AnnualTownSummary.Latest(new AnnualTown(Fixture(), "no-summary")) == null, "An unsubmitted run has no invented annual summary");
        var firstSummary = AnnualTownSummary.Create(town.Definition, town.History, 0);
        Check(firstSummary.year == 1 && firstSummary.newProjects == 2 && firstSummary.continuingProjects == 0 && !firstSummary.finalYear, "First summary describes committed choices and horizon");
        Check(firstSummary.spentCents == 97500 && firstSummary.receivedCents == 0 && firstSummary.cashChangeCents == -97500
            && firstSummary.freeAfterCommitmentsCents == 707500, "Summary distinguishes cash spent from the full reserved commitment");
        Check(!firstSummary.economic.hasPreviousYear && !firstSummary.social.hasPreviousYear && !firstSummary.environmental.hasPreviousYear, "Year 1 does not invent a previous annual score");
        var wetSummary = AnnualTownSummary.Create(town.Definition, town.History, 2);
        Check(wetSummary.newProjects == 0 && wetSummary.continuingProjects == 2 && wetSummary.scenario.Contains("Authored reference conditions"), "Waiting summary retains projects and discloses the authored output driver");
        Near(wetSummary.economic.change, -16, "Summary economic delta is measured against the actual previous year");
        Near(wetSummary.social.change, -16, "Summary social delta follows accepted illustrative services");
        Near(wetSummary.environmental.change, 0, "Output setback does not fabricate environmental change");
        Check(wetSummary.simulationNotice.Contains("PLACEHOLDER") && wetSummary.environmental.evidence.Contains("not a flood-risk estimate"), "Placeholder and environmental limits are visible in the summary");
        var transition = AnnualTownSummary.Create(growing.Definition, growing.History, 1);
        Check(transition.siteChanges.Any(s => s.Contains("Building -> Establishing")), "Summary explains delayed project establishment");
        var finalSummary = AnnualTownSummary.Latest(town);
        Check(finalSummary.year == 10 && finalSummary.finalYear && finalSummary.ledger.closingCashCents == 707500, "Final summary uses accepted Year 10 history");
        int beforeSummaryRevision = town.Revision;
        finalSummary.ledger.closingCashCents = 0; finalSummary.economic.score = -1;
        Check(AnnualTownSummary.Latest(town).ledger.closingCashCents == 707500 && town.Revision == beforeSummaryRevision && town.History.Length == 10, "Reopening or mutating a summary never changes accepted state or charges again");
        Reject(() => AnnualTownSummary.Create(town.Definition, town.History, 10), "A summary cannot invent Year 11");

        foreach (var row in town.History)
        {
            var l = row.ledger;
            Check(l.closingCashCents == l.openingCashCents - l.upfrontPaidCents - l.programPaidCents - l.operatingPaidCents - l.repairPaidCents - l.arrearsPaidCents + l.appropriationCents + l.publicReceiptsCents, "Every annual cash ledger reconciles");
            Check(row.outcomes.economicScore >= 0 && row.outcomes.economicScore <= 100 && row.outcomes.socialScore >= 0 && row.outcomes.socialScore <= 100 && row.outcomes.environmentalScore >= 0 && row.outcomes.environmentalScore <= 100, "All three derived indicators remain bounded");
        }
        return count;
    }

#if UNITY_EDITOR
    [MenuItem("Terra/Annual Town/Run Foundation Checks")]
    public static void RunBatch()
    {
        Run();
        int legacyChecks = JsonUtility.FromJson<Phase2Checks.Report>(Phase2Checks.Run()).count;
        Check(legacyChecks == 139, "Existing WorldPack/site/legacy transaction checks remain valid");
        var world = JsonUtility.FromJson<WorldData>(System.IO.File.ReadAllText("worlds/meadowbrook-v1/world.json"));
        var scenario = JsonUtility.FromJson<Scenario>(System.IO.File.ReadAllText("worlds/meadowbrook-v1/scenario.json"));
        var definition = AnnualTownDemo.FromWorldPack(world, scenario, "meadowbrook-v1");
        var town = new AnnualTown(definition, "unity-map-example");
        var first = definition.actions.First(a => a.actionId == "cultivate-standard");
        town.QueueAction(first.siteId, first.actionId, town.Revision);
        // Test Unity's real serializer, including a pending request and final history.
        var request = town.BeginResolution("unity-year-1", town.Revision);
        var save = JsonUtility.FromJson<AnnualTownSnapshot>(JsonUtility.ToJson(town.Snapshot()));
        town = AnnualTown.Restore(definition, save);
        Check(town.PendingRequest.inputHash == request.inputHash, "Unity JSON round-trip preserves frozen request");
        Advance(town, "unity-year-1");
        for (int year = 2; year <= 10; year++)
        {
            town.ContinuePlanning(town.Revision);
            if (year == 2)
            {
                var contextRequest = town.BeginResolution("unity-year-2", town.Revision);
                town.CompleteResolution(Response(contextRequest, new TownSiteEffect {
                    siteId = first.siteId, sourceId = contextRequest.driver.id, conditionDelta = -.1,
                    reason = "Authored condition setback for persistence/serialization verification."
                }));
            }
            else Advance(town, "unity-year-" + year);
        }
        var json = JsonUtility.ToJson(town.Snapshot(), true);
        var reloaded = AnnualTown.Restore(definition, JsonUtility.FromJson<AnnualTownSnapshot>(json));
        Check(reloaded.History.Length == 10 && reloaded.CashCents == town.CashCents, "Unity JSON round-trip preserves ten-year evidence and money");
        var sessionObject = new GameObject("Annual session check");
        try
        {
            var session = sessionObject.AddComponent<Terra.Gameplay.AnnualTownSession>();
            session.Initialize(definition, "session-check");
            session.Changed += snapshot => snapshot.cashCents = 0;
            session.QueueAction(first.siteId, first.actionId, session.State.revision);
            session.EndYearOffline("session-year-1");
            var sessionSave = session.ExportSave(); session.ImportSave(sessionSave);
            Check(session.State.phase == TownPhase.Reviewing && session.State.history.Length == 1 && session.State.cashCents > 0, "Unity session exports/imports state and publishes detached snapshots");
            var balance = session.State.cashCents; session.EndYearOffline("session-year-1");
            Check(session.State.cashCents == balance && session.State.history.Length == 1, "Unity button adapter handles repeated completion once");
            var summary1 = session.LatestSummary;
            Check(summary1.year == 1 && summary1.newProjects == 1 && summary1.siteChanges.Any(line => line.Contains("Building")), "Actual-map session summary shows the first project's construction delay");
            session.ContinueYear(session.State.revision);
            Check(session.State.year == 2 && session.State.phase == TownPhase.Planning && session.State.queue.Length == 0
                && session.State.assets.Length == 1 && session.State.cashCents == balance, "Next-year control reopens the same town with committed assets and money intact");
            var secondOffer = definition.actions.First(a => a.siteId != first.siteId && a.actionId == "restore-grass");
            Check(session.Preview(secondOffer.siteId, secondOffer.actionId) >= 0, "Next-year planning can preview another affordable actual-map project");
            session.QueueAction(secondOffer.siteId, secondOffer.actionId, session.State.revision);
            session.EndYearOffline("session-year-2");
            Check(session.LatestSummary.newProjects == 1 && session.LatestSummary.continuingProjects == 1
                && session.LatestSummary.economic.hasPreviousYear && session.State.history.Length == 2, "Year 2 summary combines the new project and continued Year 1 project");
            session.ContinueYear(session.State.revision);
            var paused = session.BeginYear("session-year-3", session.State.revision);
            session.ImportSave(session.ExportSave());
            var year3 = session.EndYearOffline(paused.requestId);
            Check(year3.year == 3 && session.LatestSummary.newProjects == 0 && session.State.history.Length == 3
                && session.LatestSummary.scenario.Contains("0.8"), "Imported frozen request resumes into the same Year 3 waiting summary");
            var restoredSummary = JsonUtility.ToJson(session.LatestSummary);
            session.ImportSave(session.ExportSave());
            Check(JsonUtility.ToJson(session.LatestSummary) == restoredSummary, "Reloading preserves the exact summary without rerunning simulation");
            session.FinishOffline();
            Check(session.State.phase == TownPhase.Completed && session.State.year == 10 && session.State.history.Length == 10 && session.LatestSummary.finalYear,
                "Session control flow reaches the final summary without Year 11");
            System.IO.Directory.CreateDirectory("Logs/AnnualTown");
            System.IO.File.WriteAllText("Logs/AnnualTown/playtest-final-summary.json", JsonUtility.ToJson(session.LatestSummary, true));
        }
        finally { UnityEngine.Object.DestroyImmediate(sessionObject); }
        var report = JsonUtility.ToJson(new CheckResult { passed = true, checks = count, legacyChecks = legacyChecks, mapActions = definition.actions.Length, historyYears = reloaded.History.Length, closingCashCents = reloaded.CashCents }, true);
        System.IO.Directory.CreateDirectory("Logs/AnnualTown");
        System.IO.File.WriteAllText("Logs/AnnualTown/checks.json", report);
        System.IO.File.WriteAllText("Logs/AnnualTown/ten-year-example.json", json);
        Debug.Log("TERRA_ANNUAL_CHECKS: " + report);
    }
    [Serializable] public sealed class CheckResult { public bool passed; public int checks, legacyChecks, mapActions, historyYears; public long closingCashCents; }
#endif
}
