using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        // Illustrative plant communities, not surveyed species or a vegetation model.
        // Broad low/tall swaths and flowering pockets carry the change at play scale.
        // Stable samples let the existing accepted maturity grow the same planting.
        void PlantRestorationMeadow(Point[] polygon,Transform parent,float maturity)
        {
            maturity=Mathf.Clamp01(maturity);
            var grass=Resources.Load<GameObject>("Nature/Grass_Common_Short");
            var flowerA=Resources.Load<GameObject>("Nature/Flower_3_Group");
            var flowerB=Resources.Load<GameObject>("Nature/Flower_4_Group");
            var low=new List<Matrix4x4>();var tall=new List<Matrix4x4>();
            var warm=new List<Matrix4x4>();var light=new List<Matrix4x4>();
            float l=polygon.Min(p=>p.x),r=polygon.Max(p=>p.x),b=polygon.Min(p=>p.z),t=polygon.Max(p=>p.z);
            var rng=new System.Random(5501);
            float J(float min,float max)=>min+(max-min)*(float)rng.NextDouble();
            for(float z=Mathf.Ceil(b/1.8f)*1.8f;z<t;z+=1.8f)
            for(float x=Mathf.Ceil(l/1.8f)*1.8f;x<r;x+=1.8f)
            {
                float px=x+J(-.65f,.65f),pz=z+J(-.65f,.65f);
                if(!Contains(polygon,px,pz)||NearLine(px,pz,2))continue;
                float drift=Mathf.PerlinNoise(px*.028f+184,pz*.028f+629)*.8f
                    +Mathf.PerlinNoise(px*.077f+71,pz*.077f+29)*.2f;
                float growth=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.36f,.68f,drift));
                float size=J(.85f,1.15f),yaw=J(0,360);
                float height=Mathf.Lerp(.3f,2.3f,growth)*size*Mathf.Lerp(.25f,1,maturity);
                float width=Mathf.Lerp(2.1f,4.2f,growth)*Mathf.Lerp(.65f,1,maturity);
                var matrix=PlantMatrix(grass,px,pz,height,yaw)*Matrix4x4.Scale(new Vector3(width/height,1,width/height));
                (growth>.5f?tall:low).Add(matrix);
                float bloom=Mathf.PerlinNoise(px*.065f+433,pz*.065f+112);
                // Consume all samples at every age so flowers grow in place on reload/year change.
                float chance=J(0,1),bloomSize=J(.9f,1.4f),flowerYaw=J(0,360);
                if(growth>.12f&&growth<.65f&&bloom>.57f&&chance<.5f*maturity*maturity)
                {
                    bool useWarm=Mathf.PerlinNoise(px*.022f+51,pz*.022f+87)>.5f;
                    var model=useWarm?flowerA:flowerB;
                    (useWarm?warm:light).Add(PlantMatrix(model,px,pz,bloomSize*Mathf.Lerp(.65f,1,maturity),flowerYaw));
                }
            }
            void Add(string name,GameObject source,List<Matrix4x4> plants,Color tint,bool soft=false)
            {
                if(plants.Count==0)return;
                var layer=new GameObject(name);layer.transform.SetParent(parent,false);
                layer.AddComponent<InstancedNature>().Add(source,plants,tint,soft);
            }
            Add("Restoration low sward",grass,low,new Color(.84f,.96f,.72f),true);
            Add("Restoration taller swaths",grass,tall,new Color(.82f,.94f,.72f),true);
            Add("Restoration warm flowers",flowerA,warm,Color.white);
            Add("Restoration light flowers",flowerB,light,Color.white);
        }
    }
}
