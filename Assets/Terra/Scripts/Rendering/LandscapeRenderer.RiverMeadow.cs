using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        // Authored riparian communities, not surveyed species or modelled bank ecology.
        // Larger swaths meet the water; open patches retain exposed soil and stone.
        void BuildLayeredRiverMeadow()
        {
            var low=Resources.Load<GameObject>("Nature/Grass_Common_Short");
            var rush=Resources.Load<GameObject>("Nature/Grass_Common_Tall");
            var flower=Resources.Load<GameObject>("Nature/Flower_4_Group");
            var stone=Resources.Load<GameObject>("Nature/Rock_Medium_2");
            var sources=new[]{low,rush,flower,stone};
            var tints=new[]{new Color(.82f,.94f,.72f),new Color(.78f,.92f,.70f),Color.white,Color.white};
            var tiles=new Dictionary<Vector2Int,List<Matrix4x4>[]>();
            var paths=world.features.Where(f=>f.kind=="path").ToArray();
            var rng=new System.Random(6001);
            float J(float a,float b)=>a+(b-a)*(float)rng.NextDouble();
            void Add(int layer,float x,float z,float height,float width,float sink=0)
            {
                var key=new Vector2Int(Mathf.FloorToInt(x/96),Mathf.FloorToInt(z/96));
                if(!tiles.TryGetValue(key,out var groups))
                {
                    groups=new[]{new List<Matrix4x4>(),new List<Matrix4x4>(),new List<Matrix4x4>(),new List<Matrix4x4>()};
                    tiles.Add(key,groups);
                }
                height=Mathf.Max(.08f,height);
                var matrix=PlantMatrix(sources[layer],x,z,height,J(0,360))
                    *Matrix4x4.Scale(new Vector3(width/height,1,width/height));
                // Sample the rendered channel triangles, including their actual grid interpolation.
                matrix.m13+=RiverMeadowGround(x,z)-Height(x,z)-sink;
                groups[layer].Add(matrix);
            }
            foreach(var feature in world.features.Where(f=>f.kind=="water"))
            {
                var route=RoundedWaterRoute(feature.points);
                var visited=new HashSet<Vector2Int>();
                float reach=feature.widthM*.5f+22;
                for(int i=1;i<route.Length;i++)
                {
                    float minx=Mathf.Min(route[i-1].x,route[i].x)-reach,maxx=Mathf.Max(route[i-1].x,route[i].x)+reach;
                    float minz=Mathf.Min(route[i-1].z,route[i].z)-reach,maxz=Mathf.Max(route[i-1].z,route[i].z)+reach;
                    for(int iz=Mathf.FloorToInt(minz/1.9f);iz<=maxz/1.9f;iz++)
                    for(int ix=Mathf.FloorToInt(minx/1.9f);ix<=maxx/1.9f;ix++)
                    {
                        if(!visited.Add(new Vector2Int(ix,iz)))continue;
                        float x=ix*1.9f+J(-.55f,.55f),z=iz*1.9f+J(-.55f,.55f);
                        if(!Inside(x,z)||paths.Any(p=>DistanceToRoute(p.points,x,z)<p.widthM*.5f+3))continue;
                        float distance=DistanceToRoute(route,x,z)-feature.widthM*.5f;
                        float drift=Mathf.PerlinNoise(x*.033f+184,z*.033f+629);
                        float growth=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.32f,.69f,drift));
                        float outer=7+15*growth;
                        if(distance<2.3f||distance>outer)continue;
                        float edge=Mathf.SmoothStep(0,1,Mathf.Clamp01((outer-distance)/3));
                        float height=Mathf.Lerp(.25f,2.3f,growth)*J(.85f,1.15f)*edge;
                        float width=Mathf.Lerp(2.3f,4.3f,growth)*Mathf.Lerp(.7f,1,edge);
                        Add(0,x,z,height,width);
                        if(distance<5.7f&&growth>.55f&&J(0,1)<.32f)
                            Add(1,x,z,J(1.7f,2.6f),J(1.7f,2.4f));
                        if(distance>5&&growth>.25f&&growth<.7f&&Mathf.PerlinNoise(x*.07f+433,z*.07f+112)>.60f&&J(0,1)<.38f)
                            Add(2,x,z,J(.8f,1.3f),1.2f);
                        if(distance<4.8f&&growth<.2f&&J(0,1)<.1f)
                            Add(3,x,z,J(.5f,.9f),J(1.4f,2.4f),.5f);
                    }
                }
            }
            foreach(var pair in tiles)
            {
                var patch=new GameObject("River meadow "+pair.Key.x+" "+pair.Key.y);
                patch.transform.SetParent(root,false);
                var plants=patch.AddComponent<InstancedNature>();
                for(int i=0;i<sources.Length;i++)plants.Add(sources[i],pair.Value[i],tints[i],i==0);
            }
        }

        float RiverMeadowGround(float x,float z)
        {
            // BuildChannelTerrain and the Wisconsin display cache use the same 1.5m grid.
            // Read immutable original vertices so annual swale previews cannot shift this baseline.
            float gx=Mathf.Clamp((x-xmin)/displayDx,0,displayColumns-1);
            float gz=Mathf.Clamp((z-zmin)/displayDz,0,displayRows-1);
            int ix=Mathf.Min(Mathf.FloorToInt(gx),displayColumns-2),iz=Mathf.Min(Mathf.FloorToInt(gz),displayRows-2);
            int cx=ix/64,cz=iz/64,chunksX=Mathf.CeilToInt((displayColumns-1)/64f);
            int width=Mathf.Min(64,displayColumns-1-cx*64)+1;
            var points=groundChunks[cz*chunksX+cx].original;
            int index=(iz-cz*64)*width+ix-cx*64;float u=gx-ix,v=gz-iz;
            float a=points[index].y,b=points[index+1].y,c=points[index+width].y,d=points[index+width+1].y;
            return u+v<=1?a+(b-a)*u+(c-a)*v:d+(c-d)*(1-u)+(b-d)*(1-v);
        }
    }
}
