using System;
using System.Linq;
using Terra.Contracts;
using Terra.Simulation;

namespace Terra.Core
{
    // Small, fully recomputed educational service model. Quantities are authored scenario units.
    public static class AnnualEconomy
    {
        static double Cap(double value) => Math.Max(0, Math.Min(1, value));
        static double Met(double supply, double demand) => demand == 0 ? 1 : Cap(supply / demand);
        static double Active(TownSiteYearResult row) => row.phase == TownAssetPhase.Operating || row.phase == TownAssetPhase.Establishing ? row.maturity * row.condition : 0;
        public static void Validate(AnnualTownDefinition definition)
        {
            var e = definition.economy;
            AnnualTown.Require(e != null && e.version == "services-v2" && !string.IsNullOrWhiteSpace(e.assumptions), "Schema 2 requires explicit service assumptions.");
            foreach (var field in typeof(TownEconomyDefinition).GetFields().Where(f => f.FieldType == typeof(double)))
            { double n = (double)field.GetValue(e); AnnualTown.Require(!double.IsNaN(n) && !double.IsInfinity(n) && n >= 0 && n <= 1e12, "Invalid economy quantity: " + field.Name); }
            AnnualTown.Require(e.householdDemand > 0 && e.jobsTarget > 0 && e.recreationTarget > 0 && e.localServicesTarget > 0 && e.emissionsLimit > 0 && e.coverTargetFraction > 0 && e.coverTargetFraction <= 1 && e.runoffReductionTarget > 0 && e.runoffReductionTarget <= 1, "Economy targets must be positive and bounded.");
            AnnualTown.Require(new[]{e.finalEconomicTarget,e.finalSocialTarget,e.finalEnvironmentalTarget}.All(t=>t>0&&t<=100), "Final assessment targets must be explicit scores in (0,100].");
            AnnualTown.Require(e.referenceStorm != null, "Full meters require a reference storm.");
            RunoffEvaluator.Evaluate(e.referenceStorm);
            foreach (var site in definition.sites)
                AnnualTown.Require(e.referenceStorm.sites.Count(s => s.siteId == site.id) == 1, "Reference storm must include every managed site exactly once.");
            foreach (var action in definition.actions)
            {
                var s = action.services;
                AnnualTown.Require(s != null && !string.IsNullOrWhiteSpace(s.familyId) && !string.IsNullOrWhiteSpace(s.assumptions), "Each full recipe needs explicit service quantities, including zeroes.");
                foreach (var field in typeof(TownProjectServices).GetFields().Where(f => f.FieldType == typeof(double)))
                { double n = (double)field.GetValue(s); AnnualTown.Require(!double.IsNaN(n) && !double.IsInfinity(n) && n >= 0 && n <= 1e12, "Invalid project quantity: " + field.Name); }
                AnnualTown.Require(s.occupiedAreaM2 > 0 && s.occupiedAreaM2 <= definition.sites.First(x => x.id == action.siteId).areaM2 && s.targetCurveNumber > 0 && s.targetCurveNumber <= 100, "Invalid project area or runoff mapping.");
                AnnualTown.Require(s.connected && s.consent, "Activated recipes need explicit fictional access, connection and consent.");
                AnnualTown.Require(s.cleanEnergySupply <= s.energySupply, "Clean supply cannot exceed generation.");
            }
        }
        public static SimulationOutput Water(AnnualTownDefinition definition, TownYearOutcomes outcomes)
        {
            var input = TownCopies.Copy(definition.economy.referenceStorm);
            foreach (var row in outcomes.sites.Where(r => !string.IsNullOrEmpty(r.actionId)))
            {
                var recipe = definition.actions.First(a => a.siteId == row.siteId && a.actionId == row.actionId);
                var site = input.sites.First(s => s.siteId == row.siteId);
                // Land conversion persists even if operation later suspends; retention requires funding.
                site.curveNumber += (float)((recipe.services.targetCurveNumber-site.curveNumber)*row.maturity*row.condition);
                site.retentionCapacityM3 = (float)row.retentionCapacityM3;
            }
            return RunoffEvaluator.Evaluate(input);
        }
        public static void Evaluate(AnnualTownDefinition definition, TownYearOutcomes outcomes, TownYearLedger ledger, TownYearDriver driver)
        {
            var e = definition.economy;
            var v = new TownEconomyOutcome { homesBuilt=e.baselineHomes, jobs=e.baselineJobs, recreation=e.baselineRecreation, localServices=e.baselineLocalServices,
                energyDemand=e.baselineEnergyDemand, energySupplied=e.baselineEnergySupply, cleanEnergySupplied=e.baselineCleanEnergySupply,
                waterDemand=e.baselineWaterDemand, waterSupplied=e.baselineWaterSupply, wastewaterLoad=e.baselineWastewaterLoad, wastewaterTreated=e.baselineWastewaterCapacity };
            foreach(var row in outcomes.sites.Where(r => !string.IsNullOrEmpty(r.actionId)))
            {
                var recipe = definition.actions.First(a => a.siteId == row.siteId && a.actionId == row.actionId);
                var s = recipe.services; double active = Active(row);
                v.homesBuilt += s.homes*active; v.jobs += s.jobs*active; v.recreation += s.recreation*active; v.localServices += s.localServices*active;
                v.energyDemand += s.energyDemand*active; v.waterDemand += s.waterDemand*active; v.wastewaterLoad += s.wastewaterLoad*active;
                // Saved adverse growing conditions also represent disclosed renewable variability.
                double supply = s.cleanEnergySupply > 0 ? Math.Min(1,driver.productionFactor) : 1;
                v.energySupplied += s.energySupply*active*supply; v.cleanEnergySupplied += s.cleanEnergySupply*active*supply;
                v.wastewaterTreated += s.wastewaterCapacity*active;
                v.emissionsPressure += s.emissionsPressure*active;
            }
            foreach (var row in outcomes.sites)
                if(driver.productionFactor < .9 && row.productionUnits > 0 && (string.IsNullOrEmpty(row.actionId) || new[]{"cultivate-standard","cultivate-margin","conservation-rotation","cover-crop"}.Contains(row.actionId)))
                { row.disrupted=true; row.explanation=driver.description+"; production follows the saved growing condition."; }
            outcomes.disruptedSites=outcomes.sites.Count(r=>r.disrupted);
            double power = Met(v.energySupplied,v.energyDemand), water = Met(v.waterSupplied,v.waterDemand);
            v.resourceFraction = Math.Min(power,water);
            // Scarcity proportionally curtails served work and occupied homes. Capacity is not delivered service.
            v.occupiedHomes = Math.Min(e.householdDemand,v.homesBuilt)*v.resourceFraction;
            v.jobs *= v.resourceFraction; v.localServices *= v.resourceFraction;
            v.energySupplied = Math.Min(v.energySupplied,v.energyDemand);
            v.cleanEnergySupplied = Math.Min(v.cleanEnergySupplied,v.energySupplied);
            v.waterSupplied = Math.Min(v.waterSupplied,v.waterDemand);
            v.wastewaterTreated = Math.Min(v.wastewaterLoad,v.wastewaterTreated*power);
            v.recreation = Math.Min(v.recreation,e.recreationTarget);
            v.localServices = Math.Min(v.localServices,e.localServicesTarget);
            v.serviceFraction = (power + water + Met(v.wastewaterTreated,v.wastewaterLoad) + Met(v.recreation,e.recreationTarget) + Met(v.localServices,e.localServicesTarget))/5;
            long due = checked(ledger.openingArrearsCents + ledger.programDueCents + ledger.operatingDueCents + ledger.repairDueCents);
            long paid = checked(ledger.arrearsPaidCents + ledger.programPaidCents + ledger.operatingPaidCents + ledger.repairPaidCents);
            v.fundedFraction = due==0?1:(double)paid/due;
            var runoff = Water(definition,outcomes); var baseline = RunoffEvaluator.Evaluate(e.referenceStorm);
            v.generatedM3=runoff.generatedM3;v.retainedM3=runoff.retainedM3;v.dischargedM3=runoff.dischargedM3;v.baselineDischargedM3=baseline.dischargedM3;
            double runoffScore = baseline.dischargedM3 == 0 ? (runoff.dischargedM3==0?1:0) : Cap((baseline.dischargedM3-runoff.dischargedM3)/(baseline.dischargedM3*e.runoffReductionTarget));
            double clean = Met(v.cleanEnergySupplied,v.energyDemand)*(1-Cap(v.emissionsPressure/e.emissionsLimit));
            double waterPressure = (water + Met(v.wastewaterTreated,v.wastewaterLoad))/2;
            outcomes.economicScore=100*(.4*Cap(outcomes.productionUnits/definition.rubric.productionTargetUnits)+.3*Cap(v.jobs/e.jobsTarget)+.3*v.fundedFraction);
            outcomes.socialScore=100*(.4*Cap(v.occupiedHomes/e.householdDemand)+.4*v.serviceFraction+.2*(1-(double)outcomes.disruptedSites/definition.sites.Length));
            outcomes.environmentalScore=100*(.4*Cap(outcomes.vegetatedAreaM2/outcomes.managedAreaM2/e.coverTargetFraction)+.25*runoffScore+.2*clean+.15*waterPressure);
            outcomes.economicExplanation="40% output need + 30% sustained jobs + 30% due obligations funded. Jobs and outputs are illustrative; private activity is not treasury income.";
            outcomes.socialExplanation="40% occupied housing + 40% energy, drinking water, wastewater, recreation and local-service needs (equal subweights) + 20% undisrupted sites. Resources limit occupied homes.";
            outcomes.environmentalExplanation="40% cover/condition + 25% same-storm discharge reduction + 20% low-emissions supply + 15% water/treatment needs. All except reference runoff are authored indices, not ecological or health forecasts.";
            v.reasons=new[]{"Homes occupied "+v.occupiedHomes.ToString("0.0")+" / "+e.householdDemand+"; resources supplied "+(100*v.resourceFraction).ToString("0")+"%.","Sustained jobs "+v.jobs.ToString("0.0")+" / "+e.jobsTarget+"; obligations funded "+(100*v.fundedFraction).ToString("0")+"%.","Reference-storm discharge "+v.dischargedM3.ToString("0")+" m³; wastewater treated "+v.wastewaterTreated.ToString("0.0")+" / "+v.wastewaterLoad.ToString("0.0")+" scenario units."};
            outcomes.economy=v;
        }
    }
}
