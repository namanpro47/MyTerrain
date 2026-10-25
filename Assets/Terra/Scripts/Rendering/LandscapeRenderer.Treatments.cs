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
            =>PlantMeadowStands(polygon,parent,withFlowers,exclude,1,1);

        void PlantMeadowStands(Point[] polygon,Transform parent,bool withFlowers,Func<float,float,bool> exclude,float breadth,float height)
        {
            var tuft=Resources.Load<GameObject>("Nature/Grass_Common_Short");var tall=Resources.Load<GameObject>("Nature/Grass_Wispy_Short");
            var bloom=Resources.Load<GameObject>("Nature/Flower_3_Group");var bloom2=Resources.Load<GameObject>("Nature/Flower_4_Group");
            var small=new List<Matrix4x4>();var fine=new List<Matrix4x4>();var flowers=new List<Matrix4x4>();var flowers2=new List<Matrix4x4>();
            float l=polygon.Min(v=>v.x),r=polygon.Max(v=>v.x),b=polygon.Min(v=>v.z),t=polygon.Max(v=>v.z);
            var rng=new System.Random((int)(l*71+b*131)+418);float J(float min,float max)=>min+(float)rng.NextDouble()*(max-min);
            // Continuous sward with irregular openings and height drifts. Source meshes
            // remain unchanged; cover density and flowers are illustrative, not surveyed.
            float step=withFlowers?1.25f:2.2f;
            for(float z=b+step*.5f;z<t;z+=step)for(float x=l+step*.5f;x<r;x+=step)
            {
                float px=x+J(-step*.42f,step*.42f),pz=z+J(-step*.42f,step*.42f);
                if(!Contains(polygon,px,pz)||(exclude?.Invoke(px,pz)??false)||NearLine(px,pz,1))continue;
                float drift=Mathf.PerlinNoise(px*.035f+271,pz*.035f+129);
                float detail=Mathf.PerlinNoise(px*.11f+71,pz*.11f+229);
                float growth=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.28f,.60f,drift*.75f+detail*.25f));
                if(J(0,1)>.3f+growth*.7f)continue;
                float size=Mathf.Lerp(.38f,1.15f,growth)*J(.85f,1.15f);
                small.Add(PlantMatrix(tuft,px,pz,size*height,J(0,360))*Matrix4x4.Scale(new Vector3(1.8f*breadth/height,1,1.8f*breadth/height)));
                if(growth>.55f&&J(0,1)<.18f)
                    fine.Add(PlantMatrix(tall,px,pz,J(.7f,1.2f)*height,J(0,360))*Matrix4x4.Scale(new Vector3(breadth/height,1,breadth/height)));
                float blossom=Mathf.PerlinNoise(px*.065f+141,pz*.065f+551);
                if(withFlowers&&blossom>.63f&&J(0,1)<.16f)
                {
                    bool second=drift>.55f;var model=second?bloom2:bloom;
                    (second?flowers2:flowers).Add(PlantMatrix(model,px,pz,J(.4f,.65f),J(0,360)));
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
            // A shared curved planting coordinate keeps neighbouring source patches continuous.
            // These rows and access lanes are an illustration, not surveyed farm infrastructure.
            for(float z=Mathf.Ceil(b/1.1f)*1.1f;z<t;z+=1.1f)
            {
                float offset=FieldOffset(z);
                for(float u=Mathf.Ceil((l-offset)/spacing)*spacing;u<r-offset;u+=spacing)
                {
                    float x=u+offset;
                    if(!Contains(polygon,x,z)||(baseline&&!VisualCropAt(x,z))||FieldTrackAt(x,z))continue;
                    plants.Add(FieldPlant(grain,x,z,(float)rng.NextDouble()));
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
                for(float z=Mathf.Ceil(cz/1.1f)*1.1f;z<Mathf.Min(cz+150,zmax-3);z+=1.1f)
                {
                    float offset=FieldOffset(z);
                    for(float u=Mathf.Ceil((cx-offset)/1.35f)*1.35f;u<Mathf.Min(cx+150,xmax-3)-offset;u+=1.35f)
                    {
                        float x=u+offset;
                        if(!VisualCropAt(x,z)||FieldTrackAt(x,z)||NearLine(x,z,8)||world.sites.Any(site=>Contains(site.polygon,x,z)))continue;
                        float variation=Mathf.PerlinNoise(x*.12f+500,z*.12f+500);
                        plants.Add(FieldPlant(grain,x,z,variation));
                    }
                }

                root.gameObject.AddComponent<InstancedNature>().Add(grain,plants,Color.white);
            }
        }
        // Illustrative access lanes, aligned across baseline and proposed planting, not surveyed tracks.
        static float FieldOffset(float z)=>12*Mathf.Sin(z*.012f)+5*Mathf.Sin(z*.029f);
        static float FieldSlope(float z)=>.144f*Mathf.Cos(z*.012f)+.145f*Mathf.Cos(z*.029f);
        static bool FieldTrackAt(float x,float z)=>Mathf.Abs(Mathf.Repeat(x-FieldOffset(z)+19.2f,38.4f)-19.2f)<.65f;
        Matrix4x4 FieldPlant(GameObject grain,float x,float z,float variation)
        {
            float size=.62f+variation*.12f;
            float yaw=Mathf.Atan(FieldSlope(z))*Mathf.Rad2Deg;
            return PlantMatrix(grain,x,z,size,yaw)*Matrix4x4.Scale(new Vector3(2.0f,1,1.15f));
        }
    }
}
