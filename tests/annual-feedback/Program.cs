using System.Text.Json;
using System.Text.RegularExpressions;
using Terra.Core;
using Terra.Contracts;

var options=new JsonSerializerOptions{IncludeFields=true,WriteIndented=true};
int checks=AnnualTownChecks.Run();
void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
T Read<T>(string path)=>JsonSerializer.Deserialize<T>(File.ReadAllText(path),options);
var yaml=File.ReadAllText("Assets/Terra/Art/ExternalKit/BuildActions/BuildActionCatalog.asset");
BuildActionRule[] Actions()=>Regex.Split(yaml,"  - actionId: ").Skip(1).Select(part=>{
 string art=part.Split('\n')[0].Trim();string id=art switch {"cultivation"=>"cultivate-standard","living-margin"=>"cultivate-margin","ground-cover-restoration"=>"restore-grass","vegetated-swale"=>"water-swale",_=>art};
 var dim=Regex.Match(part,@"siteSizeMetres: \{x: ([\d.]+), y: ([\d.]+)\}");
 string category=new[]{"cultivate-standard","cultivate-margin","cover-crop","conservation-rotation"}.Contains(id)?"Farming":id.Contains("housing")?"Residential":id=="public-park"||id=="village-center"?"Community":id=="restore-grass"||id=="stewardship-agreement"?"Nature":"Utilities";
 return new BuildActionRule{id=id,artId=art,title=Regex.Match(part,"title: (.+)").Groups[1].Value.Trim(),category=category,width=float.Parse(dim.Groups[1].Value,System.Globalization.CultureInfo.InvariantCulture),depth=float.Parse(dim.Groups[2].Value,System.Globalization.CultureInfo.InvariantCulture),landscape=part.Contains("landscapeSample: 1"),cost=30};
}).ToArray();

