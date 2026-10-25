using System.Text.Json;
using System.Text.RegularExpressions;
using Terra.Core;
using Terra.Contracts;

var options=new JsonSerializerOptions{IncludeFields=true,WriteIndented=true};
int checks=0;
void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
T Read<T>(string path)=>JsonSerializer.Deserialize<T>(File.ReadAllText(path),options);
var yaml=File.ReadAllText("Assets/Terra/Art/ExternalKit/BuildActions/BuildActionCatalog.asset");
BuildActionRule[] Actions()=>Regex.Split(yaml,"  - actionId: ").Skip(1).Select(part=>{
 string art=part.Split('\n')[0].Trim();string id=art switch {"cultivation"=>"cultivate-standard","living-margin"=>"cultivate-margin","ground-cover-restoration"=>"restore-grass","vegetated-swale"=>"water-swale",_=>art};
 var dim=Regex.Match(part,@"siteSizeMetres: \{x: ([\d.]+), y: ([\d.]+)\}");
 string category=new[]{"cultivate-standard","cultivate-margin","cover-crop","conservation-rotation"}.Contains(id)?"Farming":id.Contains("housing")?"Residential":id=="public-park"||id=="village-center"?"Community":id=="restore-grass"||id=="stewardship-agreement"?"Nature":"Utilities";
 return new BuildActionRule{id=id,artId=art,title=Regex.Match(part,"title: (.+)").Groups[1].Value.Trim(),category=category,width=float.Parse(dim.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture),depth=float.Parse(dim.Groups[2].Value,System.Globalization.CultureInfo.InvariantCulture),landscape=part.Contains("landscapeSample: 1"),cost=30};
}).ToArray();
var summaries=new List<object>();
foreach(var location in LocationProfile.All)
{
 string pack=location.PackId, root="worlds/"+pack+"/";
 var world=Read<WorldData>(root+"world.json");var scenario=Read<Scenario>(root+"scenario.json");var hydro=Read<RunoffConfiguration>(root+"hydrology.json");
 var overlay=Read<UniformParcelPack>("Assets/Terra/Resources/Parcels/"+pack+".json");
 var originalTerrain=world.terrain;var originalCovers=world.covers;var originalFeatures=world.features;
 UniformParcels.Apply(world,hydro,overlay,pack);
 Check(ReferenceEquals(world.terrain,originalTerrain)&&ReferenceEquals(world.covers,originalCovers)&&ReferenceEquals(world.features,originalFeatures),"Source geography retained");
 bool twice=false;try{UniformParcels.Apply(world,hydro,overlay,pack);}catch(ArgumentException){twice=true;}Check(twice,"Double overlay rejected");
 Check(world.sites.Count(s=>!s.protectedSite)>20,"More than twenty open slots");
 foreach(var site in world.sites.Where(s=>s.sourceId==UniformParcels.SourceId))
 {
   Check(site.polygon.Length==4,"Rectangular grid slots");
   Check(Math.Abs(site.analysisAreaM2-Math.Abs((site.polygon[1].x-site.polygon[0].x)*(site.polygon[2].z-site.polygon[1].z)))<.1,"Exact occupied area");
 }
 var rules=new BuildActionRules(world,Actions());rules.ExpandOffers(world,scenario);
 foreach(var open in world.sites.Where(s=>!s.protectedSite))
   Check(rules.Rejection(open,"low-density-housing")==null,"Each open plot fits at least a standard home: "+pack+" "+open.id);
 Check(location.ParcelAnnualSaveKey!=location.AnnualSaveKey&&location.ParcelChapterSaveKey!=location.ChapterSaveKey,"New layout uses distinct save keys");
 foreach(var initial in overlay.initialBuildings)
 {
   var site=world.sites.Single(s=>s.id==initial.siteId);
   Check(rules.Layout(site,rules.Find(initial.actionId)).Valid,"Initial model fits real footprint: "+pack+" "+initial.title);
   Check(rules.Rejection(site,initial.actionId)!=null,"Initial slot rejects construction");
 }
 var runoff=new LandscapeRunoff(world,scenario,hydro);
 var definition=FullAnnualGameplay.Create(world,scenario,pack,rules,runoff);
 Check(definition.version=="4.0.0","Versioned new layout");
 Check(definition.economy.baselineHomes==4&&definition.economy.baselineJobs==4,"Inherited community counted once");
 Check(overlay.initialBuildings.All(b=>definition.actions.All(a=>a.siteId!=b.siteId)),"Inherited lots never become purchasable projects");
 foreach(string id in FullAnnualGameplay.Supported)Check(definition.actions.Any(a=>a.actionId==id),"All action variants remain available: "+id);
 var town=new AnnualTown(definition,"parcels-"+pack);
 Check(town.Assets.Length==0&&town.CashCents==1000000000,"Starting community is baseline; no new charges or assets");
 var farm=definition.actions.First(a=>a.actionId=="cultivate-standard");town.QueueAction(farm.siteId,farm.actionId,0);
 var home=definition.actions.First(a=>a.actionId=="low-density-housing"&&a.siteId!=farm.siteId);town.QueueAction(home.siteId,home.actionId,town.Revision);
 Check(town.Queue.Length==2,"Independent parcels can be queued together");
 var provider=new AuthoredAnnualConsequenceProvider();AnnualTownDemo.Finish(town,provider);
 Check(town.History.Length==10&&town.Assets.Length==2,"Ten years commit only player's projects");
 foreach(var year in town.History)
 {
   var l=year.ledger;
   Check(l.openingCashCents-l.upfrontPaidCents-l.arrearsPaidCents-l.programPaidCents-l.operatingPaidCents-l.repairPaidCents+l.appropriationCents+l.publicReceiptsCents==l.closingCashCents,"Ledger conservation");
   var water=AnnualEconomy.Water(definition,year.outcomes);
   Check(Math.Abs(water.generatedM3-water.retainedM3-water.dischargedM3)<.15,"Reference-storm water conservation");
 }
 var restored=AnnualTown.Restore(definition,ReadSnapshot(town));
 Check(restored.CashCents==town.CashCents&&restored.Phase==TownPhase.Completed,"New-layout save round trip");
 var pendingTown=AnnualTown.Restore(definition,ReadSnapshot(town));
 int completedInputBytes=JsonSerializer.SerializeToUtf8Bytes(new AnnualChapterRequest{schemaVersion=1,definition=definition,snapshot=pendingTown.Snapshot()},options).Length;
 Check(completedInputBytes<16*1024*1024,"Complete dense-layout narration request stays within the bounded local service limit");
 summaries.Add(new{pack,openPlots=world.sites.Count(s=>!s.protectedSite),initialBuildings=overlay.initialBuildings.Length,actions=definition.actions.Length,definitionBytes=JsonSerializer.SerializeToUtf8Bytes(definition,options).Length,completedInputBytes,closingCash=town.CashCents});
 Console.WriteLine(pack+": "+world.sites.Count(s=>!s.protectedSite)+" open plots; 17 variants; ten-year/save checks pass");
}
AnnualTownSnapshot ReadSnapshot(AnnualTown town)=>JsonSerializer.Deserialize<AnnualTownSnapshot>(JsonSerializer.Serialize(town.Snapshot(),options),options);
Directory.CreateDirectory("docs/evidence/map-parcels");
File.WriteAllText("docs/evidence/map-parcels/model-checks.json",JsonSerializer.Serialize(new{checks,locations=summaries},options));
Console.WriteLine("Parcel model checks passed: "+checks);
