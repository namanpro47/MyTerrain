using System;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    public static class AnnualSimulation
    {
        // Hash preimage: complete accepted report with only this hash field cleared.
        // The report version plus accepted definition/rule identity provide the domain.
        public static string ReportHash(TownYearReport report)
        {
            AnnualTown.Require(report?.simulation!=null&&report.simulation.version=="annual-numeric-report-v1"&&report.acceptedRequest?.simulation!=null
                &&report.ledger!=null&&report.outcomes!=null&&report.simulation.closingState?.sites!=null&&report.simulation.closingState.repairs!=null
                &&report.simulation.crops!=null&&report.simulation.impacts!=null&&report.simulation.storage!=null&&report.simulation.payments!=null,"Incomplete accepted numeric report.");
            var copy=TownCopies.Copy(report);copy.simulation.transitionHash=null;return TownCopies.Hash(copy);
        }
        public static void ValidateReportHash(TownYearReport report)
            =>AnnualTown.Require(!string.IsNullOrWhiteSpace(report?.simulation?.transitionHash)&&report.simulation.transitionHash==ReportHash(report),"Accepted numeric report hash changed.");
        public static TownYearOutcomes OpeningOutcomes(AnnualTownDefinition definition)
        {
            AnnualTown.Require(definition?.simulation!=null,"Numeric opening baseline requires its pinned definition.");
            var rows=definition.sites.Select(s=>new TownSiteYearResult {siteId=s.id,phase=TownAssetPhase.Operating,maturity=1,condition=1,
                productionUnits=s.baselineProductionUnits,serviceUnits=s.baselineServiceUnits,vegetatedAreaM2=s.areaM2*s.baselineVegetatedFraction,
                explanation="Opening illustrative capacity before realized weather; not a promised harvest."}).ToArray();
            var o=new TownYearOutcomes {rubricVersion=definition.rubric.version,sites=rows,productionUnits=rows.Sum(r=>r.productionUnits),serviceUnits=rows.Sum(r=>r.serviceUnits),
                vegetatedAreaM2=rows.Sum(r=>r.vegetatedAreaM2),managedAreaM2=definition.sites.Sum(s=>s.areaM2)};
            AnnualEconomy.Evaluate(definition,o,new TownYearLedger(),new TownYearDriver {productionFactor=1});return o;
        }
        internal static double Cap(double value) => Math.Max(0,Math.Min(1,value));
        static void Range(double value,double min,double max,string name)
            => AnnualTown.Require(!double.IsNaN(value)&&!double.IsInfinity(value)&&value>=min&&value<=max,"Invalid simulation "+name);
        public static void Validate(AnnualTownDefinition definition)
        {
            var d=definition.simulation;
            AnnualTown.Require(definition.schemaVersion==3&&definition.version=="4.0.0"&&definition.years==10&&d!=null&&d.version=="annual-numeric-v4.0.0","Unsupported numeric simulation version.");
            AnnualTown.Require(d.disclosure==AnnualSimulationWeather.Disclosure&&!string.IsNullOrWhiteSpace(d.assumptions),"Missing common numerical reference disclosure.");
            AnnualTown.Require(new[]{"low","central","high"}.Contains(d.sensitivity)&&d.sensitivityFactor==(d.sensitivity=="low"?.7:d.sensitivity=="high"?1.3:1),"Unknown response sensitivity.");
            var w=d.weather;
            AnnualTown.Require(w!=null&&w.packId==AnnualSimulationWeather.PackId&&w.packSha256==AnnualSimulationWeather.PackHash&&w.dailySha256==AnnualSimulationWeather.DailyHash
                &&w.samplerVersion==AnnualSimulationWeather.Sampler&&!string.IsNullOrEmpty(w.sharedScenarioSeed)&&w.recordIds?.Length==10,"Unpinned simulation weather.");
            AnnualTown.Require(w.startYear==AnnualSimulationWeather.ExpectedStartYear(w.sharedScenarioSeed)&&w.recordIds.SequenceEqual(Enumerable.Range(w.startYear,10).Select(y=>"USW00014920:"+y)),"Noncontiguous saved weather.");
            AnnualTown.Require(d.crops!=null&&d.crops.Length>0&&d.crops.All(c=>c!=null)&&d.crops.Select(c=>c.id).Distinct().Count()==d.crops.Length,"Missing/duplicate crop definitions.");
            foreach(var crop in d.crops)
            {
                AnnualTown.Require(!string.IsNullOrWhiteSpace(crop.id)&&crop.units=="illustrative production units"&&crop.evidenceLevel=="authored-scenario"&&!string.IsNullOrWhiteSpace(crop.ruleId),"Unlabelled crop curve.");
                int first=new DateTime(2001,crop.sowMonth,crop.sowDay).DayOfYear,last=new DateTime(2001,crop.harvestMonth,crop.harvestDay).DayOfYear;
                AnnualTown.Require(first<last,"Unsupported crop calendar.");
                Range(crop.outputPerHa,.001,100,"crop benchmark");Range(crop.referenceRainMm,1,2000,"crop rain reference");
                Range(crop.droughtLossPerDay,0,.1,"dry-spell response");Range(crop.heatLossPerDay,0,.1,"heat response");Range(crop.excessRainLossPerMm,0,.01,"excess-rain response");
            }
            AnnualTown.Require(d.practices?.Length==4&&d.practices.All(p=>p!=null)&&d.practices.Select(p=>p.actionId).OrderBy(x=>x,StringComparer.Ordinal).SequenceEqual(new[]{"conservation-rotation","cover-crop","cultivate-margin","cultivate-standard"}),"Incomplete crop practices.");
            foreach(var practice in d.practices)
            {
                AnnualTown.Require(practice.cropSequence?.Length>0&&practice.cropSequence.All(id=>d.crops.Any(c=>c.id==id))&&!string.IsNullOrWhiteSpace(practice.ruleId),"Unknown crop in rotation.");
                Range(practice.croppedFraction,.01,1,"cropped fraction");Range(practice.matureDroughtMitigation,0,1,"drought mitigation");
                Range(practice.matureRainVulnerability,0,1,"rain vulnerability");Range(practice.establishmentCostFraction,0,.5,"practice transition cost");
            }
            AnnualTown.Require(d.sites!=null&&d.sites.Length==definition.sites.Length&&d.sites.All(s=>s!=null)&&d.sites.Select(s=>s.siteId).Distinct().Count()==d.sites.Length,"Every site requires explicit exposure.");
            foreach(var site in d.sites)
            {
                AnnualTown.Require(definition.sites.Any(s=>s.id==site.siteId)&&!string.IsNullOrWhiteSpace(site.connectionGroupId)&&!string.IsNullOrWhiteSpace(site.ruleId),"Unknown exposure site/group.");
                AnnualTown.Require(site.connectionGroupId==definition.economy.referenceStorm.sites.Single(x=>x.siteId==site.siteId).contributingAreaId,"Exposure group differs from the declared connection map.");
                Range(site.exposure,.001,1,"exposure; missing is not zero risk");Range(site.baselineCroppedFraction,0,1,"baseline cropped fraction");
                AnnualTown.Require(site.baselineCroppedFraction==0?string.IsNullOrEmpty(site.baselineCropId):d.crops.Any(c=>c.id==site.baselineCropId),"Missing baseline crop.");
            }
            AnnualTown.Require(d.drivers?.Length==10,"One frozen driver per year is required.");
            for(int i=0;i<d.drivers.Length;i++)
            {
                var driver=d.drivers[i];
                AnnualTown.Require(driver!=null&&driver.year==i+1&&driver.historicalYear==w.startYear+i&&driver.recordId==w.recordIds[i]&&driver.packSha256==w.packSha256
                    &&driver.derivationVersion=="terra-crop-calendar-weather-v1"&&driver.crops?.Length==d.crops.Length&&driver.events!=null,"Mismatched annual weather driver.");
                AnnualSimulationWeather.ValidateDriver(driver);
                AnnualTown.Require(driver.solarFactor==1&&driver.windFactor==1&&driver.generationRuleId=="authored-constant-generation-v1-no-irradiance-or-wind-data","Unsupported generation assumption.");
                foreach(var crop in d.crops)
                {
                    var seasons=driver.crops.Where(c=>c.cropId==crop.id).ToArray();AnnualTown.Require(seasons.Length==1,"Missing crop weather window.");
                    var season=seasons[0];
                    AnnualTown.Require(season.firstDay==new DateTime(driver.historicalYear,crop.sowMonth,crop.sowDay).DayOfYear
                        &&season.harvestDay==new DateTime(driver.historicalYear,crop.harvestMonth,crop.harvestDay).DayOfYear&&season.ruleId=="authored-crop-calendar-v1","Wrong crop weather calendar.");
                    Range(season.precipTenthsMm,0,100000,"season precipitation");Range(season.hotDays,0,season.harvestDay-season.firstDay+1,"hot days");Range(season.longestDrySpellDays,0,season.harvestDay-season.firstDay+1,"dry spell");
                }
                int previous=0;
                foreach(var e in driver.events)
                {
                    AnnualTown.Require(e!=null&&e.dayOfYear>previous&&e.dayOfYear<=(DateTime.IsLeapYear(driver.historicalYear)?366:365)&&!string.IsNullOrWhiteSpace(e.recordId),"Unordered/unknown weather event.");
                    var date=new DateTime(driver.historicalYear,1,1).AddDays(e.dayOfYear-1);
                    AnnualTown.Require(e.month==date.Month&&e.date==date.ToString("yyyy-MM-dd",System.Globalization.CultureInfo.InvariantCulture)&&e.tminTenthsC<=e.tmaxTenthsC,"Invalid weather event date/temperature.");
                    Range(e.precipTenthsMm,250,100000,"event precipitation");Range(e.antecedent7DaysTenthsMm,0,100000,"antecedent precipitation");previous=e.dayOfYear;
                }
                AnnualTown.Require(definition.drivers[i].id==driver.recordId&&definition.drivers[i].productionFactor==1&&definition.drivers[i].unexpectedPublicRepairCents==0,"Legacy weather/repair penalty leaked into numeric rules.");
            }
            AnnualTown.Require(d.publicResponsibilities?.Length==3&&d.publicResponsibilities.All(r=>r!=null)&&d.publicResponsibilities.Select(r=>r.actionId).OrderBy(x=>x,StringComparer.Ordinal).SequenceEqual(new[]{"public-park","wastewater-upgrade","water-swale"}),"Duplicate public repair responsibility.");
            foreach(var responsibility in d.publicResponsibilities)
                AnnualTown.Require(definition.actions.Any(a=>a.actionId==responsibility.actionId)&&responsibility.repairCentsPerConditionUnit>0&&responsibility.repairCentsPerConditionUnit<=1000000000
                    &&!string.IsNullOrWhiteSpace(responsibility.responsibility)&&!string.IsNullOrWhiteSpace(responsibility.ruleId),"Missing/bounded public repair responsibility.");
            Range(d.dailyStorageRecoveryFraction,0,1,"storage recovery");Range(d.privateAnnualRecovery,0,.25,"private recovery");Range(d.drySpellGraceDays,0,60,"dry-spell grace");
            Range(d.rainDamageThresholdMm,1,1000,"rain threshold");Range(d.localRainDamagePerMm,0,.1,"local damage");Range(d.upstreamRainThresholdMm,1,1000,"upstream threshold");Range(d.upstreamDamagePerMm,0,.1,"upstream damage");
            Range(d.antecedentAmplificationPerMm,0,.1,"antecedent amplification");Range(d.maximumEventDamage,0,.5,"event damage cap");Range(d.minimumWeatherFactor,0,1,"weather floor");Range(d.maximumWeatherFactor,1,2,"weather ceiling");
            AnnualTown.Require(!string.IsNullOrWhiteSpace(d.eventRuleId)&&!string.IsNullOrWhiteSpace(d.recoveryRuleId),"Missing event/recovery rule identity.");
        }
        internal static TownSimulationState Initial(AnnualTownDefinition definition)
            => definition.simulation==null?null:new TownSimulationState {sites=definition.simulation.sites.Select(s=>new TownSimulationSiteState {siteId=s.siteId,cropId=s.baselineCropId,condition=1}).ToArray(),repairs=Array.Empty<TownRepairLiability>()};
    }
}
