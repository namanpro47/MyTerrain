using System;
using System.Linq;
using System.Reflection;
using Terra.Core;
using Terra.Contracts;
using Terra.Gameplay;
using Terra.UI;
using TMPro;
using UnityEngine;

public static class ActionFeedbackChecks
{
    public static int Run(TerraHud hud)
    {
        int checks=0,sounds=0;
        void Check(bool ok,string why){checks++;if(!ok)throw new Exception("Action feedback UI: "+why);}
        var flags=BindingFlags.Instance|BindingFlags.NonPublic;
        T Field<T>(string name)=>(T)typeof(TerraHud).GetField(name,flags).GetValue(hud);
        Action<AnnualFeedbackTone> heard=_=>sounds++;
        hud.FeedbackSound+=heard;
        try
        {
            var change=new AnnualFeedback{title="Replaced · Regenerative farming / rotation",availableDelta=-350000,detail="Reservation changed",tone=AnnualFeedbackTone.Reserve};
            change.scoreDeltas[0]=-12.3;change.scoreDeltas[1]=2.4;change.scoreDeltas[2]=0;
            change.scores[0]=32;change.scores[1]=62.4;change.scores[2]=39;
            hud.ShowChange(change);
            Check(sounds==1,"one semantic cue per change");
            var receipt=Field<Transform>("changeReceipt");
            Check(!receipt.GetComponent<CanvasGroup>().blocksRaycasts,"receipt must never steal map clicks");
            Check(receipt.GetComponentsInChildren<UnityEngine.UI.Graphic>(true).All(g=>!g.raycastTarget),"decorative receipt raycasts");
            Check(Field<TextMeshProUGUI>("changeMoney").text=="-$3,500","signed dollar popup");
            Check(Field<TextMeshProUGUI>("changeContext").text.Contains("Reserved")&&Field<TextMeshProUGUI>("changeContext").text.Contains("forecast"),"reservation and forecast labels");
            Check(Field<TextMeshProUGUI>("changeScores").text.Contains("-12.3")&&Field<TextMeshProUGUI>("changeScores").text.Contains("+2.4"),"mixed outcomes preserve both signs");
            hud.TickChangeFeedback(30);
            Check(Field<float>("changeRemaining")>=6.9f,"stalled or unfocused frame preserves reading time");
            Check(receipt.gameObject.activeSelf==!hud.ModalOpen,"unfocused receipt remains visible outside modals");
            foreach(bool large in new[]{false,true})
            {
                if(large)typeof(TerraHud).GetMethod("ToggleText",flags).Invoke(hud,null);
                foreach(var label in receipt.GetComponentsInChildren<TextMeshProUGUI>(true))
                {
                    label.ForceMeshUpdate(true);
                    Check(label.preferredHeight<=label.rectTransform.rect.height+2,"receipt clipping: "+label.text);
                }
            }
            typeof(TerraHud).GetMethod("ToggleText",flags).Invoke(hud,null);
            hud.SetReducedMotion(true);hud.ShowChange(change);Check(sounds==2,"reduced motion retains cue");
            hud.Toast("Not enough available money");Check(Field<AnnualFeedback>("activeChange")==null,"blocked-action explanation takes priority");
            for(int i=0;i<12;i++)hud.ShowChange(change);
            Check(Field<System.Collections.Generic.List<AnnualFeedback>>("recentChanges").Count==8,"bounded inspectable recent history");
            hud.ClearChangeFeedback();Check(Field<AnnualFeedback>("activeChange")==null&&Field<System.Collections.Generic.List<AnnualFeedback>>("recentChanges").Count==0,"new run clears feedback");
            hud.SetReducedMotion(false);hud.Toast("");
            // TerraAudioChecks verifies the shared bank and master/effects controls.
            // Real accepted state through all three UI surfaces. Choose baseline
            // precision near a rounding boundary so old raw subtraction is caught.
            var definition=Field<AnnualTownDefinition>("annualRules");
            var town=new AnnualTown(definition,"feedback-rounding-ui");
            var report=AnnualTownDemo.Advance(town,"rounding-year",new AuthoredAnnualConsequenceProvider());
            var baseline=AnnualGameplay.Baseline(definition);
            double[] after={report.outcomes.economicScore,report.outcomes.socialScore,report.outcomes.environmentalScore};
            double[] before=after.Select(value=>Enumerable.Range(101,99).Select(n=>value-n/1000d)
                .FirstOrDefault(valueBefore=>AnnualFeedback.Points(value-valueBefore)!=AnnualFeedback.Points(AnnualFeedback.DisplayDelta(valueBefore,value)))).ToArray();
            Check(before.Any(value=>value!=0),"fixture exercises hidden-precision mismatch");
            for(int i=0;i<3;i++)if(before[i]==0)before[i]=after[i]-.149;
            baseline.economicScore=before[0];baseline.socialScore=before[1];baseline.environmentalScore=before[2];
            hud.AnnualState(town,baseline,report.outcomes);
            for(int i=0;i<3;i++)Check(Field<string[]>("annualScoreReadout")[i].Contains("Change  "+AnnualFeedback.Points(AnnualFeedback.DisplayDelta(before[i],after[i]))),"annual HUD uses displayed delta");
            hud.PlacementAcceptedState(town,baseline);
            for(int i=0;i<3;i++)Check(Field<string[]>("annualScoreReadout")[i].Contains("Last year "+AnnualFeedback.Points(AnnualFeedback.DisplayDelta(before[i],after[i]))),"placement headline agrees with receipt");
            hud.AnnualReport(town,baseline,()=>{});
            string[] names={"Economic","Social","Environment"};
            for(int i=0;i<3;i++)Check(Field<TextMeshProUGUI>("fieldNotesBody").text.Contains(names[i]+" "+AnnualFeedback.Score(after[i])+" ("+AnnualFeedback.Points(AnnualFeedback.DisplayDelta(before[i],after[i]))+")"),"accepted ledger agrees with receipt");
            hud.CloseModal();
            var restored=new AnnualTown(definition,"feedback-ui-restored");
            hud.AnnualState(restored,AnnualGameplay.Baseline(definition),AnnualGameplay.Forecast(restored).outcomes);
        }
        finally{hud.FeedbackSound-=heard;}
        Debug.Log("ACTION_FEEDBACK_UI_CHECKS_PASSED: "+checks);return checks;
    }
}
