using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        sealed class GroundChunk
        {
            public Mesh mesh;
            public Vector3[] original,normals,changed,changedNormals;
            public Bounds bounds;
            public bool deformed;
        }
        sealed class SwaleShape
        {
            public Site site;
            public Point[] route;
            public Bounds bounds;
            public float halfWidth;
            public Vector4 cutBounds;
        }
        readonly List<GroundChunk> groundChunks=new List<GroundChunk>();
        readonly Dictionary<string,SwaleShape> swaleShapes=new Dictionary<string,SwaleShape>();
        const float SwaleDepth=1.15f;

        SwaleShape SwaleFor(Site site)
        {
            if(swaleShapes.TryGetValue(site.id,out var cached))return cached;
            float l=site.polygon.Min(p=>p.x),r=site.polygon.Max(p=>p.x),b=site.polygon.Min(p=>p.z),t=site.polygon.Max(p=>p.z);
            bool alongX=r-l>=t-b;
            float length=alongX?r-l:t-b,width=alongX?t-b:r-l;
            var route=new Point[25];
            for(int i=0;i<route.Length;i++)
            {
                float u=(float)i/(route.Length-1),along=Mathf.Lerp(12,length-12,u);
                float across=width*.5f+Mathf.Sin(u*Mathf.PI*2-.4f)*Mathf.Min(7,width*.12f);
                route[i]=alongX?new Point(l+along,b+across):new Point(l+across,b+along);
            }
            var shape=new SwaleShape{site=site,route=route,halfWidth=Mathf.Min(7,width*.18f),bounds=new Bounds(new Vector3((l+r)*.5f,0,(b+t)*.5f),new Vector3(r-l,1000,t-b))};
            shape.cutBounds=new Vector4(route.Min(p=>p.x)-shape.halfWidth,route.Min(p=>p.z)-shape.halfWidth,route.Max(p=>p.x)+shape.halfWidth,route.Max(p=>p.z)+shape.halfWidth);
            swaleShapes.Add(site.id,shape);return shape;
        }
        float SwaleCut(SwaleShape shape,float x,float z)
        {
            // Most parcel/terrain samples lie outside the narrow basin. Reject those
            // before polygon and route queries, preserving the original cut footprint.
            var bounds=shape.cutBounds;
            if(x<bounds.x||x>bounds.z||z<bounds.y||z>bounds.w||!Contains(shape.site.polygon,x,z))return 0;
            float radiusSquared=shape.halfWidth*shape.halfWidth;
            float distanceSquared=SwaleDistanceSquared(shape,x,z,radiusSquared);
            // Squared distance gives the same rounded profile without a square root
            // for each of the route's segments (and another square afterwards).
            return SwaleDepth*Mathf.Pow(Mathf.Max(0,1-distanceSquared/radiusSquared),2);
        }
        static float SwaleDistanceSquared(SwaleShape shape,float x,float z,float limit=float.MaxValue)
        {
            var p=new Vector2(x,z);float nearest=limit;
            for(int i=1;i<shape.route.Length;i++)
            {
                var a=new Vector2(shape.route[i-1].x,shape.route[i-1].z);var b=new Vector2(shape.route[i].x,shape.route[i].z);
                float bx=Mathf.Max(Mathf.Max(Mathf.Min(a.x,b.x)-x,0),x-Mathf.Max(a.x,b.x));
                float bz=Mathf.Max(Mathf.Max(Mathf.Min(a.y,b.y)-z,0),z-Mathf.Max(a.y,b.y));
                if(bx*bx+bz*bz>=nearest)continue;
                var d=b-a;float t=d.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude):0;
                nearest=Mathf.Min(nearest,(p-(a+d*t)).sqrMagnitude);
            }
            return nearest;
        }
        void BuildGroundChunks(Vector3[] vertices,int columns,int rows,Material material)
        {
            const int cells=64;
            float dx=vertices[1].x-vertices[0].x,dz=vertices[columns].z-vertices[0].z;
            var normals=new Vector3[vertices.Length];
            for(int z=0;z<rows;z++)for(int x=0;x<columns;x++)
            {
                int xl=Mathf.Max(0,x-1),xr=Mathf.Min(columns-1,x+1),zb=Mathf.Max(0,z-1),zt=Mathf.Min(rows-1,z+1);
                normals[z*columns+x]=new Vector3((vertices[z*columns+xl].y-vertices[z*columns+xr].y)/((xr-xl)*dx),1,(vertices[zb*columns+x].y-vertices[zt*columns+x].y)/((zt-zb)*dz)).normalized;
            }
            for(int z0=0;z0<rows-1;z0+=cells)for(int x0=0;x0<columns-1;x0+=cells)
            {
                int w=Mathf.Min(cells,columns-1-x0)+1,h=Mathf.Min(cells,rows-1-z0)+1;
                var points=new Vector3[w*h];var ns=new Vector3[w*h];
                for(int z=0;z<h;z++)for(int x=0;x<w;x++){int source=(z0+z)*columns+x0+x;points[z*w+x]=vertices[source];ns[z*w+x]=normals[source];}
                var indices=new int[(w-1)*(h-1)*6];int n=0;
                for(int z=0;z<h-1;z++)for(int x=0;x<w-1;x++){int i=z*w+x;indices[n++]=i;indices[n++]=i+w;indices[n++]=i+1;indices[n++]=i+1;indices[n++]=i+w;indices[n++]=i+w+1;}
                var mesh=new Mesh{name="Continuous ground "+x0+" "+z0};mesh.MarkDynamic();mesh.vertices=points;mesh.normals=ns;mesh.triangles=indices;mesh.RecalculateBounds();meshes.Add(mesh);
                var go=new GameObject(mesh.name);go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
                groundChunks.Add(new GroundChunk{mesh=mesh,original=points,normals=ns,bounds=mesh.bounds});
            }
        }
        void FlushSwaleGround()
        {
            var active=new List<SwaleShape>();
            foreach(var site in world.sites)
            {
                string treatment;
                if(site.id==ghostSiteId&&ghostTreatment!=null)treatment=ghostTreatment;
                else if(!surfaceTreatments.TryGetValue(site.id,out treatment))continue;
                if(treatment=="water-swale")active.Add(SwaleFor(site));
            }
            foreach(var chunk in groundChunks)
            {
                var relevant=active.Where(s=>s.bounds.Intersects(chunk.bounds)).ToArray();
                if(relevant.Length==0)
                {
                    if(chunk.deformed){chunk.mesh.vertices=chunk.original;chunk.mesh.normals=chunk.normals;chunk.mesh.RecalculateBounds();chunk.deformed=false;}
                    continue;
                }
                if(chunk.changed==null){chunk.changed=new Vector3[chunk.original.Length];chunk.changedNormals=new Vector3[chunk.original.Length];}
                float Cut(float x,float z){float result=0;foreach(var s in relevant)result=Mathf.Max(result,SwaleCut(s,x,z));return result;}
                for(int i=0;i<chunk.original.Length;i++)
                {
                    var p=chunk.original[i];float cut=Cut(p.x,p.z);chunk.changed[i]=p-Vector3.up*cut;
                    var normal=chunk.normals[i];
                    float gx=(Cut(p.x+.75f,p.z)-Cut(p.x-.75f,p.z))/1.5f,gz=(Cut(p.x,p.z+.75f)-Cut(p.x,p.z-.75f))/1.5f;
                    chunk.changedNormals[i]=new Vector3(normal.x/normal.y+gx,1,normal.z/normal.y+gz).normalized;
                }
                chunk.mesh.vertices=chunk.changed;chunk.mesh.normals=chunk.changedNormals;chunk.mesh.RecalculateBounds();chunk.deformed=true;
            }
        }
        void PlantSwale(Site site,Transform parent)
        {
            var shape=SwaleFor(site);
            // A dry vegetated swale: no permanent ribbon of standing water is implied without rainfall state.
            PlantMeadow(site.polygon,parent,3.7f,true,(x,z)=>SwaleCut(shape,x,z)>.02f);
            var low=Resources.Load<GameObject>("Nature/Grass_Common_Short");
            var rush=Resources.Load<GameObject>("Nature/Grass_Common_Tall");
            var rock=Resources.Load<GameObject>("Nature/Rock_Medium_2");
            var lows=new List<Matrix4x4>();var rushes=new List<Matrix4x4>();var stones=new List<Matrix4x4>();
            int seed=418;foreach(char letter in site.id)seed=unchecked(seed*31+letter);
            var rng=new System.Random(seed);float J(float a,float b)=>a+(b-a)*(float)rng.NextDouble();
            var bounds=shape.bounds;
            for(float z=bounds.min.z+4;z<bounds.max.z-4;z+=1.5f)for(float x=bounds.min.x+4;x<bounds.max.x-4;x+=1.5f)
            {
                float px=x+J(-.65f,.65f),pz=z+J(-.65f,.65f),distance=Mathf.Sqrt(SwaleDistanceSquared(shape,px,pz));
                if(distance>shape.halfWidth+3||!Contains(site.polygon,px,pz))continue;
                float cut=SwaleCut(shape,px,pz);
                float cohort=Mathf.PerlinNoise(px*.11f+200,pz*.11f+200);
                if(distance>2.1f&&cohort>.35f)
                {
                    var m=PlantMatrix(low,px,pz,J(.38f,.62f),J(0,360))*Matrix4x4.Scale(new Vector3(2.2f,1,2.2f));m.m13-=cut;lows.Add(m);
                    if(distance<shape.halfWidth&&cohort>.59f&&rng.NextDouble()<.4){m=PlantMatrix(rush,px,pz,J(.35f,.55f),J(0,360));m.m13-=cut;rushes.Add(m);}
                }
                if(distance<2.5f&&cohort<.52f&&rng.NextDouble()<.22)
                {var m=PlantMatrix(rock,px,pz,J(.12f,.22f),J(0,360));m.m13-=cut+.15f;stones.Add(m);}
            }
            var plants=parent.gameObject.AddComponent<InstancedNature>();plants.Add(low,lows,Color.white,true);plants.Add(rush,rushes,Color.white);plants.Add(rock,stones,Color.white);
        }
    }
}
