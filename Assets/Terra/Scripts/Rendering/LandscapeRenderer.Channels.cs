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
            var low=Resources.Load<GameObject>("Nature/Grass_Common_Short");
            var rush=Resources.Load<GameObject>("Nature/Grass_Common_Tall");
            var rock=Resources.Load<GameObject>("Nature/Rock_Medium_2");
            // One coherent source family: a low connected sward, occasional taller stands, exposed bank.
            // Plant positions and bank microrelief are illustrative, never surveyed vegetation.
            var bankRandom=new System.Random(2181);
            float Jitter(float min,float max)=>min+(max-min)*(float)bankRandom.NextDouble();
            bool Clear(Vector2 p)=>Inside(p.x,p.y)&&!world.features.Any(f=>f.kind=="path"&&DistanceToRoute(f.points,p.x,p.y)<f.widthM*.5f+1.7f);
            foreach(var feature in world.features.Where(f=>f.kind=="water"))
            {
                var sward=new List<Matrix4x4>();var stands=new List<Matrix4x4>();var stones=new List<Matrix4x4>();
                var route=RoundedWaterRoute(feature.points);float next=1;
                for(int i=1;i<route.Length;i++)
                {
                    var a=new Vector2(route[i-1].x,route[i-1].z);var d=new Vector2(route[i].x,route[i].z)-a;
                    var tangent=d.normalized;var n=new Vector2(-tangent.y,tangent.x);
                    for(;next<d.magnitude;next+=1.4f)
                    foreach(float side in new[]{-1f,1f})
                    {
                        var center=a+tangent*next;
                        float growth=Mathf.PerlinNoise(center.x*.043f+500,center.y*.043f+500+side*12);
                        float width=12*Mathf.SmoothStep(0,1,Mathf.Clamp01((growth-.32f)/.32f));
                        for(float across=0;across<width;across+=1.3f)
                        {
                            var p=center+n*(feature.widthM*.5f+4.7f+across+Jitter(-.65f,.65f))*side+tangent*Jitter(-.7f,.7f);
                            if(!Clear(p))continue;
                            // Broad low fans overlap into readable planted mass, not a row of isolated tall dots.
                            float scale=Jitter(.85f,1.15f);
                            sward.Add(PlantMatrix(low,p.x,p.y,scale,Jitter(0,360))*Matrix4x4.Scale(new Vector3(2.1f,1.05f,2.1f)));
                            if(across>1.3f&&growth>.62f&&bankRandom.NextDouble()<.22)
                                stands.Add(PlantMatrix(rush,p.x,p.y,Jitter(.8f,1.3f),Jitter(0,360)));
                        }
                        if(growth<.34f&&bankRandom.NextDouble()<.14)
                        {
                            var shelf=center+n*(feature.widthM*.5f+2.8f)*side;
                            if(!Clear(shelf))continue;
                            for(int stoneIndex=0;stoneIndex<3;stoneIndex++)
                            {
                                var p=shelf+tangent*Jitter(-2,2)+n*Jitter(-.6f,.6f);
                                var matrix=PlantMatrix(rock,p.x,p.y,Jitter(.65f,1.25f),Jitter(0,360));
                                matrix.m13-=1.3f;stones.Add(matrix);
                            }
                        }
                    }
                    next-=d.magnitude;
                }
                var instances=root.gameObject.AddComponent<InstancedNature>();
                instances.Add(low,sward,new Color(.80f,.92f,.68f),true);
                instances.Add(rush,stands,new Color(.74f,.89f,.57f));
                instances.Add(rock,stones,Color.white);
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
                // Only the illustrated channel face gains small-scale variation; analytical Height() is unchanged.
                float edgeVariation=(Mathf.PerlinNoise(px*.13f+81,pz*.13f+93)-.5f)*1.1f;
                distance[i]+=edgeVariation;
                float cut=1-Mathf.SmoothStep(0,1,Mathf.Clamp01((distance[i]-1.6f)/2.5f));
                vertices[i]=new Vector3(px,Height(px,pz)-cut*2.0f,pz);
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
