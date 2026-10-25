using System.Collections.Generic;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        // Decorative canopy footprints affect presentation only, never source cover or annual accounting.
        readonly List<Vector3> forestFootprints=new List<Vector3>();
        Texture2D forestFloor;
        void BuildForestFloor()
        {
            const int size=512;
            var colors=new Color[size*size];
            float width=xmax-xmin,depth=zmax-zmin;
            foreach(var tree in forestFootprints)
            {
                float radius=tree.z;
                int x0=Mathf.Max(0,Mathf.FloorToInt((tree.x-radius-xmin)/width*size));
                int x1=Mathf.Min(size-1,Mathf.CeilToInt((tree.x+radius-xmin)/width*size));
                int z0=Mathf.Max(0,Mathf.FloorToInt((tree.y-radius-zmin)/depth*size));
                int z1=Mathf.Min(size-1,Mathf.CeilToInt((tree.y+radius-zmin)/depth*size));
                for(int z=z0;z<=z1;z++)for(int x=x0;x<=x1;x++)
                {
                    var p=new Vector2(xmin+(x+.5f)/size*width,zmin+(z+.5f)/size*depth);
                    float distance=Vector2.Distance(p,new Vector2(tree.x,tree.y))/radius;
                    float weight=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.35f,1,distance));
                    colors[z*size+x].r=Mathf.Max(colors[z*size+x].r,weight);
                }
            }
            forestFloor=new Texture2D(size,size,TextureFormat.RGBA32,true,true)
                {name="Illustrative canopy footprint",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
            forestFloor.SetPixels(colors);forestFloor.Apply();
            GroundMat(Color.white).SetTexture("_CanopyMap",forestFloor);
        }
    }
}