void Reject(Action action,string message) { bool rejected=false;try{action();}catch(ArgumentException){rejected=true;}catch(InvalidOperationException){rejected=true;}Check(rejected,message); }
T Clone<T>(T value)=>JsonSerializer.Deserialize<T>(JsonSerializer.Serialize(value,options),options);
var provider=new AuthoredAnnualConsequenceProvider();
TownYearReport Advance(AnnualTown town)=>AnnualTownDemo.Advance(town,town.RunId+"/"+town.Year,provider);
foreach(var location in LocationProfile.All)
{
    string root="worlds/"+location.PackId+"/";
    var world=Read<WorldData>(root+"world.json");var scenario=Read<Scenario>(root+"scenario.json");var hydro=Read<RunoffConfiguration>(root+"hydrology.json");
    UniformParcels.Apply(world,hydro,Read<UniformParcelPack>("Assets/Terra/Resources/Parcels/"+location.PackId+".json"),location.PackId);
    var rules=new BuildActionRules(world,Actions());rules.ExpandOffers(world,scenario);
    var d=FullAnnualGameplay.Create(world,scenario,location.PackId,rules,new LandscapeRunoff(world,scenario,hydro));
    var apartment=d.actions.First(a=>a.actionId=="high-density-housing");
    var budgetRules=Clone(d);budgetRules.startingCashCents=550000000;
    var budget=new AnnualTown(budgetRules,"apartment-"+location.PackId);
    Check(budget.Preview(apartment.siteId,apartment.actionId)==50000000,"$5M apartment fits $5.5M cash");
    budget.QueueAction(apartment.siteId,apartment.actionId,budget.Revision);
    Check(budget.AvailableCents==50000000&&budget.CashCents==550000000,"Displayed available equals actual current-year affordability");
    var forecast=AnnualGameplay.Forecast(budget);var accepted=Advance(budget);
    Check(accepted.ledger.upfrontPaidCents==500000000,"Apartment charges exactly the listed upfront cost");
    Check(forecast.ledger.closingCashCents==accepted.ledger.closingCashCents,"Forecast cash reconciles");
    Check(budget.CommittedCents>0,"Future upkeep is still disclosed");
    var poor=Clone(d);poor.startingCashCents=499999999;
    var under=new AnnualTown(poor,"one-cent-short");Reject(()=>under.QueueAction(apartment.siteId,apartment.actionId,0),"Reject actual one-cent shortfall");
    Check(under.Revision==0&&under.Queue.Length==0,"Unaffordable queue is atomic");

    var town=new AnnualTown(d,"conversion-"+location.PackId);
    var farm=d.actions.First(a=>a.actionId=="cultivate-standard"&&d.actions.Any(b=>b.siteId==a.siteId&&b.actionId=="restore-grass"));
    town.QueueAction(farm.siteId,farm.actionId,0);Advance(town);town.ContinuePlanning(town.Revision);
    string firstYear=JsonSerializer.Serialize(town.History[0],options);long cash=town.CashCents;
    var meadow=d.actions.First(a=>a.siteId==farm.siteId&&a.actionId=="restore-grass");
    long conversion=town.ConversionCost(farm.siteId);
    Check(conversion==2500000,"Farm conversion has $25K preparation cost");
    town.QueueAction(farm.siteId,meadow.actionId,town.Revision);
    Check(town.CashCents==cash&&town.Assets.Single().actionId==farm.actionId,"Queued conversion leaves cash and accepted use intact");
    town.UndoPlan(town.Revision);Check(town.Queue.Length==0&&town.Assets.Single().actionId==farm.actionId,"Undo conversion restores prior use");
    town.RedoPlan(town.Revision);Check(town.Queue.Single().actionId==meadow.actionId,"Redo conversion is stable");
    town=AnnualTown.Restore(d,Clone(town.Snapshot()));
    var request=town.BeginResolution("convert-year-2",town.Revision);
    town=AnnualTown.Restore(d,Clone(town.Snapshot()));
    var converted=town.CompleteResolution(provider.Resolve(request));
    Check(converted.ledger.upfrontPaidCents==meadow.upfrontPublicCostCents+conversion,"Conversion price paid once, without historic refund");
    Check(town.Assets.Length==1&&town.Assets[0].actionId==meadow.actionId,"Conversion replaces rather than duplicates asset");
    var row=converted.outcomes.sites.Single(s=>s.siteId==farm.siteId);
    Check(row.phase==TownAssetPhase.Building&&row.maturity==0,"Conversion has a transition year");
    Check(row.productionUnits==0&&converted.ledger.operatingPaidCents==0,"Former crop and its upkeep stop during conversion");
    Check(JsonSerializer.Serialize(town.History[0],options)==firstYear,"Conversion preserves accepted historical evidence");
    long once=town.CashCents;town.CompleteResolution(provider.Resolve(request));Check(town.CashCents==once,"Repeated completion cannot double-charge");
    town.ContinuePlanning(town.Revision);var establishing=Advance(town).outcomes.sites.Single(s=>s.siteId==farm.siteId);
    Check(establishing.phase==TownAssetPhase.Establishing&&establishing.maturity>0&&establishing.maturity<1,"Meadow establishes gradually after conversion");
    AnnualTownDemo.Finish(town,provider);
    var loaded=AnnualTown.Restore(d,Clone(town.Snapshot()));
    Check(loaded.History.Length==10&&loaded.CashCents==town.CashCents,"Converted ten-year run reloads exactly");
    Check(AnnualChapterNarrative.Decade(d,loaded.History).Contains("changed direction"),"Decade narrative includes land-use changes");

    foreach(int years in new[]{1,3,5,10})
    {
        var shortRun=new AnnualTown(d,"rounds-"+years+"-"+location.PackId);
        shortRun.ConfigurePlanningYears(years,shortRun.Revision);
        shortRun=AnnualTown.Restore(d,Clone(shortRun.Snapshot()));
        Check(shortRun.PlanningYears==years,"Decision years persist");
        for(int y=1;y<=10;y++)
        {
            if(shortRun.Phase==TownPhase.Reviewing)shortRun.ContinuePlanning(shortRun.Revision);
            if(y==1)shortRun.QueueAction(farm.siteId,farm.actionId,shortRun.Revision);
            if(y>years)Reject(()=>shortRun.QueueAction(farm.siteId,meadow.actionId,shortRun.Revision),"No decisions beyond configured horizon");
            Advance(shortRun);
            shortRun=AnnualTown.Restore(d,Clone(shortRun.Snapshot()));
        }
        Check(shortRun.History.Length==10&&shortRun.Phase==TownPhase.Completed,"Short play still resolves ten annual reports");
        Check(shortRun.History.Skip(years).All(r=>r.acceptedRequest.actions.Length==0),"Later years simulate with no new interventions");
        Check(shortRun.History.Skip(years).All(r=>r.ledger.operatingDueCents>0),"Ongoing costs continue after decision years");
    }
    var fullRules=Clone(d); fullRules.startingCashCents=10000000000L; // Explicit $100M fixture isolates full-plot behavior from strategy financing.
    var full=new AnnualTown(fullRules,"all-plots-"+location.PackId);
    Check(!full.AllPlotsAssigned,"Open landscape is not full");
    foreach(var site in d.sites.Where(s=>!s.protectedSite))
    {
        var a=d.actions.Where(a=>a.siteId==site.id&&a.actionId!="cultivate-margin").OrderBy(a=>a.upfrontPublicCostCents).First();
        full.QueueAction(a.siteId,a.actionId,full.Revision);
    }
    Check(full.AllPlotsAssigned,"Full plot flag includes queued plans and excludes protected land");
    var beforeFull=Clone(full.Snapshot());AnnualTownDemo.Finish(full,provider);
    Check(full.Phase==TownPhase.Completed&&full.History.Length==10,"Full-land fast-forward uses all ten transitions");
    var stepwise=AnnualTown.Restore(fullRules,beforeFull);for(int y=1;y<=10;y++){if(y>1)stepwise.ContinuePlanning(stepwise.Revision);Advance(stepwise);}
    Check(stepwise.CashCents==full.CashCents&&stepwise.History.Last().outcomes.environmentalScore==full.History.Last().outcomes.environmentalScore,"Skip and manual waiting have identical final finances and outcomes");
    Console.WriteLine(location.PackId+": affordability, conversion, all horizons, full-land skip passed");
}
Directory.CreateDirectory("docs/evidence/annual-feedback");
File.WriteAllText("docs/evidence/annual-feedback/model-checks.json",JsonSerializer.Serialize(new{checks},options));
Console.WriteLine("Annual feedback checks passed: "+checks);
