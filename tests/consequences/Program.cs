using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using Terra.Contracts;
using Terra.Core;

static class Program
{
    static readonly JsonSerializerOptions options=new JsonSerializerOptions{IncludeFields=true,WriteIndented=true};
    static readonly List<string> checks=new List<string>();
    static WorldData world;static Scenario scenario;static RunoffConfiguration config;
    static T Read<T>(string path)=>JsonSerializer.Deserialize<T>(File.ReadAllText(path),options);
    static T Copy<T>(T value)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value,options),options);
    static string Value(WorldResult result){result.revision=0;return JsonSerializer.Serialize(result,options);}
    static void Check(bool yes,string label){if(!yes)throw new Exception(label);checks.Add(label);}
    static void Near(double actual,double expected,string label,double tolerance=.003)=>Check(Math.Abs(actual-expected)<=tolerance,label+" ("+actual+" vs "+expected+")");
    static void Reject(Action action,string label){bool rejected=false;try{action();}catch(ArgumentException){rejected=true;}Check(rejected,label);}
    static WorldStateStore Store(WorldData w=null,Scenario s=null,RunoffConfiguration c=null)=>new WorldStateStore(w??world,s??scenario,new LandscapeRunoff(w??world,s??scenario,c??config));
    static Evaluation Edit(WorldStateStore store,string operation,string site=null,string treatment=null,string dest=null)
    {
        string before=Value(store.Result);int revision=store.Revision;var command=new EditCommand{operation=operation,siteId=site,treatmentId=treatment,destinationSiteId=dest,expectedRevision=revision};
        var preview=store.Preview(command);Check(preview.valid,"Valid "+operation+" / "+site+" / "+treatment);
        Check(Value(store.Result)==before&&store.Revision==revision,"Hydrology preview is pure");
        var commit=store.Commit(command);Check(commit.valid&&Value(commit.result)==Value(preview.result),"Complete hydrology and accounting preview matches commit");Balance(commit.result);return commit;
    }
    static float Fact(WorldResult r,string code)=>r.explanations.Single(f=>f.code==code&&string.IsNullOrEmpty(f.siteId)).value;
    static void Balance(WorldResult r)
    {
        Near(r.generatedM3,r.retainedM3+r.dischargedM3,"Landscape volume conserved",.005);
        Check(r.generatedM3>=0&&r.retainedM3>=0&&r.dischargedM3>=0,"Nonnegative landscape quantities");
        foreach(var s in r.sites)Near(s.generatedM3,s.retainedM3+s.dischargedM3,"Site conserved: "+s.siteId);
        Near(r.generatedM3,Fact(r,"context-generated")+Fact(r,"managed-generated"),"Context + managed volumes = full domain",.005);
    }
    static int Main(string[] args)
    {
        string root=args.Length>0?Path.GetFullPath(args[0]):Directory.GetCurrentDirectory();Directory.SetCurrentDirectory(root);
        world=Read<WorldData>("worlds/meadowbrook-v1/world.json");scenario=Read<Scenario>("worlds/meadowbrook-v1/scenario.json");config=Read<RunoffConfiguration>("worlds/meadowbrook-v1/hydrology.json");
        string worldBefore=JsonSerializer.Serialize(world,options),scenarioBefore=JsonSerializer.Serialize(scenario,options),configBefore=JsonSerializer.Serialize(config,options);
        var store=Store();var baseline=store.Result;Balance(baseline);
        Check(baseline.runoffAvailable&&baseline.sites.Length==world.sites.Length,"Hydrology enabled without dropping production or adding fake managed sites");
        Near(baseline.production,6,"Baseline production retained");Near(baseline.remainingCredits,60,"Baseline budget retained");
        Near(Fact(baseline,"analysis-area"),1486800,"Entire partitioned context analyzed");
        Near(baseline.sites.Single(s=>s.siteId=="south-01").generatedM3,11.85764145695332*6.75,"Independent CN78 50mm West field value");
        Near(Fact(baseline,"baseline-discharge"),baseline.dischargedM3,"Same-storm baseline captured");
        Edit(store,"add","south-01","cultivate-margin");
        Near(store.Result.sites.Single(s=>s.siteId=="south-01").generatedM3,8.328602575579806*6.75,"Frozen mixed-CN margin convention preserved");
        Near(store.Result.production,14,"Hydrology join preserves final production");
        Edit(store,"replace","south-01","restore-grass");Near(store.Result.sites.Single(s=>s.siteId=="south-01").generatedM3,1.706343414451108*6.75,"Independent grass runoff value");
        Near(store.Result.production,5,"Restoration loses displaced baseline productive use");
        Edit(store,"remove","south-01");Check(Value(store.Result)==Value(baseline),"Removal restores exact full water and accounting baseline");
        Edit(store,"add","south-08","cultivate-standard");var cultivated=store.Result;
        Check(cultivated.generatedM3>baseline.generatedM3&&cultivated.production>baseline.production,"Southern field cultivation trades vegetation/runoff for productive capacity");
        Edit(store,"add","south-07","water-swale");var swale=store.Result;
        Near(swale.retainedM3,8,"One device retains exactly its bounded 8 m³");
        Check(swale.sites.Single(s=>s.siteId=="south-08").dischargedM3<cultivated.sites.Single(s=>s.siteId=="south-08").dischargedM3,"Southern swale helps connected Southern field");
        foreach(var id in new[]{"south-01","south-02","south-03","east-09","reserve-11","reserve-12","west-10"})
            Near(swale.sites.Single(s=>s.siteId==id).dischargedM3,cultivated.sites.Single(s=>s.siteId==id).dischargedM3,"Disconnected site unchanged: "+id);
        Near(Fact(swale,"context-generated"),Fact(baseline,"context-generated"),"Background cannot gain storage benefit");
        Check(swale.sites.Single(s=>s.siteId=="south-07").generatedM3<cultivated.sites.Single(s=>s.siteId=="south-07").generatedM3,"Swale grass-cover effect occurs before additional storage");
        var deviceFacts=swale.explanations.Where(f=>f.siteId=="south-07").ToArray();Near(deviceFacts.Single(f=>f.code=="retention-capacity-used").value,8,"Used-capacity explanation matches conservation");
        string beforeMove=Value(swale);Edit(store,"move","south-07",null,"east-09");var moved=store.Result;
        Near(moved.sites.Single(s=>s.siteId=="south-08").dischargedM3,cultivated.sites.Single(s=>s.siteId=="south-08").dischargedM3,"Moving device away removes the Southern field benefit");
        Check(store.Undo()&&Value(store.Result)==beforeMove,"Atomic move undo restores connected hydrology");Check(store.Redo()&&Value(store.Result)==Value(moved),"Redo restores new connection effects");
        Edit(store,"reset");Check(Value(store.Result)==Value(baseline),"Reset restores all model outputs");
        // Two finite devices in one hypothetical technical group; retain volume only once.
        var joined=Copy(config);foreach(var c in joined.connections)if(c.groupId=="eastern-plots")c.groupId="southern-plots";
        var two=Store(c:joined);Edit(two,"add","south-07","water-swale");Edit(two,"add","east-09","water-swale");Near(two.Result.retainedM3,16,"Two devices share available volume and retain 16 total");
        var huge=Copy(world);foreach(var s in huge.sites)foreach(var o in s.offers)if(o.treatmentId=="water-swale")o.retentionCapacityM3=100000;
        var capped=Store(w:huge,c:joined);Edit(capped,"add","south-07","water-swale");Edit(capped,"add","east-09","water-swale");
        var grouped=joined.connections.Where(c=>c.groupId=="southern-plots").Select(c=>c.siteId).ToArray();
        Near(capped.Result.retainedM3,capped.Result.sites.Where(s=>grouped.Contains(s.siteId)).Sum(s=>s.generatedM3),"Oversized capacity capped to the connected group, not entire landscape");
        Check(capped.Result.explanations.Where(f=>f.code=="retention-capacity-unused").Sum(f=>f.value)>199000,"Unused capacity reported instead of retaining unrelated runoff");
        var zero=Copy(scenario);zero.storm.rainfallMm=0;var dry=Store(s:zero);Edit(dry,"add","south-07","water-swale");Near(dry.Result.dischargedM3,0,"Zero baseline and proposal stay finite zero");Near(dry.Result.retainedM3,0,"No water retained when rain is zero");
        // The storm is frozen on model construction, even if external setup code changes the source DTO.
        var changeable=Copy(scenario);var fixedStorm=Store(s:changeable);changeable.storm.rainfallMm=500;
        Check(Value(fixedStorm.Result)==Value(baseline),"Run's baseline/proposal storm is frozen");
        var reordered=Copy(config);Array.Reverse(reordered.context);Array.Reverse(reordered.parameters);Array.Reverse(reordered.connections);
        Check(Value(Store(c:reordered).Result)==Value(baseline),"Parameter/context/connection ordering does not change results");
        for(int i=0;i<20;i++){Edit(store,"add","south-08","cultivate-margin");Edit(store,"add","south-07","water-swale");Edit(store,"reset");Check(Value(store.Result)==Value(baseline),"Repeated edit/reset exact baseline "+i);}
        var bad=Copy(config);bad.context[0].areaM2+=100;Reject(()=>Store(c:bad),"Missing/duplicated partition area fails");
        bad=Copy(config);bad.context[0].coverId="unknown";Reject(()=>Store(c:bad),"Unknown cover fails explicitly");
        bad=Copy(config);bad.connections[0].groupId="context-isolated";Reject(()=>Store(c:bad),"Cannot attach a device group to background context");
        bad=Copy(config);bad.connections[0].siteId=bad.connections[1].siteId;Reject(()=>Store(c:bad),"Duplicate connection membership fails");
        bad=Copy(config);bad.modelVersion="unknown";Reject(()=>Store(c:bad),"Unknown model version fails");
        Check(worldBefore==JsonSerializer.Serialize(world,options)&&scenarioBefore==JsonSerializer.Serialize(scenario,options)&&configBefore==JsonSerializer.Serialize(config,options),"All canonical input DTOs remain unchanged");
        Directory.CreateDirectory("docs/evidence/phase4");File.WriteAllText("docs/evidence/phase4/integration-checks.json",JsonSerializer.Serialize(new {passed=true,count=checks.Count,baseline,cultivated,swale,moved,checks},options)+"\n");
        Console.WriteLine("Phase 4: "+checks.Count+" integration checks passed; baseline discharge "+baseline.dischargedM3+" m³.");return 0;
    }
}
