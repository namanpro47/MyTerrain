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
            for(float z=zmin;z<zmax;z+=patchSize)for(float x=xmin;x<xmax;x+=patchSize)
            {
                var patch=new GameObject("Grassland patch");patch.transform.SetParent(grassland.transform,false);
                PlantMeadowStands(Rectangle(x,z,Mathf.Min(x+patchSize,xmax),Mathf.Min(z+patchSize,zmax)),
                    patch.transform,false,(px,pz)=>CoverAt(px,pz)!="grass"||world.sites.Any(site=>Contains(site.polygon,px,pz)),3,1.4f);
            }
        }
    }
}
