using System.Collections.Generic;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        // Decorative canopy footprints affect presentation only, never source cover or annual accounting.
        readonly List<Vector3> forestFootprints=new List<Vector3>();
        Texture2D forestFloor;
        void BuildWoodlandCohorts()
        {
            meshCanopy=new GameObject("Layered woodland cohorts");meshCanopy.transform.SetParent(root,false);
            var tree=Resources.Load<GameObject>("Forest/ForestOak");
            var young=Resources.Load<GameObject>("Forest/ForestSapling");
            var rng=new System.Random(3707);float R(float a,float b)=>a+(float)rng.NextDouble()*(b-a);
            var matureTiles=new Dictionary<Vector2Int,List<Matrix4x4>>();
            var youngTiles=new Dictionary<Vector2Int,List<Matrix4x4>>();
            void Add(Dictionary<Vector2Int,List<Matrix4x4>> tiles,float x,float z,float scale,float breadth)
            {
                var key=new Vector2Int(Mathf.FloorToInt((x-xmin)/126),Mathf.FloorToInt((z-zmin)/126));
                if(!tiles.TryGetValue(key,out var list)){list=new List<Matrix4x4>();tiles.Add(key,list);}
                list.Add(Matrix4x4.TRS(new Vector3(x,Height(x,z),z),Quaternion.Euler(0,R(0,360),0),new Vector3(scale*breadth,scale,scale)));
            }
            // Illustrative age/height hierarchy: taller centers, younger edges, open glades.
            // Source woodland polygons stay authoritative; these are not surveyed stands.
            for(float cz=zmin+20;cz<zmax-20;cz+=42)for(float cx=xmin+20;cx<xmax-20;cx+=42)
            {
                float ax=cx+R(-12,12),az=cz+R(-12,12);
                if(CoverAt(ax,az)!="woodland"||R(0,1)<.12f)continue;
                var points=new List<Vector2>();float radius=R(14,22),rotation=R(0,Mathf.PI*2);
                int desired=rng.Next(7,12);
                for(int k=0;k<desired*8&&points.Count<desired;k++)
                {
                    float a=R(0,Mathf.PI*2),r=Mathf.Sqrt(R(0,1));
                    float u=Mathf.Cos(a)*radius*r,v=Mathf.Sin(a)*radius*.75f*r;
                    float x=ax+Mathf.Cos(rotation)*u-Mathf.Sin(rotation)*v;
                    float z=az+Mathf.Sin(rotation)*u+Mathf.Cos(rotation)*v;var p=new Vector2(x,z);
                    if(!Inside(x,z)||CoverAt(x,z)!="woodland"||NearLine(x,z,8)||points.Exists(q=>Vector2.Distance(p,q)<8))continue;
                    float scale=Mathf.Lerp(1.35f,.78f,r)*R(.9f,1.1f);points.Add(p);
                    Add(matureTiles,x,z,scale,R(.9f,1.1f));
                }
                if(points.Count==0)continue;
                forestFootprints.Add(new Vector3(ax,az,radius+6));
                for(int k=0;k<18;k++)
                {
                    float a=rotation+k*Mathf.PI*2/18+R(-.15f,.15f),r=radius*R(.82f,1.25f);
                    float x=ax+Mathf.Cos(a)*r,z=az+Mathf.Sin(a)*r*.8f;
                    if(!Inside(x,z)||CoverAt(x,z)!="woodland"||NearLine(x,z,4))continue;
                    Add(youngTiles,x,z,R(2.8f,4.8f),1);
                }
            }
            // Batch by spatial tile, not by ecological group; the visual composition is
            // independent of draw-call granularity. Artist geometry/materials stay shared.
            foreach(var pair in matureTiles)
            {
                var patch=new GameObject("Woodland tile");patch.transform.SetParent(meshCanopy.transform,false);
                var nature=patch.AddComponent<InstancedNature>();nature.Add(tree,pair.Value,Color.white);
                if(youngTiles.TryGetValue(pair.Key,out var saplings)){nature.Add(young,saplings,Color.white);youngTiles.Remove(pair.Key);}
            }
            foreach(var pair in youngTiles)
            {
                var patch=new GameObject("Woodland edge tile");patch.transform.SetParent(meshCanopy.transform,false);
                patch.AddComponent<InstancedNature>().Add(young,pair.Value,Color.white);
            }
        }
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
