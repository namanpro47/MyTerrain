using System;
using System.Linq;
using Terra.Contracts;
using Terra.Core;

static class GameplayChecks
{
    static int checks;
    static void Check(bool condition,string message){checks++;if(!condition)throw new Exception(message);}
    static void Reject(Action action){bool rejected=false;try{action();}catch(ArgumentException){rejected=true;}catch(InvalidOperationException){rejected=true;}Check(rejected,"Expected rejection");}
    public static int Run()
    {
        var d=AnnualTownChecks.Fixture();
        var town=new AnnualTown(d,"queue-edits");var first=d.actions[0];
        town.QueueAction(first.siteId,first.actionId,town.Revision);var cash=town.CashCents;var reserve=town.QueuedCommitmentCents;
        town.UndoPlan(town.Revision);Check(town.CanRedo&&town.Queue.Length==0,"Undo creates redo");
        town=AnnualTown.Restore(d,town.Snapshot());town.RedoPlan(town.Revision);
        Check(town.Queue.Length==1&&town.CashCents==cash&&town.QueuedCommitmentCents==reserve,"Redo restores reservation without cash change");
        var before=town.Snapshot();Reject(()=>town.MoveQueuedAction(first.siteId,"not-a-site",town.Revision));
        Check(town.Revision==before.revision&&town.Queue[0].siteId==first.siteId,"Failed move is atomic");
        town.ClearPlan(town.Revision);town.UndoPlan(town.Revision);Check(town.Queue.Length==1,"Clear is undoable");
        var forecast=AnnualGameplay.Forecast(town);Check(town.CashCents==cash&&town.History.Length==0&&town.Queue.Length==1,"Forecast never settles live run");
        var accepted=AnnualTownDemo.Advance(town,"year-one",new AuthoredAnnualConsequenceProvider());
        Check(accepted.ledger.closingCashCents==forecast.ledger.closingCashCents,"Preview matches accepted ledger");
        Check(!town.CanUndo&&!town.CanRedo,"Settled years clear edit history");
        Reject(()=>town.UndoPlan(town.Revision));town.ContinuePlanning(town.Revision);
        Reject(()=>town.RemoveQueuedAction(first.siteId,town.Revision));
        var services=d.actions[0];services.servicesFollowProduction=false;services.constructionYears=0;services.establishmentYears=1;services.serviceUnits=4;
        d.drivers[0].productionFactor=.5;var independent=new AnnualTown(d,"services");independent.QueueAction(services.siteId,services.actionId,0);
        var report=AnnualTownDemo.Advance(independent,"services-y1",new AuthoredAnnualConsequenceProvider());
        Check(report.outcomes.sites.First(s=>s.siteId==services.siteId).serviceUnits==4,"Homes and recreation don't follow farm weather");
        return checks;
    }
}
