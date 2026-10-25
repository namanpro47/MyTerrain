using System.Linq;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        // Broad illustrative grass stands give mapped grassland a lower vegetation
        // layer. Source cover, annual projects and the terrain surface stay authoritative.
        void BuildGrassland()
        {
            const float patchSize=60;
            var grassland=new GameObject("Mapped grassland stands");
            grassland.transform.SetParent(root,false);
            if(clusteredWoodland)
            {
                BuildGrasslandDrifts(grassland.transform);
                return;
            }
            for(float z=zmin;z<zmax;z+=patchSize)for(float x=xmin;x<xmax;x+=patchSize)
            {
                var patch=new GameObject("Grassland patch");patch.transform.SetParent(grassland.transform,false);
                PlantMeadowStands(Rectangle(x,z,Mathf.Min(x+patchSize,xmax),Mathf.Min(z+patchSize,zmax)),
                    patch.transform,false,(px,pz)=>CoverAt(px,pz)!="grass"||world.sites.Any(site=>Contains(site.polygon,px,pz)),3,1.4f);
            }
        }

        // Broad overlapping stands carry the middle-scale meadow silhouette.
        // This height and grouping are illustrative, not observed vegetation.
        // Annual treatment planting and the other regions retain their own layouts.
        void BuildGrasslandDrifts(Transform parent)
        {
            var source=Resources.Load<GameObject>("Nature/Grass_Common_Short");
            var rng=new System.Random(5402);
            float J(float low,float high)=>low+(high-low)*(float)rng.NextDouble();
            for(float cz=zmin;cz<zmax;cz+=60)for(float cx=xmin;cx<xmax;cx+=60)
            {
                var plants=new System.Collections.Generic.List<Matrix4x4>();
                for(float z=cz+1.25f;z<Mathf.Min(cz+60,zmax);z+=2.5f)
                for(float x=cx+1.25f;x<Mathf.Min(cx+60,xmax);x+=2.5f)
                {
                    float px=x+J(-1,1),pz=z+J(-1,1);
                    if(px<xmin||px>xmax||pz<zmin||pz>zmax||CoverAt(px,pz)!="grass"||NearLine(px,pz,6)
                        ||world.sites.Any(site=>Contains(site.polygon,px,pz)))continue;
                    float field=Mathf.PerlinNoise(px*.014f+184,pz*.014f+629)*.8f
                        +Mathf.PerlinNoise(px*.038f+71,pz*.038f+29)*.2f;
                    float growth=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.47f,.65f,field));
                    if(growth<.025f)continue;
                    float height=Mathf.Lerp(.55f,3.2f,growth)*J(.8f,1.2f);
                    float width=Mathf.Lerp(2.4f,5.2f,growth);
                    plants.Add(Matrix4x4.TRS(new Vector3(px,Height(px,pz)+.03f,pz),
                        Quaternion.Euler(0,J(0,360),0),new Vector3(width,height,width)));
                }
                if(plants.Count==0)continue;
                var patch=new GameObject("Grassland drift");patch.transform.SetParent(parent,false);
                patch.AddComponent<InstancedNature>().Add(source,plants,new Color(.82f,.94f,.72f),true);
            }
        }
    }
}
