using System;
using System.Linq;
using System.Collections.Generic;
using Terra.Contracts;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        // Round only the visible corner within a six-metre neighbourhood; source polylines are never changed.
        static Point[] RoundedWaterRoute(Point[] points)
        {
            if(points.Length<3)return points;
            var route=new List<Point>{points[0]};
            for(int i=1;i<points.Length-1;i++)
            {
                var p=new Vector2(points[i].x,points[i].z);
                var before=new Vector2(points[i-1].x,points[i-1].z)-p;
                var after=new Vector2(points[i+1].x,points[i+1].z)-p;
                float radius=Mathf.Min(6,before.magnitude*.25f,after.magnitude*.25f);
                var a=p+before.normalized*radius;var b=p+after.normalized*radius;
                for(int j=0;j<=4;j++){float t=j*.25f;var q=(1-t)*(1-t)*a+2*(1-t)*t*p+t*t*b;route.Add(new Point(q.x,q.y));}
            }
            route.Add(points[points.Length-1]);return route.ToArray();
        }
        void BuildStreamsidePlants()
        {
            var rush=Resources.Load<GameObject>("Nature/Grass_Common_Tall");var rock=Resources.Load<GameObject>("Nature/Rock_Medium_2");
            foreach(var feature in world.features.Where(f=>f.kind=="water"))
            {
                var grasses=new List<Matrix4x4>();var stones=new List<Matrix4x4>();var route=RoundedWaterRoute(feature.points);float next=3;
                for(int i=1;i<route.Length;i++)
                {
                    var a=new Vector2(route[i-1].x,route[i-1].z);var d=new Vector2(route[i].x,route[i].z)-a;var n=new Vector2(-d.y,d.x).normalized;
                    for(;next<d.magnitude;next+=3)
                    foreach(float side in new[]{-1f,1f})
                    {
                        var center=a+d.normalized*next;var p=center+n*(feature.widthM*.5f+4+Range(0,1.5f))*side;
                        if(!Inside(p.x,p.y)||world.features.Any(f=>f.kind=="path"&&DistanceToRoute(f.points,p.x,p.y)<f.widthM*.5f+2))continue;
                        float cluster=Mathf.PerlinNoise(p.x*.07f+500,p.y*.07f+500);
                        if(cluster>.42f)grasses.Add(PlantMatrix(rush,p.x,p.y,Range(.9f,1.6f),Range(0,360)));
                        if(cluster>.58f&&random.NextDouble()<.45)
                        {
                            var shelf=center+n*(feature.widthM*.5f+2.4f)*side;
                            var stone=PlantMatrix(rock,shelf.x,shelf.y,Range(.7f,1.3f),Range(0,360));
                            stone.m13-=2.65f;stones.Add(stone);
                        }
                    }
                    next-=d.magnitude;
                }
                var instances=root.gameObject.AddComponent<InstancedNature>();instances.Add(rush,grasses,Color.white);instances.Add(rock,stones,Color.white);
            }
        }
        // A visual channel profile, not elevation data or a hydrological model. Height() and analytical data remain unchanged.
        void BuildChannelTerrain(Material material)
        {
            const float spacing=1.5f;
            int columns=Mathf.CeilToInt((xmax-xmin)/spacing)+1,rows=Mathf.CeilToInt((zmax-zmin)/spacing)+1;
            float dx=(xmax-xmin)/(columns-1),dz=(zmax-zmin)/(rows-1);
            var distance=new float[columns*rows];Array.Fill(distance,100);
            foreach(var feature in world.features.Where(f=>f.kind=="water"))
            {
            var route=RoundedWaterRoute(feature.points);
            for(int i=1;i<route.Length;i++)
            {
                var a=new Vector2(route[i-1].x,route[i-1].z);var b=new Vector2(route[i].x,route[i].z);var segment=b-a;
                int x0=Mathf.Max(0,Mathf.FloorToInt((Mathf.Min(a.x,b.x)-22-xmin)/dx)),x1=Mathf.Min(columns-1,Mathf.CeilToInt((Mathf.Max(a.x,b.x)+22-xmin)/dx));
                int z0=Mathf.Max(0,Mathf.FloorToInt((Mathf.Min(a.y,b.y)-22-zmin)/dz)),z1=Mathf.Min(rows-1,Mathf.CeilToInt((Mathf.Max(a.y,b.y)+22-zmin)/dz));
                for(int z=z0;z<=z1;z++)for(int x=x0;x<=x1;x++)
                {
                    var p=new Vector2(xmin+x*dx,zmin+z*dz);
                    float t=segment.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(p-a,segment)/segment.sqrMagnitude):0;
                    int index=z*columns+x;distance[index]=Mathf.Min(distance[index],Vector2.Distance(p,a+segment*t)-feature.widthM*.5f);
                }
            }
            }
            var vertices=new Vector3[columns*rows];var colors=new Color32[vertices.Length];
            for(int z=0;z<rows;z++)for(int x=0;x<columns;x++)
            {
                int i=z*columns+x;float px=xmin+x*dx,pz=zmin+z*dz;
                float cut=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((distance[i]-2.0f)/2.4f));
                vertices[i]=new Vector3(px,Height(px,pz)-cut*3.8f,pz);
                byte value=(byte)(Mathf.Clamp01((distance[i]+3)/20)*255);colors[i]=new Color32(value,value,value,255);
            }
            riverMask=new Texture2D(columns,rows,TextureFormat.RGBA32,true,true){name="Illustrative channel distance",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
            riverMask.SetPixels32(colors);riverMask.Apply();material.SetTexture("_RiverMap",riverMask);
            var indices=new int[(columns-1)*(rows-1)*6];int n=0;
            for(int z=0;z<rows-1;z++)for(int x=0;x<columns-1;x++)
            {int i=z*columns+x;indices[n++]=i;indices[n++]=i+columns;indices[n++]=i+1;indices[n++]=i+1;indices[n++]=i+columns;indices[n++]=i+columns+1;}
            var mesh=new Mesh{name="Source-height landscape with illustrative channels",indexFormat=IndexFormat.UInt32};
            mesh.vertices=vertices;mesh.triangles=indices;mesh.RecalculateNormals();mesh.RecalculateBounds();meshes.Add(mesh);
            var ground=new GameObject("Continuous landscape and inset stream banks");ground.transform.SetParent(root,false);
            ground.AddComponent<MeshFilter>().sharedMesh=mesh;ground.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        Material StreamMaterial(bool natural=true)
        {
            if(!natural)
            {
                if(swaleMaterial==null)swaleMaterial=new Material(Resources.Load<Shader>("TerraWater"));
                return swaleMaterial;
            }
            if(streamMaterial==null)
            {
                streamMaterial=new Material(Resources.Load<Shader>("TerraWater"));streamMaterial.SetFloat("_NaturalBanks",1);
                streamMaterial.SetTexture("_RiverMap",riverMask);streamMaterial.SetVector("_LandBounds",new Vector4(xmin,zmin,xmax-xmin,zmax-zmin));
            }
            return streamMaterial;
        }
    }
}
