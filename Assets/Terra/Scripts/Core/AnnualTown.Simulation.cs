using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    public sealed partial class AnnualTown
    {
        sealed class NumericDraft
        {
            public TownSimulationReport report;
            public TownYearOutcomes outcomes;
            public long newRepairDue;
            public List<TownRepairLiability> repairs;
        }
        NumericDraft PrepareNumeric(TownAsset[] nextAssets,Dictionary<string,bool> funded,TownYearConsequences effects,TownYearLedger ledger)
        {
            var rules=definition.simulation;
            Require(pending.simulation!=null&&pending.simulation.rulesVersion==rules.version
                &&TownCopies.Hash(pending.simulation.openingState)==TownCopies.Hash(numericState)
                &&TownCopies.Hash(pending.simulation.driver)==TownCopies.Hash(rules.drivers[Year-1]),"Frozen numeric inputs changed.");
            var driver=pending.simulation.driver;
            var state=TownCopies.Copy(numericState);
            var outcomes=EvaluateOutcomes(nextAssets,funded,effects,ledger);
            var rows=outcomes.sites.ToDictionary(r=>r.siteId,StringComparer.Ordinal);
            var states=state.sites.ToDictionary(s=>s.siteId,StringComparer.Ordinal);
            var opening=new Dictionary<string,double>();
            var meanLoss=new Dictionary<string,double>();
            var harvestCondition=new Dictionary<string,double>();
            var cropResults=new List<TownCropResult>();
            var impacts=new List<TownEventImpact>();
            var storage=new List<TownEventStorage>();
            var repairs=TownCopies.Copy(state.repairs).ToList();
            var initialCapacity=outcomes.sites.ToDictionary(r=>r.siteId,r=>r.retentionCapacityM3,StringComparer.Ordinal);
            double CoverFraction(string id,double condition)
            {
                var site=sites[id];var asset=nextAssets.FirstOrDefault(a=>a.siteId==id);
                double potential=site.baselineVegetatedFraction;
                if(asset!=null)potential+=(Action(id,asset.actionId).vegetatedFraction-potential)*rows[id].maturity;
                return Clamp01(potential*condition); // One condition factor for the whole functional cover.
            }
            int calendarDays=DateTime.IsLeapYear(driver.historicalYear)?366:365;
            foreach(var site in definition.sites)
            {
                var row=rows[site.id];var s=states[site.id];var asset=nextAssets.FirstOrDefault(a=>a.siteId==site.id);
                if(asset!=null)s.condition=asset.condition; // Existing stable funding deterioration, once.
                opening[site.id]=s.condition;meanLoss[site.id]=0;harvestCondition[site.id]=s.condition;s.recoveredLastYear=0;
                var exposure=rules.sites.Single(x=>x.siteId==site.id);
                var recipe=asset==null?null:Action(site.id,asset.actionId);
                var practice=recipe==null?null:rules.practices.FirstOrDefault(p=>p.actionId==recipe.actionId);
                bool active=asset!=null&&IsOperating(asset,Year)&&funded[site.id];
                double area=site.areaM2*exposure.baselineCroppedFraction*(recipe==null?1:1-recipe.services.occupiedAreaM2/site.areaM2);
                string cropId=exposure.baselineCropId;
                if(practice!=null)
                {
                    int phase=Math.Max(0,Year-asset.startedYear-recipe.constructionYears)%practice.cropSequence.Length;
                    s.rotationPhase=phase;cropId=practice.cropSequence[phase];area=active?site.areaM2*practice.croppedFraction:0;
                }
                s.cropId=area>0?cropId:null;
                if(area<=0)continue;
                var crop=rules.crops.Single(c=>c.id==cropId);var season=driver.crops.Single(c=>c.cropId==cropId);
                double maturity=practice==null?0:row.maturity;
                double dryLoss=Math.Max(0,season.longestDrySpellDays-rules.drySpellGraceDays)*crop.droughtLossPerDay;
                dryLoss*=1-(practice?.matureDroughtMitigation??0)*maturity;
                double loss=dryLoss+season.hotDays*crop.heatLossPerDay+Math.Max(0,season.precipTenthsMm/10.0-crop.referenceRainMm*1.5)*crop.excessRainLossPerMm;
                double weather=Math.Max(rules.minimumWeatherFactor,Math.Min(rules.maximumWeatherFactor,1-loss*rules.sensitivityFactor));
                double management=1-(practice?.establishmentCostFraction??0)*(1-maturity);
                cropResults.Add(new TownCropResult {siteId=site.id,cropId=crop.id,practiceId=practice?.actionId??"baseline",units=crop.units,ruleId=crop.ruleId,
                    harvestDay=season.harvestDay,productiveAreaM2=area,benchmarkPerHa=crop.outputPerHa,weatherFactor=weather,managementFactor=management,openingCondition=s.condition});
            }
            int previousDay=0;
            foreach(var e in driver.events)
            {
                // Explicit authored seasonal interpretation. Other events are still in the
                // saved driver, but are outside this bounded damage model, not zero real risk.
                if(e.month<5||e.month>10||e.tminTenthsC<0)continue;
                var reduction=new Dictionary<string,double>();
                foreach(var group in rules.sites.GroupBy(s=>s.connectionGroupId).OrderBy(g=>g.Key,StringComparer.Ordinal))
                {
                    var devices=group.Where(g=>initialCapacity[g.siteId]>0||states[g.siteId].storedWaterM3>0).OrderBy(g=>g.siteId,StringComparer.Ordinal).ToArray();
                    // Sparse accounting: absent devices and absent carried water mean
                    // exactly zero storage reduction. Keep all real device/release rows.
                    if(devices.Length==0){reduction[group.Key]=0;continue;}
                    double oldStored=devices.Sum(g=>states[g.siteId].storedWaterM3),recovered=0,released=0,capacity=0;
                    var capacities=new Dictionary<string,double>();
                    foreach(var device in devices)
                    {
                        var s=states[device.siteId];
                        double cap=opening[device.siteId]<=0?0:initialCapacity[device.siteId]*s.condition/opening[device.siteId];
                        double drain=Math.Min(s.storedWaterM3,cap*rules.dailyStorageRecoveryFraction*(e.dayOfYear-previousDay));
                        recovered+=drain;s.storedWaterM3-=drain;
                        double spill=Math.Max(0,s.storedWaterM3-cap);released+=spill;s.storedWaterM3-=spill;
                        capacities[device.siteId]=cap;capacity+=cap;
                    }
                    double inflow=group.Sum(g=>{
                        var site=sites[g.siteId];double cover=CoverFraction(g.siteId,states[g.siteId].condition);
                        return e.precipTenthsMm/10000.0*site.areaM2*(.15+.75*(1-cover));
                    });
                    double available=devices.Sum(g=>Math.Max(0,capacities[g.siteId]-states[g.siteId].storedWaterM3));
                    double captured=Math.Min(inflow,available),remaining=captured;
                    foreach(var device in devices)
                    {
                        var s=states[device.siteId];double amount=Math.Min(remaining,Math.Max(0,capacities[device.siteId]-s.storedWaterM3));
                        s.storedWaterM3+=amount;remaining-=amount;
                    }
                    reduction[group.Key]=inflow==0?0:captured/inflow;
                    double closing=devices.Sum(g=>states[g.siteId].storedWaterM3);
                    Require(Math.Abs(oldStored-recovered-released+captured-closing)<.00001,"Event storage conservation failed.");
                    storage.Add(new TownEventStorage {eventId=e.recordId,connectionGroupId=group.Key,ruleId=rules.storageRecoveryRuleId,deviceSiteIds=devices.Where(d=>capacities[d.siteId]>0).Select(d=>d.siteId).ToArray(),capacityM3=capacity,
                        openingStoredM3=oldStored,recoveredM3=recovered,capacityLossReleaseM3=released,inflowM3=inflow,capturedM3=captured,closingStoredM3=closing});
                }
                foreach(var exposure in rules.sites.OrderBy(s=>s.siteId,StringComparer.Ordinal))
                {
                    var s=states[exposure.siteId];var row=rows[s.siteId];var site=sites[s.siteId];
                    var asset=nextAssets.FirstOrDefault(a=>a.siteId==s.siteId);var recipe=asset==null?null:Action(s.siteId,asset.actionId);
                    var practice=recipe==null?null:rules.practices.FirstOrDefault(p=>p.actionId==recipe.actionId);
                    bool active=asset!=null&&IsOperating(asset,Year)&&funded[s.siteId];
                    double cover=CoverFraction(s.siteId,s.condition);
                    double local=Math.Max(0,e.precipTenthsMm/10.0-rules.rainDamageThresholdMm)*rules.localRainDamagePerMm*(1-.6*cover)
                        *(1+Math.Min(1,e.antecedent7DaysTenthsMm/10.0*rules.antecedentAmplificationPerMm))*(1-reduction[exposure.connectionGroupId]);
                    double upstream=Math.Max(0,e.precipTenthsMm/10.0-rules.upstreamRainThresholdMm)*rules.upstreamDamagePerMm;
                    double vulnerability=active&&practice!=null?1-(1-practice.matureRainVulnerability)*row.maturity:1;
                    double fraction=Math.Min(rules.maximumEventDamage,(local+upstream)*exposure.exposure*vulnerability*rules.sensitivityFactor);
                    double loss=s.condition*fraction;
                    if(loss<=0)continue;
                    s.condition=Math.Max(0,s.condition-loss);meanLoss[s.siteId]+=loss*(calendarDays-e.dayOfYear+1)/calendarDays;
                    var crop=cropResults.FirstOrDefault(c=>c.siteId==s.siteId);bool before=crop!=null&&e.dayOfYear<=crop.harvestDay;
                    if(before)harvestCondition[s.siteId]=s.condition;
                    // Missing upkeep removes protection/service, not the declared public
                    // responsibility for an already-built asset.
                    var duty=asset!=null&&IsOperating(asset,Year)?rules.publicResponsibilities.FirstOrDefault(r=>r.actionId==recipe.actionId):null;
                    impacts.Add(new TownEventImpact {siteId=s.siteId,eventId=e.recordId,ruleId=rules.eventRuleId,connectionGroupId=exposure.connectionGroupId,dayOfYear=e.dayOfYear,
                        beforeHarvest=before,publiclyResponsible=duty!=null,localHazard=local,upstreamHazard=upstream,connectedStorageReductionFraction=reduction[exposure.connectionGroupId],conditionLoss=loss});
                    if(duty!=null)
                    {
                        long due=checked((long)Math.Ceiling(loss*duty.repairCentsPerConditionUnit));
                        repairs.Add(new TownRepairLiability {id=pending.requestId+"/repair/"+s.siteId+"/"+e.dayOfYear,siteId=s.siteId,actionId=recipe.actionId,eventId=e.recordId,
                            responsibility=duty.responsibility,ruleId=duty.ruleId,createdYear=Year,originalCents=due,unpaidCents=due,originalConditionLoss=loss,unrecoveredCondition=loss});
                    }
                }
                previousDay=e.dayOfYear;
            }
            foreach(var site in definition.sites)
            {
                var s=states[site.id];var row=rows[site.id];var asset=nextAssets.FirstOrDefault(a=>a.siteId==site.id);
                double annualCondition=Math.Max(0,opening[site.id]-meanLoss[site.id]);
                double oldCondition=row.condition;row.condition=annualCondition;
                row.serviceUnits=oldCondition>0?row.serviceUnits*annualCondition/oldCondition:0;
                row.vegetatedAreaM2=site.areaM2*CoverFraction(site.id,annualCondition);
                row.retentionCapacityM3=opening[site.id]>0?initialCapacity[site.id]*s.condition/opening[site.id]:0;
                row.disrupted=impacts.Any(e=>e.siteId==site.id);
                row.explanation=row.disrupted?"Illustrative heavy-rain impacts under "+rules.eventRuleId+"; saved condition carries into the next year.":"Saved crop, lifecycle, condition and funding under "+rules.version+".";
                var crop=cropResults.FirstOrDefault(c=>c.siteId==site.id);double agricultural=0;
                if(crop!=null)
                {
                    crop.harvestCondition=harvestCondition[site.id];crop.closingConditionBeforeRecovery=s.condition;
                    agricultural=crop.outputUnits=crop.productiveAreaM2/10000*crop.benchmarkPerHa*crop.weatherFactor*crop.managementFactor*crop.harvestCondition;
                }
                double other=0;
                if(asset!=null)
                {
                    var recipe=Action(site.id,asset.actionId);
                    if(!rules.practices.Any(p=>p.actionId==recipe.actionId))
                        other=Math.Max(0,recipe.productionUnits-site.baselineProductionUnits*(1-recipe.services.occupiedAreaM2/site.areaM2))*ActiveShare(row);
                }
                row.productionUnits=agricultural+other;
                // Drain through the remaining calendar days; paid repair cannot refill or
                // retroactively enlarge storage used by an earlier event.
                s.storedWaterM3=row.retentionCapacityM3<=0?0:Math.Max(0,Math.Min(s.storedWaterM3,row.retentionCapacityM3)-row.retentionCapacityM3*rules.dailyStorageRecoveryFraction*(calendarDays-previousDay));
            }
            var report=new TownSimulationReport {version="annual-numeric-report-v1",transitionId=pending.requestId,driver=TownCopies.Copy(driver),closingState=state,
                crops=cropResults.ToArray(),impacts=impacts.ToArray(),storage=storage.ToArray(),payments=Array.Empty<TownPublicPayment>()};
            return new NumericDraft {report=report,outcomes=outcomes,repairs=repairs,newRepairDue=Sum(repairs.Where(r=>r.createdYear==Year).Select(r=>r.unpaidCents))};
        }

        TownYearOutcomes SettleNumeric(NumericDraft draft,TownAsset[] nextAssets,Dictionary<string,bool> funded,TownYearLedger ledger)
        {
            var payments=new List<TownPublicPayment>();var state=draft.report.closingState;
            void Payment(string id,string site,string action,string kind,long due,long paid,string liability=null,double recovery=0)
                =>payments.Add(new TownPublicPayment {id=id,siteId=site,actionId=action,kind=kind,dueCents=due,paidCents=paid,liabilityId=liability,nextYearRecovery=recovery});
            foreach(var q in pending.actions)
            {long cost=Action(q.siteId,q.actionId).upfrontPublicCostCents;Payment(pending.requestId+"/upfront/"+q.siteId,q.siteId,q.actionId,"upfront",cost,cost);}
            var oldRepairs=draft.repairs.Where(r=>r.createdYear<Year).OrderBy(r=>r.createdYear).ThenBy(r=>r.siteId,StringComparer.Ordinal).ThenBy(r=>r.eventId,StringComparer.Ordinal).ThenBy(r=>r.id,StringComparer.Ordinal).ToArray();
            long oldRepairDue=Sum(oldRepairs.Select(r=>r.unpaidCents));
            Require(oldRepairDue<=ledger.openingArrearsCents,"Repair debt exceeds authoritative arrears.");
            long otherArrears=ledger.openingArrearsCents-oldRepairDue,otherPaid=Math.Min(otherArrears,ledger.arrearsPaidCents);
            if(otherArrears>0)Payment(pending.requestId+"/prior-arrears",null,null,"prior-program-or-operation-arrears",otherArrears,otherPaid);
            void Repair(TownRepairLiability repair,ref long budget,string kind)
            {
                long due=repair.unpaidCents,paid=Math.Min(budget,due);budget-=paid;repair.unpaidCents-=paid;
                double recovery=Math.Min(repair.unrecoveredCondition,repair.originalConditionLoss*paid/repair.originalCents);
                repair.unrecoveredCondition=Math.Max(0,repair.unrecoveredCondition-recovery);
                var site=state.sites.Single(s=>s.siteId==repair.siteId);site.condition=Clamp01(site.condition+recovery);site.recoveredLastYear+=recovery;
                Payment(repair.id+"/paid-y"+Year,repair.siteId,repair.actionId,kind,due,paid,repair.id,recovery);
            }
            long priorBudget=ledger.arrearsPaidCents-otherPaid;
            foreach(var repair in oldRepairs)Repair(repair,ref priorBudget,"prior-repair-arrears");
            Require(priorBudget==0,"Prior arrears allocation failed.");
            long cash=ledger.openingCashCents-ledger.upfrontPaidCents-ledger.arrearsPaidCents;
            foreach(var asset in nextAssets.OrderBy(a=>a.startedYear).ThenBy(a=>a.siteId,StringComparer.Ordinal))
            {
                long program=ProgramDue(asset,Year),operating=OperatingDue(asset,Year);
                long paidProgram=Math.Min(cash,program);cash-=paidProgram;long paidOperating=Math.Min(cash,operating);cash-=paidOperating;
                Payment(pending.requestId+"/program/"+asset.siteId,asset.siteId,asset.actionId,"program",program,paidProgram);
                Payment(pending.requestId+"/operation/"+asset.siteId,asset.siteId,asset.actionId,"operation",operating,paidOperating);
            }
            long repairBudget=ledger.repairPaidCents;
            foreach(var repair in draft.repairs.Where(r=>r.createdYear==Year).OrderBy(r=>r.siteId,StringComparer.Ordinal).ThenBy(r=>r.eventId,StringComparer.Ordinal).ThenBy(r=>r.id,StringComparer.Ordinal))Repair(repair,ref repairBudget,"new-repair");
            Require(repairBudget==0,"New repair allocation failed.");
            state.repairs=draft.repairs.Where(r=>r.unpaidCents>0).OrderBy(r=>r.createdYear).ThenBy(r=>r.siteId,StringComparer.Ordinal).ThenBy(r=>r.eventId,StringComparer.Ordinal).ThenBy(r=>r.id,StringComparer.Ordinal).ToArray();
            foreach(var site in state.sites)
            {
                var asset=nextAssets.FirstOrDefault(a=>a.siteId==site.siteId);
                double linkedDamage=state.repairs.Where(r=>r.siteId==site.siteId).Sum(r=>r.unrecoveredCondition);
                // Natural/maintenance recovery never erases damage attached to unpaid bills.
                double recovery=asset==null||funded[site.siteId]?Math.Min(definition.simulation.privateAnnualRecovery,Math.Max(0,1-site.condition-linkedDamage)):0;
                site.condition=Clamp01(site.condition+recovery);site.recoveredLastYear+=recovery;site.outstandingDamage=1-site.condition;
                if(asset!=null)asset.condition=site.condition;
                var crop=draft.report.crops.FirstOrDefault(c=>c.siteId==site.siteId);if(crop!=null)crop.nextYearCondition=site.condition;
            }
            draft.report.payments=payments.ToArray();
            Require(Sum(payments.Select(p=>p.paidCents))==checked(ledger.upfrontPaidCents+ledger.arrearsPaidCents+ledger.programPaidCents+ledger.operatingPaidCents+ledger.repairPaidCents),"Itemized public payments do not reconcile.");
            Require(Sum(state.repairs.Select(r=>r.unpaidCents))<=ledger.closingArrearsCents,"Closing repair debt exceeds authoritative arrears.");
            var o=draft.outcomes;
            o.productionUnits=o.sites.Sum(r=>r.productionUnits);o.serviceUnits=o.sites.Sum(r=>r.serviceUnits);o.vegetatedAreaM2=o.sites.Sum(r=>r.vegetatedAreaM2);
            o.retentionCapacityM3=o.sites.Sum(r=>r.retentionCapacityM3);o.disruptedSites=o.sites.Count(r=>r.disrupted);
            AnnualEconomy.Evaluate(definition,o,ledger,pending.driver);
            var e=o.economy;var targets=definition.economy;
            draft.report.energyDeficit=Math.Max(0,e.energyDemand-e.energySupplied);draft.report.waterDeficit=Math.Max(0,e.waterDemand-e.waterSupplied);
            draft.report.wastewaterDeficit=Math.Max(0,e.wastewaterLoad-e.wastewaterTreated);draft.report.housingDeficit=Math.Max(0,targets.householdDemand-e.occupiedHomes);draft.report.jobsDeficit=Math.Max(0,targets.jobsTarget-e.jobs);
            return o;
        }
    }
}
