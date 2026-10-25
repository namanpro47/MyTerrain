using System.Linq;
using System.Collections.Generic;
using Terra.Contracts;
using TMPro;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        readonly Dictionary<string,TextMeshPro> annualLabels=new Dictionary<string,TextMeshPro>();
        public void AnnualViewScale(float worldPerCssPixel)
        { foreach(var label in annualLabels.Values)if(label!=null){label.transform.localScale=Vector3.one*worldPerCssPixel;if(Camera.main!=null)label.transform.rotation=Camera.main.transform.rotation;} }
        public void AnnualStage(Site site,string stage,double maturity)
        {
            if(annualLabels.TryGetValue(site.id,out var previous)&&previous!=null)
            {if(Application.isPlaying)Object.Destroy(previous.gameObject);else Object.DestroyImmediate(previous.gameObject);}
            annualLabels.Remove(site.id);
            // Finished projects speak through their models; only transitional/warning labels stay on the map.
            if(stage=="OPERATING")return;
            if(stage==null||!proposals.TryGetValue(site.id,out var holder))return;
            // Construction hides the finished model while retaining its accurately sized foundation.
            if(stage=="BUILDING")
                foreach(Transform child in holder)child.gameObject.SetActive(child.name=="Level foundation");
            if(stage=="PROPOSED"||stage=="SUSPENDED"||stage=="ESTABLISHING")
            {
                Color statusTint=stage=="PROPOSED"?new Color(.66f,.85f,.86f):stage=="SUSPENDED"?Color.gray:new Color(.55f,.46f,.28f);
                float tintStrength=stage=="PROPOSED"?.65f:stage=="SUSPENDED"?.75f:1-Mathf.Clamp01((float)maturity);
                foreach(var renderer in holder.GetComponentsInChildren<Renderer>())
                {
                    if(renderer.sharedMaterial==null||!renderer.sharedMaterial.HasColor("_BaseColor"))continue;
                    var block=new MaterialPropertyBlock();
                    Color original=renderer.sharedMaterial.GetColor("_BaseColor");
                    Color tint=stage=="PROPOSED"?Color.Lerp(original,new Color(.66f,.85f,.86f),.65f):stage=="SUSPENDED"?Color.Lerp(original,Color.gray,.75f):Color.Lerp(new Color(.55f,.46f,.28f),original,(float)maturity);
                    block.SetColor("_BaseColor",tint);renderer.SetPropertyBlock(block);
                }
                // Only agreement vegetation changes status. The source cover remaining around
                // a building is not part of that investment and keeps its original appearance.
                surfaceTreatments.TryGetValue(site.id,out var treatment);
                // Establishing meadow is young green growth, not a simulated die-off.
                if(clusteredWoodland&&stage=="ESTABLISHING"&&(treatment=="restore-grass"||treatment=="stewardship-agreement"))
                    statusTint=new Color(.72f,.79f,.51f);
                var vegetation=buildRules?.Find(treatment)?.landscape==true?holder:
                    treatment=="wind"?holder.Find("Wind agreement meadow"):null;
                if(vegetation!=null)
                    foreach(var nature in vegetation.GetComponentsInChildren<InstancedNature>())nature.SetTint(statusTint,tintStrength);
            }
            float x=site.polygon.Average(p=>p.x),z=site.polygon.Average(p=>p.z);
            var go=new GameObject("Annual status");go.transform.SetParent(holder,false);
            go.transform.position=new Vector3(x,Height(x,z)+12,z);go.transform.rotation=Quaternion.Euler(37,-32,0);
            var text=go.AddComponent<TextMeshPro>();text.font=TMP_Settings.defaultFontAsset;
            text.text=stage=="ESTABLISHING"?"Growing "+(maturity*100).ToString("0")+"%":stage=="PROPOSED"?"Queued":stage=="BUILDING"?"Building":stage=="SUSPENDED"?"Paused":stage=="OPERATING"?"Operating":stage;
            text.fontSize=140;annualLabels[site.id]=text;text.alignment=TextAlignmentOptions.Center;text.color=stage=="PROPOSED"?new Color(1,.86f,.46f):Color.white;
            text.rectTransform.sizeDelta=new Vector2(800,180);text.renderer.sortingOrder=5;
        }
    }
}
