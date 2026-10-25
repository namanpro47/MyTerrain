var catalog=AssetDatabase.LoadAssetAtPath<Terra.Art.BuildActionCatalog>("Assets/Terra/Art/ExternalKit/BuildActions/BuildActionCatalog.asset");
System.IO.Directory.CreateDirectory("Logs/AnnualTown");
foreach(var place in Terra.Core.LocationProfile.All)
{
 var root="worlds/"+place.PackId+"/";
 var w=JsonUtility.FromJson<Terra.Contracts.WorldData>(System.IO.File.ReadAllText(root+"world.json"));
 var s=JsonUtility.FromJson<Terra.Contracts.Scenario>(System.IO.File.ReadAllText(root+"scenario.json"));
 var h=JsonUtility.FromJson<Terra.Core.RunoffConfiguration>(System.IO.File.ReadAllText(root+"hydrology.json"));
 if(place.IsWisconsin)Terra.Core.AnnualSpatialOpportunities.AddWind(w,h);
 var rules=Terra.Gameplay.BuildActionRuntime.Expand(catalog,ref w,ref s);
 var d=Terra.Core.FullAnnualGameplay.Create(w,s,place.PackId,rules,new Terra.Core.LandscapeRunoff(w,s,h));
 var town=new Terra.Core.AnnualTown(d,"atlas-chapter-replay-"+place.PackId);
 var a=System.Linq.Enumerable.First(d.actions,x=>x.actionId=="cultivate-standard");
 town.QueueAction(a.siteId,a.actionId,town.Revision);
 Terra.Core.AnnualTownDemo.Advance(town,town.RunId+"/year1",new Terra.Core.AuthoredAnnualConsequenceProvider());
 town.ContinuePlanning(town.Revision);
 town.BeginResolution(town.RunId+"/year2",town.Revision);
 System.IO.File.WriteAllText("Logs/AnnualTown/atlas-request-"+place.PackId+".json",JsonUtility.ToJson(new Terra.Contracts.AnnualChapterRequest {definition=d,snapshot=town.Snapshot()},true));
 while(town.Year<10){
  Terra.Core.AnnualTownDemo.Advance(town,town.PendingRequest.requestId,new Terra.Core.AuthoredAnnualConsequenceProvider());
  town.ContinuePlanning(town.Revision);
  town.BeginResolution(town.RunId+"/year"+town.Year,town.Revision);
 }
 System.IO.File.WriteAllText("Logs/AnnualTown/atlas-request-year10-"+place.PackId+".json",JsonUtility.ToJson(new Terra.Contracts.AnnualChapterRequest {definition=d,snapshot=town.Snapshot()}));
}
return "Exported Year 2 and Year 10 requests for each of the three atlas regions.";
