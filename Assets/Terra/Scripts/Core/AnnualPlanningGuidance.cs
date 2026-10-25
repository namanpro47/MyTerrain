using System;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    // A contextual prompt, not an optimizer or an outcome prediction. It never resolves a
    // year, changes a plan, spends money or adds another source of accounting truth.
    public sealed class AnnualPlanningGuidance
    {
        public string message, need, actionId, siteId;

        public static AnnualPlanningGuidance Create(AnnualTown town, TownYearOutcomes current)
        {
            if (town.Phase == TownPhase.Completed) return Say("Year 10 is complete. Compare your future, inspect history or start a new run.");
            if (town.Phase == TownPhase.Resolving) return Say("This year is pending. Retry the same year or choose the labelled offline result.");
            if (town.Phase == TownPhase.Reviewing) return Say("Read what changed and why, then continue to next year's plan.");
            if (town.Queue.Length > 0) return Say("Plan reserved, not paid. Revise with Undo, or use End Year to review and commit.");

            var definition = town.Definition;
            var occupied = town.Assets.Select(a => a.siteId).ToArray();
            var vacant = definition.actions.Where(a => !occupied.Contains(a.siteId)).ToArray();
            if (vacant.Length == 0) return Say("All project sites are committed. End Year waits; existing projects still advance.");
            var affordable = vacant.Where(a => town.Preview(a.siteId, a.actionId) >= 0).ToArray();
            if (affordable.Length == 0) return Say("No new project fits the available funds. End Year waits; upkeep and debt remain.");
            if (town.History.Length == 0) return Say("Choose a project, inspect a highlighted field, then queue it. Cash stays unchanged.");

            // Only recommend capacity for an unmet need, on a real vacant/affordable offer
            // that can open within the remaining game. This makes no claim about net scores.
            var economy = current?.economy;
            var targets = definition.economy;
            if (economy == null || targets == null) return Say("Compare needs, timing and upkeep. Choose another project, or End Year to wait.");
            var candidates = affordable.Where(a => a.services != null && town.Year + a.constructionYears <= definition.years).ToArray();
            // Count already funded capacity that has not opened yet. Do not recommend another
            // home/park simply because its predecessor is still under construction/establishment.
            var committed = town.Assets.Select(asset => new { asset, recipe = definition.actions.First(a => a.siteId == asset.siteId && a.actionId == asset.actionId) })
                .Where(x => x.recipe.services != null && town.Year < x.asset.startedYear + x.recipe.constructionYears + x.recipe.operatingYears).ToArray();
            double Capacity(Func<TownProjectServices,double> value) => committed.Sum(x => value(x.recipe.services) * x.asset.condition);
            AnnualPlanningGuidance Option(string need, Func<TownActionDefinition, bool> meetsNeed)
            {
                var offer = candidates.Where(meetsNeed).OrderBy(a => a.upfrontPublicCostCents
                    + a.annualPublicPaymentCents * a.paymentYears + a.annualPublicOperatingCents * a.operatingYears)
                    .ThenBy(a => a.actionId, StringComparer.Ordinal).ThenBy(a => a.siteId, StringComparer.Ordinal).FirstOrDefault();
                return offer == null ? null : new AnnualPlanningGuidance { need = need, actionId = offer.actionId, siteId = offer.siteId };
            }
            // Address constrained resources before inviting additional homes/jobs. A wastewater
            // plant never fixes drinking-water scarcity; the current catalog cannot add supply.
            if (economy.waterSupplied + .0001 < economy.waterDemand)
                return Say("Drinking water is limited. Avoid extra demand; End Year can wait, but adds no supply.");
            if (economy.energySupplied + .0001 < economy.energyDemand)
            {
                if (targets.baselineEnergySupply + Capacity(s => s.energySupply) >= targets.baselineEnergyDemand + Capacity(s => s.energyDemand))
                    return Say("Committed power is limited by timing or conditions. Inspect it, or End Year to wait.");
                return Option("Power need", a => a.services.energySupply > a.services.energyDemand)
                    ?? Say("No affordable power project can open by Year 10. End Year can wait; needs remain.");
            }
            AnnualPlanningGuidance suggestion = null;
            if (economy.wastewaterTreated + .0001 < economy.wastewaterLoad
                && targets.baselineWastewaterCapacity + Capacity(s => s.wastewaterCapacity) < targets.baselineWastewaterLoad + Capacity(s => s.wastewaterLoad))
                suggestion = Option("Wastewater need", a => a.services.wastewaterCapacity > a.services.wastewaterLoad);
            if (suggestion == null && targets.baselineHomes + Capacity(s => s.homes) < targets.householdDemand)
                suggestion = Option("Housing need", a => a.services.homes > 0);
            if (suggestion == null && targets.baselineRecreation + Capacity(s => s.recreation) < targets.recreationTarget)
                suggestion = Option("Recreation need", a => a.services.recreation > 0);
            if (suggestion == null && targets.baselineLocalServices + Capacity(s => s.localServices) < targets.localServicesTarget)
                suggestion = Option("Local service need", a => a.services.localServices > 0);
            if (suggestion == null && targets.baselineJobs + Capacity(s => s.jobs) < targets.jobsTarget)
                suggestion = Option("Jobs below target", a => a.services.jobs > 0);
            if (suggestion == null && current.productionUnits < definition.rubric.productionTargetUnits)
                suggestion = Option("Output below target", a => a.productionUnits > definition.sites.First(s => s.id == a.siteId).baselineProductionUnits);
            if (suggestion == null && current.vegetatedAreaM2 < current.managedAreaM2 * targets.coverTargetFraction)
                suggestion = Option("Cover below target", a => a.vegetatedFraction > definition.sites.First(s => s.id == a.siteId).baselineVegetatedFraction);
            return suggestion ?? Say("Compare remaining needs and project trade-offs, or End Year to wait with upkeep.");
        }

        static AnnualPlanningGuidance Say(string text) => new AnnualPlanningGuidance { message = text };
    }
}
