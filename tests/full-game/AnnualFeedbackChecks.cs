using System;
using System.Linq;
using Terra.Contracts;
using Terra.Core;

public static class AnnualFeedbackChecks
{
    public static int Run(AnnualTownDefinition definition)
    {
        int checks=0;
        void Check(bool value,string why){checks++;if(!value)throw new Exception("Feedback: "+why);}
        var baseline=AnnualGameplay.Baseline(definition);
        var provider=new AuthoredAnnualConsequenceProvider();
        foreach(var action in definition.actions.GroupBy(a=>a.actionId).Select(g=>g.First()))
        {
            var town=new AnnualTown(definition,"feedback-"+action.actionId);
            var tracker=new AnnualFeedbackTracker();
            AnnualFeedback Observe(string cause=null)=>tracker.Observe(town,baseline,AnnualGameplay.Forecast(town).outcomes,cause);
            Check(Observe()==null,"initial arrival is silent");
            long cash=town.CashCents,available=town.AvailableCents;
            var prior=AnnualGameplay.Forecast(town).outcomes;
            town.QueueAction(action.siteId,action.actionId,town.Revision);
            var queued=Observe("Queued "+action.title);
            Check(queued!=null&&!queued.accepted&&queued.availableDelta==town.AvailableCents-available,"exact reservation "+action.actionId);
            Check(queued.cashDelta==0&&town.CashCents==cash,"reservation does not claim payment");
            var forecast=AnnualGameplay.Forecast(town).outcomes;
            Check(queued.scores[0]==AnnualFeedback.DisplayScore(forecast.economicScore),"forecast target");
            Check(queued.scoreDeltas[0]==Math.Round(AnnualFeedback.DisplayScore(forecast.economicScore)-AnnualFeedback.DisplayScore(prior.economicScore),1),"forecast difference matches displayed precision");
            Check(Observe()==null,"repeated refresh never repeats cue");
            town.UndoPlan(town.Revision);var undone=Observe("Undo");
            Check(undone.availableDelta==-queued.availableDelta&&undone.tone==AnnualFeedbackTone.Release,"undo release");
            Check(undone.scoreDeltas.Zip(queued.scoreDeltas,(a,b)=>a+b).All(x=>x==0),"undo reverses forecast");
            town.RedoPlan(town.Revision);var redone=Observe("Redo");
            Check(redone.availableDelta==queued.availableDelta,"redo reserves once");
            var request=town.BeginResolution("year-1",town.Revision);
            Check(Observe()==null,"pending does not claim acceptance");
            town.CancelResolution(town.Revision);Check(Observe()==null,"cancel/retry does not duplicate feedback");
            var beforeAvailable=town.AvailableCents;
            var report=AnnualTownDemo.Advance(town,"accepted-1",provider);
            var accepted=Observe();
            Check(accepted.accepted&&accepted.cashDelta==report.ledger.closingCashCents-report.ledger.openingCashCents,"actual accepted cash");
            Check(accepted.availableDelta==town.AvailableCents-beforeAvailable,"actual available after acceptance");
            Check(accepted.scoreDeltas[0]==Math.Round(AnnualFeedback.DisplayScore(report.outcomes.economicScore)-AnnualFeedback.DisplayScore(baseline.economicScore),1),"accepted compares actual baseline, not forecast");
            Check(Observe()==null,"reading a report is silent");
            var reload=new AnnualFeedbackTracker();Check(reload.Observe(town,baseline,report.outcomes)==null,"restored report is silent");
            town.ContinuePlanning(town.Revision);var next=Observe();
            Check(next==null||(!next.accepted&&next.cashDelta==0&&next.title=="Year 2 forecast"),"continue labels forecast honestly");
            var previousCash=town.CashCents;
            AnnualTownDemo.Finish(town,provider);var finish=Observe();
            Check(finish.accepted&&finish.cashDelta==town.CashCents-previousCash,"fast-forward aggregates actual delta");
            Check(Observe()==null,"completed refresh is silent");
            var replay=new AnnualTown(definition,"replay");Check(tracker.Observe(replay,baseline,AnnualGameplay.Forecast(replay).outcomes)==null,"new run is silent");
        }
        var edit=new AnnualTown(definition,"edit");var edits=new AnnualFeedbackTracker();
        AnnualFeedback Edit(string title=null)=>edits.Observe(edit,baseline,AnnualGameplay.Forecast(edit).outcomes,title);
        Edit();var farm=definition.actions.First(a=>a.actionId=="cultivate-standard");edit.QueueAction(farm.siteId,farm.actionId,edit.Revision);Edit("Queue");
        long old=edit.AvailableCents;
        var replacement=definition.actions.First(a=>a.siteId==farm.siteId&&a.actionId=="conservation-rotation");
        edit.QueueAction(replacement.siteId,replacement.actionId,edit.Revision);var replaced=Edit("Replace");
        Check(replaced.availableDelta==edit.AvailableCents-old,"replacement reports net, not gross cost");
        var target=definition.actions.First(a=>a.siteId!=farm.siteId&&a.actionId==replacement.actionId);
        edit.MoveQueuedAction(farm.siteId,target.siteId,edit.Revision);var moved=Edit("Move");
        Check(moved!=null&&moved.cashDelta==0,"move remains visible even with unchanged money");
        old=edit.AvailableCents;edit.ClearPlan(edit.Revision);var clear=Edit("Clear");
        Check(clear.availableDelta==edit.AvailableCents-old&&clear.availableDelta>0,"clear releases full plan");
        Check(AnnualFeedback.Money(-350000)=="-$3,500"&&AnnualFeedback.Money(350000)=="+$3,500","signed exact dollars");
        Check(AnnualFeedback.Points(0)=="0.0"&&!AnnualFeedback.Points(0).Contains("-"),"no negative zero");
        Check(AnnualFeedback.Score(50.149)=="50.1"&&AnnualFeedback.Score(58.251)=="58.3"&&AnnualFeedback.DisplayDelta(50.149,58.251)==8.2,"visible Economic score difference, not rounded hidden difference");
        Check(AnnualFeedback.DisplayDelta(38.849,38.951)==.2,"visible Environmental score difference");
        Check(AnnualFeedback.DisplayDelta(58.251,50.149)==-8.2,"decline reverses the same displayed difference");
        Check(AnnualFeedback.Score(10.05)=="10.1"&&AnnualFeedback.DisplayDelta(10.05,10.15)==.1,"consistent midpoint rounding");
        Check(AnnualFeedback.DisplayDelta(50.11,50.14)==0&&AnnualFeedback.Points(AnnualFeedback.DisplayDelta(50.14,50.11))=="0.0","invisible precision changes do not produce a signed zero");
        Console.WriteLine("Annual feedback checks passed: "+checks);return checks;
    }
}
