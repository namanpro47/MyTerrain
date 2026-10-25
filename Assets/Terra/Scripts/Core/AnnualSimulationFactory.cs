using System;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    public static class AnnualSimulationFactory
    {
        // A new definition/save identity. The supplied legacy definition is never mutated.
        public static AnnualTownDefinition Create(AnnualTownDefinition legacy, AnnualSimulationWeather weather, string seed, string sensitivity = "central")
        {
            AnnualTown.Require(legacy != null && legacy.schemaVersion == 2 && legacy.simulation == null, "Upgrade an explicit legacy full-game definition once.");
            AnnualTown.Require(new[]{"low","central","high"}.Contains(sensitivity), "Choose a pinned low/central/high response sensitivity.");
            var d = TownCopies.Copy(legacy);
            d.schemaVersion = 3; d.version = "4.0.0";
            d.assumptions = AnnualSimulationWeather.Disclosure + " Crop output, exposure, response curves and repair duties are illustrative scenario rules. No calibrated yield, flood depth, probability or private crop income enters public cash.";
            var crops = new[] {
                new TownCropDefinition { id="corn",units="illustrative production units",evidenceLevel="authored-scenario",ruleId="corn-curve-v1",sowMonth=5,sowDay=1,harvestMonth=10,harvestDay=15,outputPerHa=4,referenceRainMm=500,droughtLossPerDay=.015,heatLossPerDay=.012,excessRainLossPerMm=.0004 },
                new TownCropDefinition { id="soy",units="illustrative production units",evidenceLevel="authored-scenario",ruleId="soy-curve-v1",sowMonth=5,sowDay=10,harvestMonth=10,harvestDay=1,outputPerHa=3,referenceRainMm=420,droughtLossPerDay=.012,heatLossPerDay=.008,excessRainLossPerMm=.0004 }
            };
            TownPracticeDefinition Practice(string id, string[] sequence, double area, double drought, double rain, double transition)
                => new TownPracticeDefinition {actionId=id,ruleId="practice-response-v1",cropSequence=sequence,croppedFraction=area,matureDroughtMitigation=drought,matureRainVulnerability=rain,establishmentCostFraction=transition};
            var schedule = weather.Select(seed);
            d.simulation = new TownSimulationDefinition {
                version="annual-numeric-v4.0.0",disclosure=AnnualSimulationWeather.Disclosure,sensitivity=sensitivity,sensitivityFactor=sensitivity=="low"?.7:sensitivity=="high"?1.3:1,
                assumptions="All crop calendars/curves, site exposure and recovery coefficients are authored sensitivity assumptions. PRCP is liquid equivalent; May–October events with nonfreezing daily minimum are interpreted as illustrative rain. Other seasons/phase combinations are outside this disruption model, not zero real-world risk. Connections are existing hypothetical drainage groups. No adjacency health effect is inferred. Solar and wind have separate constant generation assumptions because observations are unavailable.",
                weather=schedule,crops=crops,drivers=Enumerable.Range(1,10).Select(y=>weather.Driver(schedule,y,crops)).ToArray(),
                practices=new[]{Practice("cultivate-standard",new[]{"corn"},1,0,1,0),Practice("cover-crop",new[]{"corn"},1,.3,.75,.08),Practice("conservation-rotation",new[]{"corn","soy"},1,.2,.8,.06),Practice("cultivate-margin",new[]{"corn"},.75,.1,.65,.04)},
                sites=d.sites.Select(s=>new TownExposureDefinition {siteId=s.id,connectionGroupId=d.economy.referenceStorm.sites.Single(r=>r.siteId==s.id).contributingAreaId,
                    baselineCropId=s.baselineProductionUnits>0?"corn":null,baselineCroppedFraction=s.baselineProductionUnits>0?1:0,exposure=.6,ruleId="authored-uniform-exposure-connected-groups-v1"}).ToArray(),
                publicResponsibilities=new[]{
                    new TownPublicResponsibility {actionId="water-swale",responsibility="Scenario public drainage asset",repairCentsPerConditionUnit=5000000,ruleId="public-repair-duty-v1"},
                    new TownPublicResponsibility {actionId="public-park",responsibility="Scenario public park asset",repairCentsPerConditionUnit=2500000,ruleId="public-repair-duty-v1"},
                    new TownPublicResponsibility {actionId="wastewater-upgrade",responsibility="Scenario public wastewater asset",repairCentsPerConditionUnit=50000000,ruleId="public-repair-duty-v1"}},
                dailyStorageRecoveryFraction=.1,privateAnnualRecovery=.08,drySpellGraceDays=12,rainDamageThresholdMm=25,localRainDamagePerMm=.002,
                upstreamRainThresholdMm=45,upstreamDamagePerMm=.001,antecedentAmplificationPerMm=.002,maximumEventDamage=.25,minimumWeatherFactor=.35,maximumWeatherFactor=1.1,
                eventRuleId="illustrative-connected-rain-v1",recoveryRuleId="year-end-recovery-next-year-v1",storageRecoveryRuleId="linear-capacity-drain-per-day-v1"
            };
            // Remove the old fixed Year-3/7 loss and universal repair bill in this version only.
            d.drivers = d.simulation.drivers.Select(x=>new TownYearDriver {year=x.year,id=x.recordId,evidenceLevel="historical-regional-weather-authored-response",productionFactor=1,
                unexpectedPublicRepairCents=0,description="Saved La Crosse reference year "+x.historicalYear+"; illustrative crop and connected-rain responses."}).ToArray();
            var originalOutput=d.sites.ToDictionary(s=>s.id,s=>s.baselineProductionUnits);
            foreach(var site in d.sites)
                site.baselineProductionUnits=site.areaM2/10000*4*d.simulation.sites.Single(s=>s.siteId==site.id).baselineCroppedFraction;
            foreach(var action in d.actions)
            {
                var site=d.sites.Single(s=>s.id==action.siteId);
                var practice=d.simulation.practices.FirstOrDefault(p=>p.actionId==action.actionId);
                if(practice!=null)action.productionUnits=site.areaM2/10000*4*practice.croppedFraction;
                else
                {
                    double remaining=1-action.services.occupiedAreaM2/site.areaM2;
                    double nonCropCapacity=Math.Max(0,action.productionUnits-originalOutput[site.id]*remaining);
                    action.productionUnits=site.baselineProductionUnits*remaining+nonCropCapacity;
                }
                // No permanent rotation premium; the annual crop sequence and conditional
                // response determine realized output. This field is a declared capacity only.
            }
            d.rubric.version="full-rubric-numeric-v4";
            d.rubric.explanation="Numeric v4: existing Economic/Environmental service rubric; Social 40% occupied housing, 40% service needs, 20% mean time-weighted site condition as an authored continuity proxy. Any recorded rain impact remains visible; tiny impacts do not imply a full year of lost service. Not measured welfare.";
            AnnualTown.ValidateDefinition(d);
            return d;
        }
    }
}
