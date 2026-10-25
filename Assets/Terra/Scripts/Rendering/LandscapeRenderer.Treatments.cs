using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        // Illustrated plant communities; their density and species are not field observations or model inputs.
        void BuildTreatment(Site site,string treatment,Transform parent,bool preview)
        {
            var p=site.polygon;float l=p.Min(v=>v.x),r=p.Max(v=>v.x),b=p.Min(v=>v.z),t=p.Max(v=>v.z);
            bool cultivated=treatment.StartsWith("cultivate");
            Color ground=cultivated?new Color(.67f,.56f,.30f):new Color(.39f,.56f,.25f);
            if(preview)ground=Color.Lerp(ground,new Color(.68f,.74f,.49f),.15f);
            Patch("Illustrative treatment soil",p,ground,.12f,parent);
            if(cultivated)
            {
                if(treatment=="cultivate-margin")
                {
                    float edge=b+(t-b)*.25f;var margin=Rectangle(l,b,r,edge);
                    Patch("Living meadow margin",margin,new Color(.40f,.57f,.26f),.16f,parent);
                    PlantMeadow(margin,parent,3.1f,true);b=edge;
                }
                PlantCultivation(Rectangle(l,b,r,t),parent,false);
            }
            else if(treatment=="restore-grass")PlantMeadow(p,parent,2.9f,true);
            else
            {
                float middle=(b+t)*.5f;
                var route=new[]{new Point(l+9,middle-3),new Point(Mathf.Lerp(l,r,.35f),middle+2),new Point(Mathf.Lerp(l,r,.65f),middle+4),new Point(r-9,middle)};
                Ribbon(route,13,new Color(.32f,.43f,.23f),.18f,parent,"Swale planted bank");
                Ribbon(route,7,new Color(.47f,.45f,.30f),.22f,parent,"Swale earth bed");
                Ribbon(route,4,new Color(.17f,.43f,.45f),.25f,parent,"Illustrative swale water");
                PlantMeadow(p,parent,3.7f,true,(x,z)=>DistanceToRoute(route,x,z)<5);
                var rush=Resources.Load<GameObject>("Nature/Grass_Common_Tall");var stone=Resources.Load<GameObject>("Nature/Rock_Medium_2");
                var grasses=new List<Matrix4x4>();var stones=new List<Matrix4x4>();
                for(float x=l+11;x<r-9;x+=4)
                {
                    float z=middle+Mathf.Sin((x-l)/(r-l)*Mathf.PI)*4;
                    foreach(float side in new[]{-1f,1f})grasses.Add(PlantMatrix(rush,x,z+side*6,2.7f,x*13));
                    if(((int)x&7)<3)stones.Add(PlantMatrix(stone,x,z-4.5f,.55f,x*7));
                }
                var instances=parent.gameObject.AddComponent<InstancedNature>();instances.Add(rush,grasses,Color.white);instances.Add(stone,stones,Color.white);
            }
        }
        static float DistanceToRoute(Point[] route,float x,float z)
        {
            var p=new Vector2(x,z);float distance=float.MaxValue;
            for(int i=1;i<route.Length;i++)
            {
                var a=new Vector2(route[i-1].x,route[i-1].z);var d=new Vector2(route[i].x,route[i].z)-a;
                float t=d.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude):0;
                distance=Mathf.Min(distance,Vector2.Distance(p,a+d*t));
            }
            return distance;
        }
        void PlantMeadow(Point[] polygon,Transform parent,float spacing,bool withFlowers,Func<float,float,bool> exclude=null)
        {
            var tuft=Resources.Load<GameObject>("Nature/Grass_Common_Short");var tall=Resources.Load<GameObject>("Nature/Grass_Wispy_Short");
            var bloom=Resources.Load<GameObject>("Nature/Flower_3_Group");var bloom2=Resources.Load<GameObject>("Nature/Flower_4_Group");
            var small=new List<Matrix4x4>();var fine=new List<Matrix4x4>();var flowers=new List<Matrix4x4>();var flowers2=new List<Matrix4x4>();
            float l=polygon.Min(v=>v.x),r=polygon.Max(v=>v.x),b=polygon.Min(v=>v.z),t=polygon.Max(v=>v.z);
            var rng=new System.Random((int)(l*71+b*131)+418);float J(float min,float max)=>min+(float)rng.NextDouble()*(max-min);
            for(float z=b+2.5f;z<t-2;z+=spacing)for(float x=l+2.5f;x<r-2;x+=spacing)
            {
                float px=x+J(-.65f,.65f),pz=z+J(-.65f,.65f);
                if(!Contains(polygon,px,pz)||(exclude?.Invoke(px,pz)??false))continue;
                small.Add(PlantMatrix(tuft,px,pz,J(1.8f,3.2f),J(0,360)));
                float cluster=Mathf.PerlinNoise((px+1000)*.06f,(pz+1000)*.06f);
                if(rng.NextDouble()<.30)fine.Add(PlantMatrix(tall,px,pz,J(2.3f,3.5f),J(0,360)));
                if(withFlowers&&rng.NextDouble()<(cluster>.52f?.40:.08))
                {
                    bool second=cluster>.62f;var model=second?bloom2:bloom;
                    (second?flowers2:flowers).Add(PlantMatrix(model,px+.6f,pz+.4f,J(1.6f,2.8f),J(0,360)));
                }
            }
            var nature=parent.gameObject.AddComponent<InstancedNature>();nature.Add(tuft,small,Color.white);nature.Add(tall,fine,Color.white);
            nature.Add(bloom,flowers,Color.white);nature.Add(bloom2,flowers2,Color.white);
        }
        void PlantCultivation(Point[] polygon,Transform parent,bool baseline)
        {
            float l=polygon.Min(v=>v.x),r=polygon.Max(v=>v.x),b=polygon.Min(v=>v.z),t=polygon.Max(v=>v.z);
            var rng=new System.Random((int)(l*71+b*131)+612);var plants=new List<Matrix4x4>();
            var grain=Resources.Load<GameObject>("Nature/FieldCluster");
            const float spacing=2.4f;
            for(float x=Mathf.Ceil((l+1)/spacing)*spacing;x<r-1;x+=spacing)
            {
                // Cultivated ground shares the field palette; individual plants supply its row rhythm.
                for(float z=Mathf.Ceil((b+1)/1.6f)*1.6f;z<t-1;z+=1.6f)
                {
                    if(!Contains(polygon,x,z)||(baseline&&!VisualCropAt(x,z))||FieldTrackAt(x))continue;
                    float size=.9f+(float)rng.NextDouble()*.2f;
                    plants.Add(PlantMatrix(grain,x+((float)rng.NextDouble()-.5f)*.2f,z,size,(float)rng.NextDouble()*12-6));
                }
            }
            parent.gameObject.AddComponent<InstancedNature>().Add(grain,plants,Color.white);
        }
        void BuildBaselinePlants(Site site,Transform parent)
        {
            PlantCultivation(site.polygon,parent,true);
            if(site.baselineCover.Any(c=>c.coverId=="grass"&&c.fraction>.5f))PlantMeadow(site.polygon,parent,7,false);
        }
        void BuildCultivatedContext()
        {
            var grain=Resources.Load<GameObject>("Nature/FieldCluster");
            for(float cz=zmin;cz<zmax;cz+=150)for(float cx=xmin;cx<xmax;cx+=150)
            {
                var plants=new List<Matrix4x4>();
                for(float z=Mathf.Ceil(cz/1.6f)*1.6f;z<Mathf.Min(cz+150,zmax-3);z+=1.6f)for(float x=Mathf.Ceil(cx/2.4f)*2.4f;x<Mathf.Min(cx+150,xmax-3);x+=2.4f)
                {
                    if(!VisualCropAt(x,z)||FieldTrackAt(x)||NearLine(x,z,8)||world.sites.Any(site=>Contains(site.polygon,x,z)))continue;
                    float variation=Mathf.PerlinNoise(x*.12f+500,z*.12f+500);
                    plants.Add(PlantMatrix(grain,x,z,.9f+variation*.2f,variation*12-6));
                }
                root.gameObject.AddComponent<InstancedNature>().Add(grain,plants,Color.white);
            }
        }
        // Illustrative access lanes, aligned across baseline and proposed planting, not surveyed tracks.
        static bool FieldTrackAt(float x)=>Mathf.Repeat(x+1200,38.4f)<2.4f;
    }
}
