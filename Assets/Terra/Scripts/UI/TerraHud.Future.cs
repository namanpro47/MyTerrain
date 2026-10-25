using System;
using System.Linq;
using Terra.Core;
using UnityEngine;

namespace Terra.UI
{
    public sealed partial class TerraHud
    {
        public void ConfigureFuture(Action future)
        {
            futureShortcut=future;
            var button=futureButton=Button(root,"Ten-year future",new Vector2(0,0),new Vector2(24,328),new Vector2(164,44),future,14);
            button.transform.SetSiblingIndex(modalShade.transform.GetSiblingIndex());
        }
        public void AnnualFuture(AnnualTown town,AnnualTown comparison,Action compare,Action timeline,Action takeaway)
        {
            var h=town.History;var ours=h.Last();var other=comparison.History.Last();var o=ours.outcomes;var b=other.outcomes;
            double[] difference={o.economicScore-b.economicScore,o.socialScore-b.socialScore,o.environmentalScore-b.environmentalScore};
            string[] names={"Economic","Social","Environmental"};int gain=Array.IndexOf(difference,difference.Max()),cost=Array.IndexOf(difference,difference.Min());
            string largest="Largest relative gain: "+names[gain]+" "+Signed(difference[gain])+". Remaining trade-off: "+names[cost]+" "+Signed(difference[cost])+".";
            if(difference.Max()<=0)largest="No indicator exceeds the no-new-action path. Largest gap: "+names[cost]+" "+Signed(difference[cost])+". Try another allocation.";
            if(difference.Min()>=0)largest="All three indicators match or exceed no new action. These gains carry public costs and ongoing commitments.";
            if(difference.All(x=>Math.Abs(x)<.00001))largest="Your indicators match the no-new-action path. Different choices can change needs met, costs and land use.";
            var targets=town.Definition.economy;
            string assessment="Scenario targets: E "+targets.finalEconomicTarget+" / S "+targets.finalSocialTarget+" / Env "+targets.finalEnvironmentalTarget+". Met "+((o.economicScore>=targets.finalEconomicTarget?1:0)+(o.socialScore>=targets.finalSocialTarget?1:0)+(o.environmentalScore>=targets.finalEnvironmentalTarget?1:0))+" of 3; each is independent.";
            string text="<size=88%>YOUR YEAR 10                 NO NEW ACTION\nEconomic  <b>"+o.economicScore.ToString("0.0")+"</b>                       "+b.economicScore.ToString("0.0")+"\nSocial  <b>"+o.socialScore.ToString("0.0")+"</b>                              "+b.socialScore.ToString("0.0")+"\nEnvironmental  <b>"+o.environmentalScore.ToString("0.0")+"</b>               "+b.environmentalScore.ToString("0.0")+"\n"+assessment+"\n\n"+largest+
                "\n\nPublic spending "+Money(h.Sum(r=>r.ledger.upfrontPaidCents+r.ledger.operatingPaidCents+r.ledger.programPaidCents+r.ledger.repairPaidCents+r.ledger.arrearsPaidCents))+"\nClosing cash "+Money(town.CashCents)+" · no-action cash "+Money(comparison.CashCents)+"\nReceipts "+Money(h.Sum(r=>r.ledger.publicReceiptsCents))+" · appropriations "+Money(h.Sum(r=>r.ledger.appropriationCents))+"\nOutstanding beyond Year 10: "+Money(ours.ledger.futureCommitmentsCents)+" (includes arrears "+Money(ours.ledger.closingArrearsCents)+")\n\nSame opening services, weather, appropriation and rules. Scenario indicators, not a forecast. Inspect the map, history or choose New run.</size>";
            AnnualDialog("Ten years in "+location.Name,text,"Compare the landscape",compare);
            AnnualSecondary("History & takeaway",timeline);
        }
    }
}
