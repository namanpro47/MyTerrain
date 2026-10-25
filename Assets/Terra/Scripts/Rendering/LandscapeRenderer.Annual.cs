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
        { foreach(var label in annualLabels.Values)if(label!=null)label.transform.localScale=Vector3.one*worldPerCssPixel; }
        public void AnnualStage(Site site,string stage,double maturity)
        {
            annualLabels.Remove(site.id);
            if(stage==null||!proposals.TryGetValue(site.id,out var holder))return;
            // Construction hides the finished model while retaining its accurately sized foundation.
            if(stage=="BUILDING")
                foreach(Transform child in holder)if(child.name!="Level foundation")child.gameObject.SetActive(false);
            if(stage=="PROPOSED"||stage=="SUSPENDED"||stage=="ESTABLISHING")
            {
                foreach(var renderer in holder.GetComponentsInChildren<Renderer>())
                {
                    var block=new MaterialPropertyBlock();
                    Color original=renderer.sharedMaterial!=null&&renderer.sharedMaterial.HasProperty("_BaseColor")?renderer.sharedMaterial.GetColor("_BaseColor"):Color.white;
                    Color tint=stage=="PROPOSED"?Color.Lerp(original,new Color(.66f,.85f,.86f),.65f):stage=="SUSPENDED"?Color.Lerp(original,Color.gray,.75f):Color.Lerp(new Color(.55f,.46f,.28f),original,(float)maturity);
                    block.SetColor("_BaseColor",tint);renderer.SetPropertyBlock(block);
                }
            }
            float x=site.polygon.Average(p=>p.x),z=site.polygon.Average(p=>p.z);
            var go=new GameObject("Annual status");go.transform.SetParent(holder,false);
            go.transform.position=new Vector3(x,Height(x,z)+12,z);go.transform.rotation=Quaternion.Euler(37,-32,0);
            var text=go.AddComponent<TextMeshPro>();text.font=TMP_Settings.defaultFontAsset;
            text.text=stage=="ESTABLISHING"?"ESTABLISHING "+(maturity*100).ToString("0")+"%":stage;
            text.fontSize=140;annualLabels[site.id]=text;text.alignment=TextAlignmentOptions.Center;text.color=stage=="PROPOSED"?new Color(1,.86f,.46f):Color.white;
            text.rectTransform.sizeDelta=new Vector2(800,180);text.renderer.sortingOrder=5;
        }
    }
}
