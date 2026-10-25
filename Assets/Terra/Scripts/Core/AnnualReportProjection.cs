using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    // Call only with reports already persisted by the annual session. Hashes verify
    // consistency, not persistence or authorship. This helper never resolves a year.
    public static class AnnualReportProjection
    {
        public const string Version = "annual-report-projection-v1";
        static readonly CultureInfo Culture = CultureInfo.InvariantCulture;

        public static ReportNarrationRequest Build(AnnualTownDefinition definition, TownYearReport acceptedReport,
            TownYearReport previousAcceptedReport, string modelVersion = ReportNarration.DefaultModelVersion)
        {
            AnnualTown.ValidateDefinition(definition);
            Require(definition.schemaVersion == 3, "Report projection requires numeric annual rules.");
            var d = TownCopies.Copy(definition);
            var report = TownCopies.Copy(acceptedReport);
            var previous = TownCopies.Copy(previousAcceptedReport);
            ValidateIdentity(d, report);
            TownYearOutcomes before;
            if (report.year == 1)
            {
                Require(previous == null, "Year 1 cannot have a previous accepted year.");
                Require(report.ledger.openingCashCents == d.startingCashCents && report.ledger.openingArrearsCents == 0
                    && TownCopies.Hash(report.acceptedRequest.simulation.openingState) == TownCopies.Hash(AnnualSimulation.Initial(d)),
                    "Year 1 does not start from the pinned baseline.");
                before = AnnualSimulation.OpeningOutcomes(d);
            }
            else
            {
                ValidateIdentity(d, previous);
                Require(previous.year == report.year - 1 && previous.acceptedRequest.runId == report.acceptedRequest.runId,
                    "Previous report belongs to a different year or run.");
                Require(previous.ledger.closingCashCents == report.ledger.openingCashCents
                    && previous.ledger.closingArrearsCents == report.ledger.openingArrearsCents
                    && TownCopies.Hash(previous.simulation.closingState) == TownCopies.Hash(report.acceptedRequest.simulation.openingState),
                    "Accepted years do not share the same carried state.");
                before = previous.outcomes;
            }
            var sim = report.simulation; var o = report.outcomes; var ledger = report.ledger;
            Equal(sim.economicDelta, o.economicScore - before.economicScore, "economic delta");
            Equal(sim.socialDelta, o.socialScore - before.socialScore, "social delta");
            Equal(sim.environmentalDelta, o.environmentalScore - before.environmentalScore, "environmental delta");

            var facts = new List<ReportNarrationFact>();
            facts.Add(Fact("budget", "Your saved public budget",
                "Opening " + Money(ledger.openingCashCents) + "; projects " + Money(ledger.upfrontPaidCents)
                + "; prior debt paid " + Money(ledger.arrearsPaidCents) + "; programs " + Money(ledger.programPaidCents)
                + "; operation " + Money(ledger.operatingPaidCents) + "; new repair " + Money(ledger.repairPaidCents)
                + "; public receipts " + Money(ledger.publicReceiptsCents) + "; appropriation " + Money(ledger.appropriationCents)
                + ". Closing " + Money(ledger.closingCashCents) + "; unpaid obligations " + Money(ledger.closingArrearsCents)
                + ". Private harvest is not public income. " + (report.year < 10 ? "Closing cash carries forward without another appropriation." : "Ending cash and obligations remain visible at the ten-year horizon."),
                Array.Empty<string>(), new[] { d.simulation.version }));

            var cropFacts = new List<ReportNarrationFact>();
            foreach (var crop in sim.crops.OrderByDescending(c => c.outputUnits).ThenBy(c => c.siteId, StringComparer.Ordinal))
            {
                var rule = d.simulation.crops.Single(c => c.id == crop.cropId);
                var links = new List<string> { rule.ruleId };
                if (crop.practiceId != "baseline") links.Add(d.simulation.practices.Single(p => p.actionId == crop.practiceId).ruleId);
                string practiceName = crop.practiceId == "baseline" ? "existing farming"
                    : d.actions.Single(a => a.siteId == crop.siteId && a.actionId == crop.practiceId).title;
                cropFacts.Add(Fact("harvest:" + crop.siteId, "Harvest · " + Name(d, crop.siteId),
                    crop.cropId + " with " + practiceName + ": " + Number(crop.outputUnits) + " " + crop.units
                    + " from " + Number(crop.productiveAreaM2 / 10000) + " ha. The saved result combines weather response " + Number(100 * crop.weatherFactor)
                    + "%, practice response " + Number(100 * crop.managementFactor) + "%, and condition at harvest " + Number(100 * crop.harvestCondition) + "%"
                    + ". Reference harvest date " + new DateTime(sim.driver.historicalYear, 1, 1).AddDays(crop.harvestDay - 1).ToString("MMM d", Culture)
                    + ". These are authored responses, not a calibrated yield forecast.",
                    new[] { crop.siteId }, links.ToArray()));
            }
            var impactFacts = new List<ReportNarrationFact>();
            foreach (var group in sim.impacts.GroupBy(i => i.siteId).OrderByDescending(g => g.Sum(i => i.conditionLoss)).ThenBy(g => g.Key, StringComparer.Ordinal))
            {
                var crop = sim.crops.SingleOrDefault(c => c.siteId == group.Key);
                string timing = crop == null ? "This site has no recorded crop harvest."
                    : group.Count(i => !i.beforeHarvest).ToString(Culture) + " impacts occurred after harvest; those affect carryover condition, not the completed harvest.";
                impactFacts.Add(Fact("disruption:" + group.Key, "Saved impacts · " + Name(d, group.Key),
                    group.Count().ToString(Culture) + " recorded heavy-rain impacts reduced condition by " + Number(100 * group.Sum(i => i.conditionLoss))
                    + " percentage points in total. " + timing + " Local storage and the separate upstream component follow the saved connection rules. This is illustrative disruption, not flood depth or probability.",
                    new[] { group.Key }, new[] { d.simulation.eventRuleId, d.simulation.sites.Single(s => s.siteId == group.Key).ruleId }));
            }
            // The local fallback keeps budget, one spatial consequence and service needs.
            var highlight = impactFacts.FirstOrDefault() ?? cropFacts.FirstOrDefault();
            if (highlight != null) facts.Add(highlight);
            facts.Add(Fact("services", "Needs still unmet",
                "Energy " + Number(sim.energyDeficit) + "; water " + Number(sim.waterDeficit) + "; wastewater " + Number(sim.wastewaterDeficit)
                + "; housing " + Number(sim.housingDeficit) + "; jobs " + Number(sim.jobsDeficit)
                + ". Each is its own scenario quantity; these values are not interchangeable units. The saved rubric links funded services, resources, output and land condition to the three scores.",
                Array.Empty<string>(), new[] { d.rubric.version }));
            facts.AddRange(cropFacts.Where(f => f != highlight));
            facts.AddRange(impactFacts.Where(f => f != highlight));
            foreach (var state in sim.closingState.sites.OrderBy(s => s.siteId, StringComparer.Ordinal))
            {
                var payments = sim.payments.Where(p => p.siteId == state.siteId && IsRepair(p)).ToArray();
                var debt = sim.closingState.repairs.Where(r => r.siteId == state.siteId).ToArray();
                long paid = payments.Sum(p => p.paidCents), unpaid = debt.Sum(r => r.unpaidCents);
                if (state.recoveredLastYear == 0 && paid == 0 && unpaid == 0) continue;
                var links = new[] { d.simulation.recoveryRuleId }.Concat(debt.Select(r => r.ruleId))
                    .Concat(payments.Select(p => d.simulation.publicResponsibilities.Single(r => r.actionId == p.actionId).ruleId)).ToArray();
                facts.Add(Fact("recovery:" + state.siteId, (report.year < 10 ? "Carryover · " : "Final condition · ") + Name(d, state.siteId),
                    "Recorded year-end recovery: " + Number(100 * state.recoveredLastYear) + " condition percentage points. Public repair paid " + Money(paid)
                    + "; remaining public repair debt " + Money(unpaid) + ". Closing condition " + Number(state.condition)
                    + (report.year < 10 ? " carries into the next year." : " remains at the ten-year horizon.")
                    + " Recovery does not change the harvest already reported; private recovery and paid public repair follow separate rules.",
                    new[] { state.siteId }, links));
            }
            facts.Add(Fact("weather-reference", "Weather reference used",
                "This accepted year uses La Crosse airport observations for " + sim.driver.historicalYear.ToString(Culture)
                + ", record " + sim.driver.recordId + ", from the run's pinned Wisconsin schedule. Precipitation is liquid equivalent. Crop and disruption responses are authored; wind and solar use separate constant assumptions, not inferred weather measurements.",
                Array.Empty<string>(), new[] { sim.driver.derivationVersion, sim.driver.generationRuleId }));
            facts.Add(Fact("reference", "What this scenario represents", AnnualSimulationWeather.Disclosure,
                Array.Empty<string>(), new[] { d.simulation.version }));
            var result = new ReportNarrationRequest {
                year = report.year, acceptedTransitionId = sim.transitionId, acceptedTransitionHash = sim.transitionHash,
                rulesVersion = d.simulation.version, reportVersion = Version, promptVersion = ReportNarration.PromptVersion,
                modelVersion = modelVersion, title = "Year " + report.year.ToString(Culture) + ": saved results",
                introduction = "Economic " + Score(o.economicScore, sim.economicDelta) + " · Social " + Score(o.socialScore, sim.socialDelta)
                    + " · Environmental " + Score(o.environmentalScore, sim.environmentalDelta) + ". Public cash " + Money(ledger.closingCashCents)
                    + " is saved. Scores are authored indices, rounded to 0.1; other non-money summaries are rounded to 0.001.",
                facts = facts.ToArray()
            };
            ReportNarration.ValidateRequest(result); // Fail explicitly, never truncate material facts.
            return result;
        }

        static void ValidateIdentity(AnnualTownDefinition d, TownYearReport r)
        {
            AnnualSimulation.ValidateReportHash(r);
            var q = r.acceptedRequest; var s = r.simulation; var o = r.outcomes; var l = r.ledger;
            Require(r.year >= 1 && r.year <= 10 && q.schemaVersion == 2 && q.year == r.year && q.definitionHash == TownCopies.Hash(d)
                && !string.IsNullOrWhiteSpace(q.runId) && q.requestId == s.transitionId && q.simulation.rulesVersion == d.simulation.version,
                "Report identity does not match its rules and year.");
            var input = TownCopies.Copy(q); input.inputHash = null;
            Require(q.inputHash == TownCopies.Hash(input), "Accepted request hash changed.");
            Require(r.acceptedConsequences != null && r.acceptedConsequences.runId == q.runId && r.acceptedConsequences.requestId == q.requestId
                && r.acceptedConsequences.year == r.year && r.acceptedConsequences.inputHash == q.inputHash
                && r.acceptedConsequences.effects != null && r.acceptedConsequences.effects.Length == 0, "Numeric evidence identity changed.");
            Require(TownCopies.Hash(s.driver) == TownCopies.Hash(d.simulation.drivers[r.year - 1])
                && TownCopies.Hash(q.simulation.driver) == TownCopies.Hash(s.driver)
                && TownCopies.Hash(q.driver) == TownCopies.Hash(d.drivers[r.year - 1]), "Report weather does not match the pinned driver.");
            Require(o.rubricVersion == d.rubric.version && o.economy != null && o.sites != null, "Missing accepted rubric outcomes.");
            Require(o.sites.All(x => x != null) && o.sites.Select(x => x.siteId).OrderBy(x => x, StringComparer.Ordinal)
                .SequenceEqual(d.sites.Select(x => x.id).OrderBy(x => x, StringComparer.Ordinal)), "Outcomes need every site exactly once.");
            Range(o.economicScore, 0, 100); Range(o.socialScore, 0, 100); Range(o.environmentalScore, 0, 100);
            foreach (var deficit in new[] { s.energyDeficit, s.waterDeficit, s.wastewaterDeficit, s.housingDeficit, s.jobsDeficit }) Range(deficit, 0, 1e12);
            Equal(s.energyDeficit, Math.Max(0, o.economy.energyDemand - o.economy.energySupplied), "energy deficit");
            Equal(s.waterDeficit, Math.Max(0, o.economy.waterDemand - o.economy.waterSupplied), "water deficit");
            Equal(s.wastewaterDeficit, Math.Max(0, o.economy.wastewaterLoad - o.economy.wastewaterTreated), "wastewater deficit");
            Equal(s.housingDeficit, Math.Max(0, d.economy.householdDemand - o.economy.occupiedHomes), "housing deficit");
            Equal(s.jobsDeficit, Math.Max(0, d.economy.jobsTarget - o.economy.jobs), "jobs deficit");
            Require(l.currency == "USD" && l.priceYear == d.priceYear, "Unknown public-money units.");
            foreach (var field in typeof(TownYearLedger).GetFields().Where(f => f.FieldType == typeof(long))) Require((long)field.GetValue(l) >= 0, "Negative public ledger amount.");
            Require(l.closingCashCents == checked(l.openingCashCents - l.upfrontPaidCents - l.arrearsPaidCents - l.programPaidCents - l.operatingPaidCents - l.repairPaidCents + l.publicReceiptsCents + l.appropriationCents), "Public cash does not reconcile.");
            Require(l.closingArrearsCents == checked(l.openingArrearsCents - l.arrearsPaidCents + l.programDueCents - l.programPaidCents + l.operatingDueCents - l.operatingPaidCents + l.repairDueCents - l.repairPaidCents), "Public arrears do not reconcile.");
            Require(s.closingState.sites.All(x => x != null) && s.closingState.sites.Select(x => x.siteId).OrderBy(x => x, StringComparer.Ordinal)
                .SequenceEqual(d.sites.Select(x => x.id).OrderBy(x => x, StringComparer.Ordinal)), "Closing state needs every site exactly once.");
            foreach (var site in s.closingState.sites) { Range(site.condition, 0, 1); Range(site.recoveredLastYear, 0, 1); }
            Require(s.crops.All(c => c != null) && s.crops.Select(c => c.siteId).Distinct().Count() == s.crops.Length, "Duplicate or missing crop results.");
            foreach (var crop in s.crops)
            {
                var site = d.sites.SingleOrDefault(x => x.id == crop.siteId); var rule = d.simulation.crops.SingleOrDefault(x => x.id == crop.cropId);
                Require(site != null && rule != null && crop.ruleId == rule.ruleId && crop.units == rule.units, "Unknown crop result link.");
                var practice = d.simulation.practices.SingleOrDefault(p => p.actionId == crop.practiceId);
                Require(crop.practiceId == "baseline" ? d.simulation.sites.Single(x => x.siteId == site.id).baselineCropId == crop.cropId
                    : practice != null && practice.cropSequence.Contains(crop.cropId)
                        && o.sites.Single(x => x.siteId == site.id).actionId == practice.actionId, "Unknown crop/practice relation.");
                Require(crop.harvestDay == s.driver.crops.Single(c => c.cropId == crop.cropId).harvestDay, "Wrong harvest calendar.");
                Range(crop.productiveAreaM2, 0, site.areaM2); Range(crop.weatherFactor, 0, 2); Range(crop.managementFactor, 0, 2); Range(crop.harvestCondition, 0, 1);
                Equal(crop.benchmarkPerHa, rule.outputPerHa, "crop benchmark");
                Equal(crop.outputUnits, crop.productiveAreaM2 / 10000 * crop.benchmarkPerHa * crop.weatherFactor * crop.managementFactor * crop.harvestCondition, "crop output");
            }
            foreach (var impact in s.impacts)
            {
                Require(impact != null, "Missing impact.");
                var exposure = d.simulation.sites.SingleOrDefault(x => x.siteId == impact.siteId);
                var e = s.driver.events.SingleOrDefault(x => x.recordId == impact.eventId);
                Require(exposure != null && e != null && impact.ruleId == d.simulation.eventRuleId && impact.connectionGroupId == exposure.connectionGroupId
                    && impact.dayOfYear == e.dayOfYear, "Unknown impact cause/site/event.");
                var crop = s.crops.SingleOrDefault(c => c.siteId == impact.siteId);
                Require(impact.beforeHarvest == (crop != null && impact.dayOfYear <= crop.harvestDay), "Wrong impact harvest timing.");
                Range(impact.conditionLoss, 0, 1);
            }
            Require(s.payments.All(p => p != null) && s.payments.Select(p => p.id).Distinct().Count() == s.payments.Length, "Duplicate or missing payment rows.");
            Require(s.payments.Sum(p => p.paidCents) == checked(l.upfrontPaidCents + l.arrearsPaidCents + l.programPaidCents + l.operatingPaidCents + l.repairPaidCents), "Itemized public payments do not reconcile.");
            foreach (var payment in s.payments)
            {
                Require(payment != null && payment.paidCents >= 0 && payment.dueCents >= payment.paidCents, "Invalid public payment.");
                if (IsRepair(payment)) Require(!string.IsNullOrWhiteSpace(payment.liabilityId) && d.sites.Any(x => x.id == payment.siteId)
                    && d.simulation.publicResponsibilities.Any(x => x.actionId == payment.actionId)
                    && o.sites.Single(x => x.siteId == payment.siteId).actionId == payment.actionId, "Unknown paid public repair link.");
            }
            Require(s.closingState.repairs.All(x => x != null) && s.closingState.repairs.Select(x => x.id).Distinct().Count() == s.closingState.repairs.Length,
                "Duplicate or missing repair liabilities.");
            Require(s.closingState.repairs.Sum(x => x.unpaidCents) <= l.closingArrearsCents, "Repair debt exceeds total arrears.");
            foreach (var debt in s.closingState.repairs)
                Require(debt != null && d.sites.Any(x => x.id == debt.siteId) && debt.unpaidCents >= 0 && debt.originalCents >= debt.unpaidCents
                    && d.simulation.publicResponsibilities.Any(x => x.actionId == debt.actionId && x.ruleId == debt.ruleId)
                    && o.sites.Single(x => x.siteId == debt.siteId).actionId == debt.actionId, "Unknown public repair debt link.");
        }
        static bool IsRepair(TownPublicPayment p) => p.kind == "new-repair" || p.kind == "prior-repair-arrears";
        static ReportNarrationFact Fact(string id, string title, string text, string[] sites, string[] rules)
            => new ReportNarrationFact { id = id, title = title, text = text, siteIds = sites,
                causalRuleIds = rules.Distinct(StringComparer.Ordinal).OrderBy(x => x, StringComparer.Ordinal).ToArray() };
        static string Name(AnnualTownDefinition d, string id) => d.sites.Single(s => s.id == id).name;
        static string Money(long cents) => "$" + (cents / 100m).ToString("N2", Culture);
        static string Number(double value) { Range(value, 0, 1e12); return value.ToString("0.###", Culture); }
        static string Score(double value, double delta)
        {
            double rounded = Math.Round(delta, 1, MidpointRounding.AwayFromZero);
            return value.ToString("0.0", Culture) + " (" + (rounded > 0 ? "+" : "") + (rounded == 0 ? "0.0" : rounded.ToString("0.0", Culture)) + ")";
        }
        static void Equal(double actual, double expected, string name)
            => Require(!double.IsNaN(actual) && !double.IsInfinity(actual) && !double.IsNaN(expected) && !double.IsInfinity(expected)
                && Math.Abs(actual - expected) <= 1e-8 * Math.Max(1, Math.Abs(expected)), "Inconsistent " + name + ".");
        static void Range(double value, double min, double max)
            => Require(!double.IsNaN(value) && !double.IsInfinity(value) && value >= min && value <= max, "Invalid report quantity.");
        static void Require(bool condition, string message) { if (!condition) throw new ArgumentException(message); }
    }
}
