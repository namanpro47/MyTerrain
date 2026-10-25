using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        // One union of mapped route footprints avoids overlapping translucent road strips at junctions.
        // This is a decorative finish; source routes, widths and land-use queries are unchanged.
        void BuildPathMask(Material ground)
        {
            const int size=1024;
            float dx=(xmax-xmin)/(size-1),dz=(zmax-zmin)/(size-1);
            var distance=new float[size*size];System.Array.Fill(distance,4);
            foreach(var route in world.features.Where(f=>f.kind=="path"))
            for(int i=1;i<route.points.Length;i++)
            {
                var a=new Vector2(route.points[i-1].x,route.points[i-1].z);
                var b=new Vector2(route.points[i].x,route.points[i].z);var segment=b-a;
                float radius=route.widthM*.5f+3;
                int x0=Mathf.Max(0,Mathf.FloorToInt((Mathf.Min(a.x,b.x)-radius-xmin)/dx));
                int x1=Mathf.Min(size-1,Mathf.CeilToInt((Mathf.Max(a.x,b.x)+radius-xmin)/dx));
                int z0=Mathf.Max(0,Mathf.FloorToInt((Mathf.Min(a.y,b.y)-radius-zmin)/dz));
                int z1=Mathf.Min(size-1,Mathf.CeilToInt((Mathf.Max(a.y,b.y)+radius-zmin)/dz));
                for(int z=z0;z<=z1;z++)for(int x=x0;x<=x1;x++)
                {
                    var p=new Vector2(xmin+x*dx,zmin+z*dz);
                    float t=segment.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(p-a,segment)/segment.sqrMagnitude):0;
                    int index=z*size+x;
                    distance[index]=Mathf.Min(distance[index],Vector2.Distance(p,a+segment*t)-route.widthM*.5f);
                }
            }
            var pixels=new Color32[distance.Length];
            for(int i=0;i<pixels.Length;i++){byte value=(byte)(Mathf.Clamp01((distance[i]+2)/4)*255);pixels[i]=new Color32(value,value,value,255);}
            pathMask=new Texture2D(size,size,TextureFormat.RGBA32,true,true){name="Mapped route union",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Trilinear};
            pathMask.SetPixels32(pixels);pathMask.Apply();ground.SetTexture("_PathMap",pathMask);
        }
        // Crossings are illustrative construction at mapped route intersections, not surveyed bridge designs.
        void BuildCrossings()
        {
            var built=new List<Vector2>();
            foreach(var road in world.features.Where(f=>f.kind=="path"))
            foreach(var water in world.features.Where(f=>f.kind=="water"))
            for(int i=1;i<road.points.Length;i++)
            for(int j=1;j<water.points.Length;j++)
            {
                var a=new Vector2(road.points[i-1].x,road.points[i-1].z);
                var u=new Vector2(road.points[i].x,road.points[i].z)-a;
                var b=new Vector2(water.points[j-1].x,water.points[j-1].z);
                var v=new Vector2(water.points[j].x,water.points[j].z)-b;
                float cross=u.x*v.y-u.y*v.x;
                if(Mathf.Abs(cross)<.001f)continue;
                var delta=b-a;
                float t=(delta.x*v.y-delta.y*v.x)/cross,s=(delta.x*u.y-delta.y*u.x)/cross;
                if(t<0||t>1||s<0||s>1)continue;
                var center=a+u*t;
                if(built.Any(p=>Vector2.Distance(p,center)<20))continue;
                built.Add(center);
                var forward=u.normalized;var side=new Vector2(-forward.y,forward.x);
                float angle=Mathf.Max(.35f,Mathf.Abs(forward.x*v.normalized.y-forward.y*v.normalized.x));
                float length=(water.widthM+11)/angle,half=road.widthM*.5f+.65f;
                var approach=center-forward*length*.5f;var exit=center+forward*length*.5f;
                BuildPath(new[]{new Point(approach.x,approach.y),new Point(exit.x,exit.y)},road.widthM);
                var masonry=new Geometry();var rails=new Geometry();
                // Follow the mapped road's grade; deck walls give the route real thickness over the cut channel.
                System.Func<float,float,float,Vector3> point=(along,across,lift)=>{
                    var p=center+forward*along+side*across;return new Vector3(p.x,Height(p.x,p.y)+lift,p.y);};
                for(int edge=-1;edge<=1;edge+=2)
                {
                    if(edge<0)
                        Wall(masonry,point(-length*.5f,edge*half,.26f),point(length*.5f,edge*half,.26f),point(length*.5f,edge*half,-.65f),point(-length*.5f,edge*half,-.65f));
                    else
                        Wall(masonry,point(length*.5f,edge*half,.26f),point(-length*.5f,edge*half,.26f),point(-length*.5f,edge*half,-.65f),point(length*.5f,edge*half,-.65f));
                    Wall(masonry,point(-length*.5f,edge*half,-.65f),point(length*.5f,edge*half,-.65f),point(length*.5f,edge*(half-.4f),-.65f),point(-length*.5f,edge*(half-.4f),-.65f));
                    for(float along=-length*.5f;along<=length*.5f;along+=3)
                        CrossingBeam(rails,point(along,edge*half,.25f),point(along,edge*half,1.65f),.24f);
                    CrossingBeam(rails,point(-length*.5f,edge*half,1.25f),point(length*.5f,edge*half,1.25f),.22f);
                    CrossingBeam(rails,point(-length*.5f,edge*half,.65f),point(length*.5f,edge*half,.65f),.16f);
                }
                // Abutments close each approach and frame the opening beneath the road.
                foreach(float end in new[]{-1f,1f})
                    CrossingBeam(masonry,point(end*length*.5f,-half,-.7f),point(end*length*.5f,half,-.7f),1.6f);
                MeshObject("Illustrative crossing · stone deck and abutments",masonry,Mat(new Color(.48f,.45f,.37f)),root);
                MeshObject("Illustrative crossing · timber rails",rails,Mat(new Color(.32f,.25f,.16f)),root);
            }
        }
        static void CrossingBeam(Geometry geometry,Vector3 a,Vector3 b,float width)
        {
            var direction=(b-a).normalized;
            var side=Vector3.Cross(direction,Mathf.Abs(direction.y)>.9f?Vector3.forward:Vector3.up).normalized*width*.5f;
            var up=Vector3.Cross(direction,side).normalized*width*.5f;
            var corners=new[]{side+up,side-up,-side-up,-side+up};
            for(int i=0;i<4;i++)Wall(geometry,a+corners[i],b+corners[i],b+corners[(i+1)%4],a+corners[(i+1)%4]);
            Wall(geometry,a+corners[3],a+corners[2],a+corners[1],a+corners[0]);
            Wall(geometry,b+corners[0],b+corners[1],b+corners[2],b+corners[3]);
        }
        // A gravel-like illustrative finish on the mapped route, not a surveyed road material.
        void BuildPath(Point[] source,float width)
        {
            var points=new List<Vector2>();
            for(int i=1;i<source.Length;i++)
            {
                var a=new Vector2(source[i-1].x,source[i-1].z);var b=new Vector2(source[i].x,source[i].z);
                int steps=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(a,b)));
                for(int j=0;j<steps;j++)points.Add(Vector2.Lerp(a,b,j/(float)steps));
            }
            if(source.Length==0)return;
            points.Add(new Vector2(source[source.Length-1].x,source[source.Length-1].z));
            int across=Mathf.Max(3,Mathf.CeilToInt(width+1.6f)+1);
            var vertices=new Vector3[points.Count*across];var normals=new Vector3[vertices.Length];var uv=new Vector2[vertices.Length];var indices=new int[(points.Count-1)*(across-1)*6];
            float along=0;
            for(int i=0;i<points.Count;i++)
            {
                var before=(points[i]-points[Mathf.Max(0,i-1)]).normalized;var after=(points[Mathf.Min(points.Count-1,i+1)]-points[i]).normalized;
                if(i==0)before=after;if(i==points.Count-1)after=before;
                var n1=new Vector2(-before.y,before.x);var n2=new Vector2(-after.y,after.x);var n=(n1+n2).normalized;
                float half=(width*.5f+.8f)/Mathf.Max(.7f,Vector2.Dot(n,n2));
                if(i>0)along+=Vector2.Distance(points[i],points[i-1]);
                for(int side=0;side<across;side++)
                {
                    float cross=Mathf.Lerp(-1,1,side/(float)(across-1));
                    var p=points[i]+n*half*cross;int v=i*across+side;
                    vertices[v]=new Vector3(p.x,Height(p.x,p.y)+.20f,p.y);normals[v]=Normal(p.x,p.y);uv[v]=new Vector2(cross,along);
                }
                if(i>0)for(int side=0;side<across-1;side++)
                {int k=((i-1)*(across-1)+side)*6,v=(i-1)*across+side;indices[k]=v;indices[k+1]=v+1;indices[k+2]=v+across;indices[k+3]=v+1;indices[k+4]=v+across+1;indices[k+5]=v+across;}
            }
            var mesh=new Mesh{name="Mapped path with soft surface edges",indexFormat=IndexFormat.UInt32};mesh.vertices=vertices;mesh.normals=normals;mesh.uv=uv;mesh.triangles=indices;mesh.RecalculateBounds();meshes.Add(mesh);
            if(pathMaterial==null){pathMaterial=new Material(Resources.Load<Shader>("TerraPath"));pathMaterial.SetTexture("_BankMap",Resources.Load<Texture2D>("ArtistDirtGround"));}
            var go=new GameObject("Mapped route · illustrative surface");go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=pathMaterial;
        }
    }
}
