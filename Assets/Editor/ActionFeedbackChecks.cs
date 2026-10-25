using System;
using System.Linq;
using System.Reflection;
using Terra.Core;
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
            // All authored cues have signal, bounded amplitude, and distinct note contours.
            var chime=typeof(TerraMain).GetMethod("ChangeChime",BindingFlags.Static|BindingFlags.NonPublic);
            foreach(var notes in new[]{new[]{660f,440f},new[]{440f,740f},new[]{523f,784f},new[]{392f,294f},new[]{523f,494f}})
            {
                var clip=(AudioClip)chime.Invoke(null,new object[]{"test",notes[0],notes[1]});
                var samples=new float[clip.samples];clip.GetData(samples,0);
                Check(samples.Any(v=>Mathf.Abs(v)>.05f)&&samples.All(v=>Mathf.Abs(v)<.8f),"audible nonclipping cue");
                Check(Mathf.Abs(samples[0])<.001f&&Mathf.Abs(samples.Last())<.001f,"click-free cue envelope");
                UnityEngine.Object.DestroyImmediate(clip);
            }
        }
        finally{hud.FeedbackSound-=heard;}
        Debug.Log("ACTION_FEEDBACK_UI_CHECKS_PASSED: "+checks);return checks;
    }
}
