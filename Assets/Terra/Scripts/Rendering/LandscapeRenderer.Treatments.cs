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
            // Surface changes live on the terrain material; plants and swale geometry remain independently reversible.
            if(cultivated)
            {
                if(treatment=="cultivate-margin")
                {
                    float edge=b+(t-b)*.25f;var margin=Rectangle(l,b,r,edge);
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
                float px=x+J(-spacing*.48f,spacing*.48f),pz=z+J(-spacing*.48f,spacing*.48f);
                if(!Contains(polygon,px,pz)||(exclude?.Invoke(px,pz)??false))continue;
                float cluster=Mathf.PerlinNoise((px+1000)*.035f,(pz+1000)*.035f);
                int stems=cluster>.50f?3:1;
                for(int n=0;n<stems;n++)
                {
                    float sx=px+J(-.8f,.8f),sz=pz+J(-.8f,.8f);
                    if(Contains(polygon,sx,sz)&&!(exclude?.Invoke(sx,sz)??false))
                        small.Add(PlantMatrix(tuft,sx,sz,J(.38f,.70f),J(0,360)));
                }
                if(rng.NextDouble()<.30)fine.Add(PlantMatrix(tall,px,pz,J(.55f,.95f),J(0,360)));
                if(withFlowers&&cluster>.55f&&rng.NextDouble()<.65)
                {
                    bool second=cluster>.62f;var model=second?bloom2:bloom;
                    (second?flowers2:flowers).Add(PlantMatrix(model,px+.6f,pz+.4f,J(.27f,.45f),J(0,360)));
                }
            }
            var nature=parent.gameObject.AddComponent<InstancedNature>();nature.Add(tuft,small,Color.white,true);nature.Add(tall,fine,Color.white,true);
            nature.Add(bloom,flowers,Color.white);nature.Add(bloom2,flowers2,Color.white);
        }
        void PlantCultivation(Point[] polygon,Transform parent,bool baseline)
        {
            float l=polygon.Min(v=>v.x),r=polygon.Max(v=>v.x),b=polygon.Min(v=>v.z),t=polygon.Max(v=>v.z);
            var rng=new System.Random((int)(l*71+b*131)+612);var plants=new List<Matrix4x4>();
            var grain=Resources.Load<GameObject>("Nature/FieldCluster");
            const float spacing=1.35f;
            for(float x=Mathf.Ceil((l+1)/spacing)*spacing;x<r-1;x+=spacing)
            {
                // Cultivated ground shares the field palette; individual plants supply its row rhythm.
                for(float z=Mathf.Ceil((b+1)/1.1f)*1.1f;z<t-1;z+=1.1f)
                {
                    if(!Contains(polygon,x,z)||(baseline&&!VisualCropAt(x,z))||FieldTrackAt(x))continue;
                    float size=.48f+(float)rng.NextDouble()*.12f;
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
                for(float z=Mathf.Ceil(cz/1.1f)*1.1f;z<Mathf.Min(cz+150,zmax-3);z+=1.1f)for(float x=Mathf.Ceil(cx/1.35f)*1.35f;x<Mathf.Min(cx+150,xmax-3);x+=1.35f)
                {
                    if(!VisualCropAt(x,z)||FieldTrackAt(x)||NearLine(x,z,8)||world.sites.Any(site=>Contains(site.polygon,x,z)))continue;
                    float variation=Mathf.PerlinNoise(x*.12f+500,z*.12f+500);
                    plants.Add(PlantMatrix(grain,x,z,.48f+variation*.12f,variation*12-6));
                }
                root.gameObject.AddComponent<InstancedNature>().Add(grain,plants,Color.white);
            }
        }
        // Illustrative access lanes, aligned across baseline and proposed planting, not surveyed tracks.
        static bool FieldTrackAt(float x)=>Mathf.Abs(Mathf.Repeat(x+19.2f,38.4f)-19.2f)<.8f;
    }
}
