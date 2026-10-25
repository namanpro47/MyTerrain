using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    // A reading view of accepted results, including older saved offline chapters.
    // No resolution, score calculation or receipt mutation happens here.
    public static class AnnualChapterNarrative
    {
        public static AnnualChapterText Create(AnnualTownDefinition definition, TownYearReport report)
        {
            var sites = report.outcomes.sites;
            int added = report.acceptedRequest.actions.Length;
            int building = sites.Count(s => s.actionId != null && s.phase == TownAssetPhase.Building);
            int growing = sites.Count(s => s.actionId != null && s.phase == TownAssetPhase.Establishing);
            var economy = report.outcomes.economy;
            string opening = added == 0
                ? "This year, you gave the existing landscape time. No new projects were added, but the community's needs and the costs of caring for earlier choices continued."
                : "Your plan became part of the landscape this year. " + added + (added == 1 ? " new project is" : " new projects are") + " now committed, and their costs and consequences will carry into the years ahead.";
            if(added>0)
            {
                var titles=report.acceptedRequest.actions.Select(q=>definition.actions.First(a=>a.siteId==q.siteId&&a.actionId==q.actionId).title).Distinct().Take(2);
                opening+=" Your commitments include "+string.Join(" and ",titles)+".";
            }
            string pace = building > 0 ? "Some projects are still under construction. Their promised services must wait until they open."
                : growing > 0 ? "Some projects are still establishing. Their benefits will develop over time rather than arriving all at once."
                : "The year's results reflect the land and services already in use.";
            string budget = report.ledger.closingArrearsCents > 0
                ? "The year closes with unpaid bills. Those commitments follow you into the next plan, so restoring reliable funding matters before making new promises."
                : "The year's bills have been paid. The balance left in the public budget is your starting point for what comes next; some of it is already promised to ongoing projects.";
            budget += report.ledger.publicReceiptsCents > 0
                ? " Operating projects also returned tax receipts to the public budget."
                : " No project tax receipts arrived this year. Projects that are still being built cannot yet contribute them.";
            string people = pace + "\n\n";
            if (economy != null && economy.resourceFraction < .999)
                people += "Demand for energy or water ran ahead of the available supply. That limited what operating projects could deliver: adding capacity is only useful when the services behind it can keep up.";
            else if (economy != null)
                people += economy.occupiedHomes < definition.economy.householdDemand
                    ? "Housing needs remain unmet. A home only helps when it is ready and supplied; the next plan can weigh housing against work and everyday services."
                    : "The scenario's housing need is met. The next challenge is to sustain work and everyday services while keeping those homes supplied.";
            else people += "Food, homes and shared spaces support different parts of community life. The indicators below show how this year's accepted results compare with the start of the year.";
            string land = report.outcomes.disruptedSites > 0
                ? "Disruption reached part of the landscape this year. The accepted results include those setbacks, so the next plan starts from what actually remained in service."
                : "Land already committed to a use stays part of the story. Vegetated ground and any operating water projects contribute to the environmental result, alongside the pressures from everyday activity.";
            if(economy!=null&&economy.wastewaterTreated+.001<economy.wastewaterLoad)
                land+=" Wastewater demand exceeded treatment capacity, leaving a service gap for the next plan.";
            land += "\n\n" + report.acceptedRequest.driver.description + " These are authored game conditions, not a prediction for this place.";
            return new AnnualChapterText {
                title = report.year == definition.years ? "The future you leave behind" : "A year takes shape",
                introduction = opening + "\n\n" + pace,
                cards = new[] {
                    Card("What the year leaves in the budget", budget, "budget"),
                    Card("The pace of community life", people, "economic", "social"),
                    Card("The land carries the choices", land, "environmental", "weather")
                }
            };
        }
        public static string Decade(AnnualTownDefinition definition, TownYearReport[] history)
        {
            if (history == null || history.Length == 0) return "The landscape is waiting for its first decisions.";
            var end = history.Last();
            var choices = history.SelectMany(r => r.acceptedRequest.actions).ToArray();
            var changed = choices.Select(q => q.siteId).Distinct().Count();
            var conversions = choices.Length - changed;
            var groups = choices.GroupBy(q => q.actionId).OrderByDescending(g => g.Count()).Take(3)
                .Select(g => definition.actions.First(a => a.actionId == g.Key).title.ToLowerInvariant()).ToArray();
            string story = choices.Length == 0
                ? "You let the decade unfold without adding new projects. The community continued to rely on its existing homes, work and services while the land experienced each year's conditions."
                : "Over ten years, you reshaped " + changed + (changed == 1 ? " plot" : " plots") + " through " + string.Join(", ", groups) + ". "
                  + (conversions > 0 ? "You also changed direction on " + conversions + (conversions == 1 ? " occasion" : " occasions") + ", paying to replace earlier uses and allowing the new landscape time to establish. " : "")
                  + "Those choices changed how the community uses its limited land.";
            var e = end.outcomes.economy;
            if (e != null)
            {
                story += e.occupiedHomes > definition.economy.baselineHomes
                    ? " More people can be housed than at the beginning, provided those homes keep receiving the water, energy and services they need."
                    : " Housing remains an important question for the community's next decade.";
                if (e.jobs > definition.economy.baselineJobs) story += " The accepted projects support additional local work, with ongoing costs and resource needs.";
                if (e.resourceFraction < .999) story += " Energy or water shortages still limit what those investments can deliver.";
            }
            var opening = AnnualGameplay.Baseline(definition);
            story += end.outcomes.vegetatedAreaM2 > opening.vegetatedAreaM2
                ? " More vegetated land could support cover and water management as it matures."
                : end.outcomes.vegetatedAreaM2 < opening.vegetatedAreaM2
                    ? " Some vegetated land has given way to other uses, leaving a lasting trade-off for land and water management." : " The overall vegetated area remains close to its starting extent.";
            if (end.outcomes.sites.Any(s => s.actionId != null && (s.phase == TownAssetPhase.Building || s.phase == TownAssetPhase.Establishing)))
                story += " Some late choices are still building or establishing; their full effects lie beyond this review.";
            story += end.ledger.closingArrearsCents > 0
                ? " Unpaid bills also carry forward, so the community's next choices start with a financial constraint."
                : " Keeping these benefits requires continuing to fund the services and care they depend on.";
            return story + " These are possible consequences within this game, not a forecast for the real community.";
        }
        static AnnualChapterCard Card(string title, string text, params string[] facts)
            => new AnnualChapterCard { title = title, explanation = text, factIds = facts };
    }
}
