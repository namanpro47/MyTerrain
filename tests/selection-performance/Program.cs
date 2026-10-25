using System.Text.Json;
using Terra.Contracts;
using Terra.Core;

var options=new JsonSerializerOptions {IncludeFields=true};
var source=JsonSerializer.Deserialize<AnnualTownDefinition>(File.ReadAllText("docs/evidence/full-game/definition.json"),options)!;
int checks=0,forecasts=0;
string Json(object value)=>JsonSerializer.Serialize(value,value.GetType(),options);
void Check(bool ok,string why){checks++;if(!ok)throw new Exception(why);}
TownYearReport Reference(AnnualTown town)
{
    var copy=AnnualTown.Restore(town.Definition,town.Snapshot());
    if(copy.Phase==TownPhase.Reviewing||copy.Phase==TownPhase.Completed)return copy.History.Last();
    copy.NeutralPlanningDriver();
    return AnnualTownDemo.Advance(copy,copy.PendingRequest?.requestId??copy.RunId+"/preview/"+copy.Revision,new AuthoredAnnualConsequenceProvider());
}
void Compare(AnnualTown town,string label)
{
    Check(town.CopyForForecast().PlanningYears==town.PlanningYears,"Forecast preserves the configured decision horizon");
    string state=Json(town.Snapshot()),definition=Json(town.Definition);
    string expected=Json(Reference(town));var result=AnnualGameplay.Forecast(town);
    Check(Json(result)==expected,"Forecast differs from replay: "+label);
    Check(Json(town.Snapshot())==state,"Forecast mutated live state: "+label);
    Check(Json(town.Definition)==definition,"Neutral forecast mutated saved weather/rules: "+label);
    result.outcomes.sites[0].productionUnits=-999;
    Check(Json(AnnualGameplay.Forecast(town))==expected,"Returned report aliases live history: "+label);
    forecasts++;
}
foreach(var region in new[]{"legacy","meadowbrook-v1","point-reyes-v1","suusamyr-v1"})
foreach(int horizon in new[]{10,3})
{
    var d=region=="legacy"?source:AnnualPlayerRules.Create(source,region,"forecast-parity");
    var town=new AnnualTown(d,"forecast-"+region+"/"+horizon);
    town.ConfigurePlanningYears(horizon,town.Revision);
    while(town.Phase!=TownPhase.Completed)
    {
        Compare(town,region+"/empty/"+town.Year);
        var offer=d.actions.FirstOrDefault(a=>a.actionId!=AnnualPlayerRules.CancelAction&&!town.Assets.Any(x=>x.siteId==a.siteId&&x.cancelledYear==0));
        if(town.Year<=town.PlanningYears&&offer!=null&&!town.BuildingBlockedByDebt&&town.Preview(offer.siteId,offer.actionId)>=0)
        {
            town.QueueAction(offer.siteId,offer.actionId,town.Revision);Compare(town,region+"/queued/"+town.Year);
            town.UndoPlan(town.Revision);Compare(town,region+"/undo/"+town.Year);
            town.RedoPlan(town.Revision);Compare(town,region+"/redo/"+town.Year);
        }
        if(town.Year<=town.PlanningYears&&region!="legacy"&&town.Year%3==0)
        {
            var active=town.Assets.FirstOrDefault(a=>a.cancelledYear==0);
            if(active!=null){town.QueueCancellation(active.siteId,town.Revision);Compare(town,region+"/cancellation/"+town.Year);}
        }
        var pending=town.BeginResolution("parity/"+town.Year,town.Revision);
        Compare(town,region+"/pending/"+town.Year);
        town.CompleteResolution(new AuthoredAnnualConsequenceProvider().Resolve(pending));
        Compare(town,region+"/accepted/"+town.Year);
        if(town.Phase==TownPhase.Reviewing)town.ContinuePlanning(town.Revision);
    }
    Compare(town,region+"/completed");
}
Console.WriteLine($"Forecast parity passed: {checks} assertions across {forecasts} state/forecast comparisons");
