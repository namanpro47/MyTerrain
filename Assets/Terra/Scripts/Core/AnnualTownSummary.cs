using System;
using System.Globalization;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    /// <summary>A factual view of accepted history, reusable by the future game HUD or an AI narrator.</summary>
    [Serializable] public sealed class AnnualTownSummary
    {
        public int year, newProjects, continuingProjects;
        public bool finalYear;
        public string headline, scenario, simulationNotice;
        public long spentCents, receivedCents, cashChangeCents, freeAfterCommitmentsCents;
        public TownYearLedger ledger;
        public AnnualOutcomeSummary economic, social, environmental;
        public string[] siteChanges;

        public const string PlaceholderNotice = "SIMULATION PLACEHOLDER - authored rules, not a forecast. No LLM, crop-yield model or flood simulation is connected.";

        public static AnnualTownSummary Latest(AnnualTown town)
        {
            var history = town.History;
            return history.Length == 0 ? null : Create(town.Definition, history, history.Length - 1);
        }

        public static AnnualTownSummary Create(AnnualTownDefinition definition, TownYearReport[] history, int index)
        {
            if (definition == null || history == null || index < 0 || index >= history.Length)
                throw new ArgumentException("Choose an accepted annual report.");
            // Only read saved results. Opening a summary must never resolve, reroll or charge a year.
            var report = history[index];
            var previous = index == 0 ? null : history[index - 1];
            var outcome = report.outcomes;
            var ledger = TownCopies.Copy(report.ledger);
            int added = report.acceptedRequest.actions.Length;
            return new AnnualTownSummary {
                year = report.year, finalYear = report.year == definition.years,
                newProjects = added, continuingProjects = report.acceptedRequest.assets.Length,
                headline = added == 0 ? "No new projects. Existing land and commitments still advanced."
                    : added + (added == 1 ? " new project committed." : " new projects committed."),
                scenario = report.acceptedRequest.driver.description + " (" + report.acceptedRequest.driver.evidenceLevel + "). Output multiplier: "
                    + Number(report.acceptedRequest.driver.productionFactor) + ".",
                simulationNotice = PlaceholderNotice,
                ledger = ledger,
                spentCents = checked(ledger.upfrontPaidCents + ledger.programPaidCents + ledger.operatingPaidCents + ledger.repairPaidCents + ledger.arrearsPaidCents),
                receivedCents = checked(ledger.appropriationCents + ledger.publicReceiptsCents),
                cashChangeCents = checked(ledger.closingCashCents - ledger.openingCashCents),
                freeAfterCommitmentsCents = checked(ledger.closingCashCents - ledger.futureCommitmentsCents),
                economic = Metric("Economic", outcome.economicScore, previous?.outcomes.economicScore,
                    Number(outcome.productionUnits) + " illustrative production units; operating payments " + Money(ledger.operatingPaidCents) + " / " + Money(ledger.operatingDueCents) + " due.", outcome.economicExplanation),
                social = Metric("Social", outcome.socialScore, previous?.outcomes.socialScore,
                    Number(outcome.serviceUnits) + " illustrative service units; " + outcome.disruptedSites + " disrupted sites. Services currently use production as a proxy.", outcome.socialExplanation),
                environmental = Metric("Environmental", outcome.environmentalScore, previous?.outcomes.environmentalScore,
                    Number(outcome.vegetatedAreaM2 / 10000) + " / " + Number(outcome.managedAreaM2 / 10000) + " managed hectares vegetated; "
                    + Number(outcome.retentionCapacityM3) + " m3 configured retention capacity. This is not a flood-risk estimate.", outcome.environmentalExplanation),
                siteChanges = outcome.sites.Select(site => {
                    var name = definition.sites.First(s => s.id == site.siteId).name;
                    var old = previous?.outcomes.sites.FirstOrDefault(s => s.siteId == site.siteId);
                    string phase = site.actionId == null ? "Existing use" : site.phase.ToString();
                    string transition = old != null && old.actionId != null && old.phase != site.phase ? old.phase + " -> " + phase : phase;
                    string project = site.actionId == null ? "" : " / " + definition.actions.First(a => a.siteId == site.siteId && a.actionId == site.actionId).title;
                    return name + project + ": " + transition + "; production " + Number(site.productionUnits)
                        + (old == null ? "" : " (" + Signed(site.productionUnits - old.productionUnits) + " vs last year)")
                        + (site.actionId == null ? "" : "; establishment " + (site.maturity * 100).ToString("0", CultureInfo.InvariantCulture) + "%")
                        + ". " + site.explanation;
                }).ToArray()
            };
        }

        static AnnualOutcomeSummary Metric(string label, double score, double? previous, string evidence, string rubric)
            => new AnnualOutcomeSummary { label = label, score = score, hasPreviousYear = previous.HasValue,
                change = previous.HasValue ? score - previous.Value : 0, evidence = evidence, rubric = rubric };
        static string Number(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);
        static string Money(long cents) => (cents / 100m).ToString("C2", CultureInfo.GetCultureInfo("en-US"));
        static string Signed(double value) => value.ToString("+0.##;-0.##;0", CultureInfo.InvariantCulture);
    }

    [Serializable] public sealed class AnnualOutcomeSummary
    {
        public string label, evidence, rubric;
        public double score, change;
        public bool hasPreviousYear;
    }
}
