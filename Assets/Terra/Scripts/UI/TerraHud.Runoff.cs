using System;
using System.Linq;
using Terra.Contracts;
using TMPro;
using UnityEngine;

namespace Terra.UI
{
    // TN-03/05, CS-02/04: estimates use the same storm; no synthetic ecological score.
    public sealed partial class TerraHud
    {
        TextMeshProUGUI runoffValue,runoffBaseline;string runoffContextSummary="";
        public void ConfigureRunoff(float rainfallMm)
        {
            var measures=root.Find("Measures");var rect=(RectTransform)measures;rect.sizeDelta=new Vector2(400,154);
            runoffStatus.rectTransform.sizeDelta=new Vector2(365,24);
            runoffStatus.text=rainfallMm.ToString("0")+" mm storm · direct runoff estimate";
            runoffValue=Label(measures,"",new Vector2(0,1),new Vector2(18,-91),new Vector2(365,28),20,Paper,FontStyles.Bold);
            runoffBaseline=Label(measures,"",new Vector2(0,1),new Vector2(18,-124),new Vector2(365,23),14,Soft);
            ((RectTransform)toastPanel.transform).anchoredPosition=new Vector2(0,-190);
            var previewRect=(RectTransform)tooltip.transform;previewRect.anchorMin=previewRect.anchorMax=previewRect.pivot=new Vector2(1,1);previewRect.anchoredPosition=new Vector2(-24,-190);
            previewBody.rectTransform.anchoredPosition=new Vector2(18,-96);previewBody.rectTransform.sizeDelta=new Vector2(256,108);
            coverSummary.rectTransform.anchoredPosition=new Vector2(18,-210);coverSummary.rectTransform.sizeDelta=new Vector2(256,24);
            previewAction.rectTransform.sizeDelta=new Vector2(256,64);sizes[previewAction]=14;previewAction.fontSize=14;
        }
        static string Amount(float value)=>value.ToString("N0");
        static string Delta(float value)=>Math.Abs(value)<.5f?(value==0?"0":value<0?"−<1":"+<1"):(value<0?"−":"+")+Amount(Math.Abs(value));
        static float Fact(WorldResult r,string code)=>r.explanations.FirstOrDefault(f=>f.code==code&&string.IsNullOrEmpty(f.siteId))?.value??0;
        public void RunoffResult(WorldResult result)
        {
            if(runoffValue==null)return;
            runoffContextSummary="Before storage: sites ≈ "+Amount(Fact(result,"managed-generated"))+" m³; context ≈ "+Amount(Fact(result,"context-generated"))+" m³.";
            runoffValue.text="≈ "+Amount(result.dischargedM3)+" m³   (Δ "+Delta(result.dischargedM3-Fact(result,"baseline-discharge"))+")";
            runoffBaseline.text="Baseline ≈ "+Amount(Fact(result,"baseline-discharge"))+" m³ · Held "+Amount(result.retainedM3)+" m³";
        }
        public void RunoffPreview(Site site,Evaluation evaluation,WorldResult current,string[] connections,bool waterTool,string mode)
        {
            if(!tooltip.activeSelf||!current.runoffAvailable)return;
            var proposal=evaluation.valid?evaluation.result:current;
            float vegetation=proposal.coverAreas.Where(c=>c.coverId=="grass"||c.coverId=="woodland").Sum(c=>c.areaM2)/10000;
            coverSummary.text="Managed vegetation: "+vegetation.ToString("0.00")+" ha";
            string effect=evaluation.valid?"Δ runoff: "+Delta(proposal.dischargedM3-current.dischargedM3)+" m³":"Same 50 mm storm · estimate";
            if(waterTool&&site!=null)
            {
                float capacity=site.offers.FirstOrDefault(o=>o.treatmentId=="water-swale")?.retentionCapacityM3??0;
                float used=proposal.explanations.FirstOrDefault(f=>f.code=="retention-capacity-used"&&f.siteId==site.id)?.value??0;
                previewAction.text=effect+"\nStorage used: "+Amount(used)+" / "+Amount(capacity)+" m³\nAssumed group: "+connections.Length+" sites";
            }
            else
            {
                float generated=proposal.generatedM3-current.generatedM3;
                string reason=mode=="reset"?"Original cover and storage restored.":!evaluation.valid?"Choose a treatment to compare.":generated<-.01f?"Cover generates less runoff.":generated>.01f?"Cover generates more runoff.":"Cover runoff stays the same.";
                previewAction.text=effect+"\n"+reason;
            }
        }
    }
}
