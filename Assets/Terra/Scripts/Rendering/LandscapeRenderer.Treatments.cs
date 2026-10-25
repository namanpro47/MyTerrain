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
            if(BuildCatalogAction(site,treatment,parent,preview))return;
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
            else PlantSwale(site,parent);
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
            // Each colony has an elongated footprint, a low connected sward and a taller
            // interior. Open grass between colonies is deliberate; this is not a grid of dots.
            float colonySpacing=withFlowers?17:24;
            for(float cz=b+8;cz<t-4;cz+=colonySpacing)for(float cx=l+8;cx<r-4;cx+=colonySpacing)
            {
                float centerX=cx+J(-6,6),centerZ=cz+J(-6,6),angle=J(0,Mathf.PI*2);
                float radius=J(6.5f,11),aspect=J(.45f,.75f),cos=Mathf.Cos(angle),sin=Mathf.Sin(angle);
                bool flowering=withFlowers&&rng.NextDouble()<.42;
                float stemSpacing=Mathf.Clamp(spacing*.4f,1.05f,2.8f);
                for(float v=-radius*aspect;v<=radius*aspect;v+=stemSpacing)for(float u=-radius;u<=radius;u+=stemSpacing)
                {
                    float a=u+J(-.5f,.5f),c=v+J(-.5f,.5f);
                    float radial=a*a/(radius*radius)+c*c/(radius*radius*aspect*aspect);
                    if(radial>1||rng.NextDouble()<radial*.22f)continue;
                    float px=centerX+cos*a-sin*c,pz=centerZ+sin*a+cos*c;
                    if(!Contains(polygon,px,pz)||(exclude?.Invoke(px,pz)??false)||NearLine(px,pz,1))continue;
                    float size=J(.65f,.95f)*Mathf.Lerp(1,.68f,radial);
                    small.Add(PlantMatrix(tuft,px,pz,size,J(0,360))*Matrix4x4.Scale(new Vector3(2.1f,1,2.1f)));
                    if(radial<.72f&&rng.NextDouble()<.35)
                        fine.Add(PlantMatrix(tall,px,pz,J(.9f,1.4f),J(0,360))*Matrix4x4.Scale(new Vector3(1.3f,1,1.3f)));
                    if(flowering&&radial<.65f&&rng.NextDouble()<.48)
                    {
                        bool second=angle>Mathf.PI;var model=second?bloom2:bloom;
                        (second?flowers2:flowers).Add(PlantMatrix(model,px,pz,J(.58f,.88f),J(0,360)));
                    }
                }
            }
            var nature=parent.gameObject.AddComponent<InstancedNature>();
            var grassTint=new Color(.74f,.88f,.66f);
            nature.Add(tuft,small,grassTint,true);nature.Add(tall,fine,grassTint,true);
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
