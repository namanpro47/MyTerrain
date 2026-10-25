using System.Linq;
using Terra.Contracts;
using TMPro;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        public void AnnualStage(Site site,string stage,double maturity)
        {
            if(stage==null||!proposals.TryGetValue(site.id,out var holder))return;
            // Construction hides the finished model while retaining its accurately sized foundation.
            if(stage=="BUILDING")
                foreach(Transform child in holder)if(child.name!="Level foundation")child.gameObject.SetActive(false);
            float x=site.polygon.Average(p=>p.x),z=site.polygon.Average(p=>p.z);
            var go=new GameObject("Annual status");go.transform.SetParent(holder,false);
            go.transform.position=new Vector3(x,Height(x,z)+12,z);go.transform.rotation=Quaternion.Euler(37,-32,0);
            var text=go.AddComponent<TextMeshPro>();text.font=TMP_Settings.defaultFontAsset;
            text.text=stage=="ESTABLISHING"?"ESTABLISHING "+(maturity*100).ToString("0")+"%":stage;
            text.fontSize=7;text.alignment=TextAlignmentOptions.Center;text.color=stage=="PROPOSED"?new Color(1,.86f,.46f):Color.white;
            text.rectTransform.sizeDelta=new Vector2(120,14);text.renderer.sortingOrder=5;
        }
    }
}
