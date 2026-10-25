using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;
using Terra.Simulation;

namespace Terra.Core
{
    [Serializable] public sealed class RunoffParameter { public string coverId;public float curveNumber; }
    [Serializable] public sealed class RunoffContext { public string id,coverId;public float areaM2; }
    [Serializable] public sealed class RunoffConnection { public string siteId,groupId; }
    [Serializable] public sealed class RunoffConfiguration
    {
        public int schemaVersion; public string modelVersion,worldSha256,partitionSha256,sourceId,sourceUrl,assumptionSourceId,connectionNote;
        public float analysisAreaM2; public RunoffParameter[] parameters;public RunoffContext[] context;public RunoffConnection[] connections;
    }
    // Pure integration adapter. Each site has the frozen v1 area-weighted CN; volumes are summed across sites/context.
    public sealed class LandscapeRunoff
    {
        public const string ModelVersion="terra-landscape-runoff-v1";
        readonly WorldData world;readonly Scenario scenario;readonly Storm storm;
        readonly Dictionary<string,float> parameters;
        readonly Dictionary<string,string> groups;
        readonly SimulationSiteInput[] context;
        readonly SimulationOutput baseline;
        readonly float area;
        public float RainfallMm=>storm.rainfallMm;
        public float BaselineDischargeM3=>baseline.dischargedM3;
        public string[] ConnectedSites(string siteId)=>groups.TryGetValue(siteId??"",out var group)?groups.Where(p=>p.Value==group).Select(p=>p.Key).OrderBy(x=>x,StringComparer.Ordinal).ToArray():Array.Empty<string>();
        public LandscapeRunoff(WorldData world,Scenario scenario,RunoffConfiguration config)
        {
            Need(config!=null&&config.schemaVersion==1&&config.modelVersion==ModelVersion,"Unsupported landscape runoff configuration.");
            Need(config.parameters!=null&&config.context!=null&&config.connections!=null,"Missing model parameters, context or connections.");
            Need(config.sourceId==RunoffEvaluator.FormulaSourceId&&config.assumptionSourceId=="scenario-assumptions-v1","Unrecognized model provenance.");
            this.world=world;this.scenario=scenario;area=config.analysisAreaM2;
            var source=scenario.storm;
            Need(source.soilGroup=="B"&&source.initialAbstractionRatio==.2f,"This configuration requires assumed soil B and abstraction 0.2.");
            storm=new Storm{id=source.id,rainfallMm=source.rainfallMm,soilGroup=source.soilGroup,antecedentCondition=source.antecedentCondition,sourceId=source.sourceId,initialAbstractionRatio=source.initialAbstractionRatio};
            parameters=new Dictionary<string,float>(StringComparer.Ordinal);
            foreach(var p in config.parameters){Need(p!=null&&!string.IsNullOrWhiteSpace(p.coverId)&&!parameters.ContainsKey(p.coverId),"Duplicate/invalid cover mapping.");Need(Finite(p.curveNumber)&&p.curveNumber>0&&p.curveNumber<=100,"Invalid curve number.");parameters.Add(p.coverId,p.curveNumber);}
            groups=new Dictionary<string,string>(StringComparer.Ordinal);
            foreach(var c in config.connections){Need(c!=null&&world.sites.Any(s=>s.id==c.siteId)&&!groups.ContainsKey(c.siteId)&&!string.IsNullOrWhiteSpace(c.groupId)&&!c.groupId.StartsWith("context"),"Invalid/duplicate site connection.");groups.Add(c.siteId,c.groupId);}
            Need(groups.Count==world.sites.Length,"Every site must have exactly one explicit connection group.");
            var ids=new HashSet<string>(world.sites.Select(s=>s.id),StringComparer.Ordinal);
            context=config.context.Select(c=>{Need(c!=null&&!string.IsNullOrWhiteSpace(c.id)&&ids.Add(c.id)&&Finite(c.areaM2)&&c.areaM2>0,"Invalid or duplicate context area.");Need(parameters.ContainsKey(c.coverId),"Missing context cover mapping.");return new SimulationSiteInput{siteId=c.id,contributingAreaId="context-isolated",areaM2=c.areaM2,curveNumber=parameters[c.coverId]};}).ToArray();
            Need(Finite(area)&&area>0&&Math.Abs(context.Sum(c=>(double)c.areaM2)+world.sites.Sum(s=>(double)s.analysisAreaM2)-area)<.2,"Context and sites must partition the analysis domain.");
            foreach(var t in scenario.treatments)CurveNumberMixture.AreaWeighted(t.cover,parameters);
            baseline=Evaluate(Array.Empty<Intervention>(),0);
        }
        SimulationOutput Evaluate(Intervention[] proposals,int revision)
        {
            var edits=proposals.ToDictionary(p=>p.siteId,p=>p.treatmentId,StringComparer.Ordinal);
            var input=new List<SimulationSiteInput>(context);
            foreach(var site in world.sites)
            {
                var cover=site.baselineCover;float capacity=0;
                if(edits.TryGetValue(site.id,out var id))
                {cover=scenario.treatments.First(t=>t.id==id).cover;capacity=site.offers.First(o=>o.treatmentId==id).retentionCapacityM3;}
                input.Add(new SimulationSiteInput{siteId=site.id,areaM2=site.analysisAreaM2,contributingAreaId=groups[site.id],curveNumber=CurveNumberMixture.AreaWeighted(cover,parameters),retentionCapacityM3=capacity});
            }
            return RunoffEvaluator.Evaluate(new SimulationInput{schemaVersion=1,revision=revision,storm=storm,sites=input.ToArray()});
        }
        public SimulationInput AnnualBaseline() => new SimulationInput { schemaVersion=1, revision=0, storm=TownCopies.Copy(storm),
            sites=context.Concat(world.sites.Select(s=>new SimulationSiteInput { siteId=s.id, contributingAreaId=groups[s.id], areaM2=s.analysisAreaM2, curveNumber=CurveNumberMixture.AreaWeighted(s.baselineCover,parameters) })).ToArray() };
        public double CurveNumber(CoverFraction[] cover) => CurveNumberMixture.AreaWeighted(cover,parameters);
        // Fixed-storm comparison of accepted annual cover/storage, not annual rain or a flood forecast.
        // Baseline cover remains outside the real building/park footprint. Establishment is gradual.
        public SimulationOutput EvaluateAnnual(TownYearOutcomes outcomes, int revision)
        {
            var input = new List<SimulationSiteInput>(context);
            foreach (var site in world.sites)
            {
                var row = outcomes.sites.First(r => r.siteId == site.id);
                double cn = CurveNumberMixture.AreaWeighted(site.baselineCover,parameters);
                if (row.actionId != null)
                {
                    double target;
                    if (row.actionId == "low-density-housing" || row.actionId == "public-park")
                    {
                        bool home = row.actionId == "low-density-housing";
                        double share = Math.Min(1,(home ? 832.0 : 1920.0)/site.analysisAreaM2);
                        double green = home ? .35 : .85;
                        target = cn*(1-share) + share*(green*parameters["grass"]+(1-green)*parameters["sealed"]);
                    }
                    else if (row.actionId == "conservation-rotation") target = .3*parameters["grass"]+.7*parameters["cultivated"];
                    else target = CurveNumberMixture.AreaWeighted(scenario.treatments.First(t=>t.id==row.actionId).cover,parameters);
                    cn += (target-cn)*row.maturity*row.condition;
                }
                input.Add(new SimulationSiteInput { siteId=site.id,areaM2=site.analysisAreaM2,contributingAreaId=groups[site.id],curveNumber=(float)cn,retentionCapacityM3=(float)row.retentionCapacityM3 });
            }
            return RunoffEvaluator.Evaluate(new SimulationInput { schemaVersion=1,revision=revision,storm=storm,sites=input.ToArray() });
        }
        public void Apply(Intervention[] proposals,WorldResult accounting)
        {
            var result=Evaluate(proposals,accounting.revision);var byId=result.sites.ToDictionary(s=>s.siteId,StringComparer.Ordinal);
            foreach(var site in accounting.sites){var water=byId[site.siteId];site.generatedM3=water.generatedM3;site.retainedM3=water.retainedM3;site.dischargedM3=water.dischargedM3;}
            accounting.runoffAvailable=true;accounting.generatedM3=result.generatedM3;accounting.retainedM3=result.retainedM3;accounting.dischargedM3=result.dischargedM3;
            float contextGenerated=result.sites.Where(s=>!groups.ContainsKey(s.siteId)).Sum(s=>s.generatedM3);
            var facts=new List<ExplanationFact>(accounting.explanations);facts.AddRange(result.explanations.Where(f=>string.IsNullOrEmpty(f.siteId)||groups.ContainsKey(f.siteId)));
            void Fact(string code,float value,string unit)=>facts.Add(new ExplanationFact{code=code,value=value,unit=unit,sourceId="scenario-assumptions-v1",siteId="",treatmentId=""});
            Fact("baseline-discharge",baseline.dischargedM3,"m3");Fact("context-generated",contextGenerated,"m3");Fact("managed-generated",accounting.sites.Sum(s=>s.generatedM3),"m3");
            Fact("analysis-area",area,"m2");Fact("managed-area",world.sites.Sum(s=>s.analysisAreaM2),"m2");
            accounting.explanations=facts.ToArray();
        }
        static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
        static void Need(bool valid,string message){if(!valid)throw new ArgumentException(message);}
    }
}
