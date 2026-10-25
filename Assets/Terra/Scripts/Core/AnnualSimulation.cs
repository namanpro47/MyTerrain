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
        // Planning envelope uses public rules, choices and saved condition only. It never
        // reads the unplayed year's driver, resolves a copy or implies a probability interval.
        public static TownPlanningRange PlanningRange(AnnualTown town)
        {
            AnnualTown.Require(town!=null&&town.Definition.simulation!=null&&(town.Phase==TownPhase.Planning||town.Phase==TownPhase.Resolving),"A numeric unplayed year is required.");
            var d=town.Definition;var snapshot=town.Snapshot();
            var result=new TownPlanningRange {year=town.Year,queuedUpfrontCents=town.Queue.Sum(q=>d.actions.Single(a=>a.siteId==q.siteId&&a.actionId==q.actionId).upfrontPublicCostCents),
                totalReservedCents=checked(town.CommittedCents+town.QueuedCommitmentCents),availableCents=town.AvailableCents,
                units="illustrative production units",disclosure="Conditional output envelope, not a probability or promised harvest. Upper bound assumes funded operation and no new damage. Weather, condition and funding can reduce output to zero. Queued upfront cost is paid only when End Year is accepted.",
                sites=d.sites.Select(site=>{
                    var asset=town.Assets.FirstOrDefault(a=>a.siteId==site.id);var queued=town.Queue.FirstOrDefault(q=>q.siteId==site.id);
                    string actionId=asset?.actionId??queued?.actionId;
                    var recipe=actionId==null?null:d.actions.Single(a=>a.siteId==site.id&&a.actionId==actionId);
                    var practice=recipe==null?null:d.simulation.practices.FirstOrDefault(p=>p.actionId==recipe.actionId);
                    int started=asset?.startedYear??town.Year;
                    bool active=recipe==null||town.Year>=started+recipe.constructionYears&&town.Year<started+recipe.constructionYears+recipe.operatingYears;
                    var exposure=d.simulation.sites.Single(x=>x.siteId==site.id);
                    string cropId=exposure.baselineCropId;
                    double area=site.areaM2*exposure.baselineCroppedFraction*(recipe==null?1:1-recipe.services.occupiedAreaM2/site.areaM2);
                    if(practice!=null)
                    {
                        cropId=practice.cropSequence[Math.Max(0,town.Year-started-recipe.constructionYears)%practice.cropSequence.Length];
                        area=active?site.areaM2*practice.croppedFraction:0;
                    }
                    double agricultural=area<=0?0:area/10000*d.simulation.crops.Single(c=>c.id==cropId).outputPerHa*d.simulation.maximumWeatherFactor;
                    double other=recipe==null||practice!=null||!active?0:Math.Max(0,recipe.productionUnits-site.baselineProductionUnits*(1-recipe.services.occupiedAreaM2/site.areaM2));
                    return new TownSiteProductionRange {siteId=site.id,cropId=area>0?cropId:null,practiceId=practice?.actionId??"baseline",cropOperating=area>0,croppedAreaM2=area,
                        minimumProductionUnits=0,maximumProductionUnits=(agricultural+other)*snapshot.simulation.sites.Single(x=>x.siteId==site.id).condition};
                }).ToArray()};
            return result;
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
            AnnualTown.Require(d.storageRecoveryRuleId=="linear-capacity-drain-per-day-v1","Unknown storage recovery equation.");
        }
        // Required numeric state is never normalized into a legacy/empty state.
        public static void ValidateState(AnnualTownDefinition definition,TownSimulationState state,int completedYear,long arrears)
        {
            var d=definition.simulation;
            AnnualTown.Require(d!=null&&completedYear>=0&&completedYear<=definition.years&&arrears>=0,"Invalid numeric state context.");
            AnnualTown.Require(state?.sites!=null&&state.repairs!=null&&state.sites.Length==definition.sites.Length
                &&state.sites.All(s=>s!=null)&&state.sites.Select(s=>s.siteId).Distinct().Count()==state.sites.Length,"Incomplete/duplicate numeric site state.");
            foreach(var s in state.sites)
            {
                AnnualTown.Require(definition.sites.Any(site=>site.id==s.siteId)&&s.rotationPhase>=0
                    &&s.rotationPhase<d.practices.Max(p=>p.cropSequence.Length)
                    &&(string.IsNullOrEmpty(s.cropId)||d.crops.Any(c=>c.id==s.cropId)),"Unknown numeric site/crop/rotation.");
                Range(s.condition,0,1,"site condition");Range(s.outstandingDamage,0,1,"outstanding damage");
                Range(s.recoveredLastYear,0,1,"year-end recovery");Range(s.storedWaterM3,0,1e9,"occupied storage");
                AnnualTown.Require(Math.Abs(s.outstandingDamage-(1-s.condition))<1e-9,"Numeric condition authority disagrees.");
            }
            AnnualTown.Require(state.repairs.All(r=>r!=null)&&state.repairs.Select(r=>r.id).Distinct().Count()==state.repairs.Length,"Duplicate/missing repair liability.");
            long debt=0;
            foreach(var r in state.repairs)
            {
                AnnualTown.Require(!string.IsNullOrWhiteSpace(r.id)&&r.createdYear>=1&&r.createdYear<=completedYear
                    &&state.sites.Any(s=>s.siteId==r.siteId)&&definition.actions.Any(a=>a.siteId==r.siteId&&a.actionId==r.actionId),"Unknown repair site/action/year.");
                var duty=d.publicResponsibilities.FirstOrDefault(p=>p.actionId==r.actionId);
                AnnualTown.Require(duty!=null&&r.responsibility==duty.responsibility&&r.ruleId==duty.ruleId
                    &&d.drivers[r.createdYear-1].events.Any(e=>e.recordId==r.eventId),"Unknown public repair cause.");
                AnnualTown.Require(r.originalCents>0&&r.originalCents<=1000000000&&r.unpaidCents>0&&r.unpaidCents<=r.originalCents,"Invalid public repair debt.");
                Range(r.originalConditionLoss,0,1,"original repair loss");Range(r.unrecoveredCondition,0,r.originalConditionLoss,"unrecovered repair loss");
                AnnualTown.Require(Math.Abs(r.unrecoveredCondition-r.originalConditionLoss*r.unpaidCents/r.originalCents)<1e-9,"Repair recovery does not match cumulative payment.");
                debt=checked(debt+r.unpaidCents);
            }
            AnnualTown.Require(debt<=arrears,"Itemized repair debt exceeds public arrears.");
            foreach(var s in state.sites)
                AnnualTown.Require(state.repairs.Where(r=>r.siteId==s.siteId).Sum(r=>r.unrecoveredCondition)<=s.outstandingDamage+1e-9,"Unpaid public damage was erased.");
        }
        public static void ValidateReport(AnnualTownDefinition definition,TownYearReport report)
        {
            AnnualTown.Require(definition?.simulation!=null,"Numeric report requires its definition.");
            ReportHash(report); // Required shape and pinned report hash encoding, before field checks.
            var s=report.simulation;var r=report.acceptedRequest;var l=report.ledger;
            AnnualTown.Require(report.year>=1&&report.year<=10&&r.year==report.year&&s.transitionId==r.requestId
                &&r.simulation.rulesVersion==definition.simulation.version
                &&TownCopies.Hash(s.driver)==TownCopies.Hash(definition.simulation.drivers[report.year-1])
                &&TownCopies.Hash(r.simulation.driver)==TownCopies.Hash(s.driver),"Numeric report identity/driver differs.");
            ValidateState(definition,s.closingState,report.year,l.closingArrearsCents);
            foreach(double delta in new[]{s.economicDelta,s.socialDelta,s.environmentalDelta})Range(delta,-100,100,"score change");
            foreach(double deficit in new[]{s.energyDeficit,s.waterDeficit,s.wastewaterDeficit,s.housingDeficit,s.jobsDeficit})Range(deficit,0,1e12,"unmet demand");
            AnnualTown.Require(s.crops.All(c=>c!=null)&&s.crops.Select(c=>c.siteId).Distinct().Count()==s.crops.Length,"Duplicate/missing harvest.");
            foreach(var c in s.crops)
            {
                var crop=definition.simulation.crops.FirstOrDefault(x=>x.id==c.cropId);
                AnnualTown.Require(crop!=null&&definition.sites.Any(x=>x.id==c.siteId)&&c.units==crop.units&&c.ruleId==crop.ruleId
                    &&(c.practiceId=="baseline"||definition.simulation.practices.Any(p=>p.actionId==c.practiceId)),"Unknown harvest cause.");
                Range(c.productiveAreaM2,0,definition.sites.Single(x=>x.id==c.siteId).areaM2,"cropped area");
                Range(c.weatherFactor,definition.simulation.minimumWeatherFactor,definition.simulation.maximumWeatherFactor,"harvest weather");
                foreach(double value in new[]{c.openingCondition,c.harvestCondition,c.closingConditionBeforeRecovery,c.nextYearCondition,c.managementFactor})Range(value,0,1,"harvest condition");
                AnnualTown.Require(c.benchmarkPerHa==crop.outputPerHa&&c.harvestDay==s.driver.crops.Single(x=>x.cropId==c.cropId).harvestDay
                    &&c.outputUnits==c.productiveAreaM2/10000*c.benchmarkPerHa*c.weatherFactor*c.managementFactor*c.harvestCondition,"Harvest equation differs.");
            }
            AnnualTown.Require(s.impacts.All(i=>i!=null)&&s.impacts.Select(i=>i.siteId+"/"+i.eventId).Distinct().Count()==s.impacts.Length,"Duplicate/missing event impact.");
            foreach(var i in s.impacts)
            {
                var e=s.driver.events.FirstOrDefault(x=>x.recordId==i.eventId);
                AnnualTown.Require(e!=null&&e.dayOfYear==i.dayOfYear&&i.ruleId==definition.simulation.eventRuleId
                    &&definition.simulation.sites.Any(x=>x.siteId==i.siteId&&x.connectionGroupId==i.connectionGroupId),"Unknown event impact cause.");
                Range(i.localHazard,0,1e9,"local hazard");Range(i.upstreamHazard,0,1e9,"upstream hazard");
                Range(i.connectedStorageReductionFraction,0,1,"retention reduction");Range(i.conditionLoss,0,1,"event condition loss");
                var crop=s.crops.FirstOrDefault(c=>c.siteId==i.siteId);
                AnnualTown.Require(i.beforeHarvest==(crop!=null&&i.dayOfYear<=crop.harvestDay),"Harvest event timing differs.");
            }
            AnnualTown.Require(s.storage.All(x=>x!=null)&&s.storage.Select(x=>x.connectionGroupId+"/"+x.eventId).Distinct().Count()==s.storage.Length,"Duplicate/missing group storage.");
            foreach(var x in s.storage)
            {
                AnnualTown.Require(x.ruleId==definition.simulation.storageRecoveryRuleId&&s.driver.events.Any(e=>e.recordId==x.eventId)
                    &&definition.simulation.sites.Any(site=>site.connectionGroupId==x.connectionGroupId)&&x.deviceSiteIds!=null
                    &&x.deviceSiteIds.Distinct().Count()==x.deviceSiteIds.Length&&x.deviceSiteIds.All(id=>definition.simulation.sites.Any(site=>site.siteId==id&&site.connectionGroupId==x.connectionGroupId)),"Unknown storage cause/device.");
                foreach(double value in new[]{x.capacityM3,x.openingStoredM3,x.recoveredM3,x.capacityLossReleaseM3,x.inflowM3,x.capturedM3,x.closingStoredM3})Range(value,0,1e12,"event storage");
                AnnualTown.Require(Math.Abs(x.openingStoredM3-x.recoveredM3-x.capacityLossReleaseM3+x.capturedM3-x.closingStoredM3)<.00001
                    &&x.capturedM3<=x.inflowM3&&x.closingStoredM3<=x.capacityM3+.00001,"Storage conservation/capacity differs.");
            }
            AnnualTown.Require(s.payments.All(p=>p!=null)&&s.payments.Select(p=>p.id).Distinct().Count()==s.payments.Length,"Duplicate/missing payment.");
            foreach(var p in s.payments)
            {
                AnnualTown.Require(!string.IsNullOrWhiteSpace(p.id)&&p.dueCents>=0&&p.paidCents>=0&&p.paidCents<=p.dueCents
                    &&new[]{"upfront","prior-program-or-operation-arrears","prior-repair-arrears","program","operation","new-repair"}.Contains(p.kind),"Invalid payment.");
                Range(p.nextYearRecovery,0,1,"paid recovery");
                AnnualTown.Require(p.kind=="prior-program-or-operation-arrears"||definition.actions.Any(a=>a.siteId==p.siteId&&a.actionId==p.actionId),"Unknown payment site/action.");
            }
            AnnualTown.Require(s.payments.Sum(p=>p.paidCents)==checked(l.upfrontPaidCents+l.arrearsPaidCents+l.programPaidCents+l.operatingPaidCents+l.repairPaidCents)
                &&s.payments.Sum(p=>p.dueCents)==checked(l.upfrontPaidCents+l.openingArrearsCents+l.programDueCents+l.operatingDueCents+l.repairDueCents),"Itemized payments differ from ledger.");
        }
        internal static TownSimulationState Initial(AnnualTownDefinition definition)
            => definition.simulation==null?null:new TownSimulationState {sites=definition.simulation.sites.Select(s=>new TownSimulationSiteState {siteId=s.siteId,cropId=s.baselineCropId,condition=1}).ToArray(),repairs=Array.Empty<TownRepairLiability>()};
    }
}
