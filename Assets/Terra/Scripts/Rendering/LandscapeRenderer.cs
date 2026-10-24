using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;
using UnityEngine;
using UnityEngine.Rendering;
using Object=UnityEngine.Object;
namespace Terra.Rendering
{
    // Source geometry remains in metres. Only vegetation, row marks and line widths are decorative.
    public sealed class LandscapeRenderer : IDisposable
    {
        readonly WorldData world; readonly Material template; readonly Transform root;
        readonly GameObject tree,crop,grass; readonly System.Random random=new System.Random(418);
        readonly Dictionary<string,Transform> proposals=new Dictionary<string,Transform>();
        readonly Dictionary<string,Transform> baselines=new Dictionary<string,Transform>();
        readonly Dictionary<string,LineRenderer> outlines=new Dictionary<string,LineRenderer>();
        readonly Dictionary<string,LineRenderer> outlineBackings=new Dictionary<string,LineRenderer>();
        readonly Dictionary<Color,Material> boundaryMaterials=new Dictionary<Color,Material>();
        string activeOutline,chosenOutline;float boundaryScale=2;
        readonly Dictionary<Color,Material> materials=new Dictionary<Color,Material>();
        readonly Dictionary<Color,Material> groundMaterials=new Dictionary<Color,Material>();
        readonly List<Mesh> meshes=new List<Mesh>();
        readonly List<CoverPatch>[,] coverIndex; readonly float xmin,zmin,xmax,zmax,cell;
        Transform ghost;Material glintMaterial;LineRenderer settlingOutline;float settleStarted;
        static readonly Color Wood=new Color(.20f,.35f,.24f),Field=new Color(.64f,.60f,.34f),Meadow=new Color(.46f,.59f,.34f),Water=new Color(.22f,.48f,.51f);
        sealed class Geometry
        {public readonly List<Vector3> vertices=new List<Vector3>();public readonly List<Vector3> normals=new List<Vector3>();public readonly List<int> triangles=new List<int>();}
        public LandscapeRenderer(WorldData w,Material material,Transform parent,GameObject tree,GameObject crop,GameObject grass,GameObject shrub)
        {
            world=w;template=material;root=parent;this.tree=tree;this.crop=crop;this.grass=grass;
            var t=w.terrain;cell=t.cellSizeM;xmin=t.xMin;zmin=t.zMin;xmax=xmin+(t.columns-1)*cell;zmax=zmin+(t.rows-1)*cell;
            coverIndex=new List<CoverPatch>[t.columns-1,t.rows-1];
            foreach(var c in w.covers)
            {
                int x0=IndexX(c.polygon.Min(p=>p.x)),x1=IndexX(c.polygon.Max(p=>p.x)),z0=IndexZ(c.polygon.Min(p=>p.z)),z1=IndexZ(c.polygon.Max(p=>p.z));
                for(int z=z0;z<=z1;z++)for(int x=x0;x<=x1;x++){if(coverIndex[x,z]==null)coverIndex[x,z]=new List<CoverPatch>();coverIndex[x,z].Add(c);}
            }
        }
        int IndexX(float x)=>Mathf.Clamp((int)((x-xmin)/cell),0,world.terrain.columns-2);
        int IndexZ(float z)=>Mathf.Clamp((int)((z-zmin)/cell),0,world.terrain.rows-2);
        public float Height(float x,float z)
        {
            var t=world.terrain;float fx=Mathf.Clamp((x-xmin)/cell,0,t.columns-1),fz=Mathf.Clamp((z-zmin)/cell,0,t.rows-1);
            int ix=Mathf.Min((int)fx,t.columns-2),iz=Mathf.Min((int)fz,t.rows-2),n=iz*t.columns+ix;float u=fx-ix,v=fz-iz;
            float a=t.heights[n],b=t.heights[n+1],c=t.heights[n+t.columns],d=t.heights[n+t.columns+1];
            // Same diagonal as exported source validation and every clipped render triangle.
            return u+v<=1?a+(b-a)*u+(c-a)*v:d+(c-d)*(1-u)+(b-d)*(1-v);
        }
        Vector3 Normal(float x,float z)=>new Vector3(Height(x-1,z)-Height(x+1,z),2,Height(x,z-1)-Height(x,z+1)).normalized;
        public string CoverAt(float x,float z)
        {var candidates=coverIndex[IndexX(x),IndexZ(z)];return candidates?.FirstOrDefault(c=>Contains(c.polygon,x,z))?.coverId??"grass";}
        bool Inside(float x,float z)=>x>=xmin&&x<=xmax&&z>=zmin&&z<=zmax;
        Material Mat(Color color)
        {if(!materials.TryGetValue(color,out var m)){m=new Material(template){color=color};if(m.HasProperty("_Smoothness"))m.SetFloat("_Smoothness",.08f);materials.Add(color,m);}return m;}
        Material GroundMat(Color color)
        {if(!groundMaterials.TryGetValue(color,out var material)){material=new Material(Resources.Load<Shader>("TerraGround"));material.SetColor("_BaseColor",color);groundMaterials.Add(color,material);}return material;}
        static Color CoverColor(string id)=>id=="woodland"?Wood:id=="cultivated"?Field:id=="water"?Water:id=="sealed"?new Color(.62f,.58f,.46f):Meadow;
        public void Build()
        {
            var timer=System.Diagnostics.Stopwatch.StartNew();var ground=new Dictionary<Color,Geometry>();
            int index=0;
            foreach(var c in world.covers)
            {
                // Subtle cell variation is decorative; cell boundaries and classes stay source-defined.
                Color color=CoverColor(c.coverId)*(.97f+.015f*(index++%5));color.a=1;
                if(!ground.TryGetValue(color,out var geometry)){geometry=new Geometry();ground[color]=geometry;}
                Surface(c.polygon,0,geometry);
            }
            foreach(var pair in ground)MeshObject("Mapped land cover",pair.Value,GroundMat(pair.Key),root);
            BuildEdge();
            foreach(var f in world.features.Where(f=>f.kind=="water"))
            {Ribbon(f.points,f.widthM+5,new Color(.39f,.49f,.32f),.035f,root,"Illustrative stream bank");Ribbon(f.points,f.widthM,Water,.12f,root,"Mapped stream centreline");}
            foreach(var f in world.features.Where(f=>f.kind=="path"))
            {Ribbon(f.points,f.widthM+2,new Color(.56f,.53f,.41f),.17f,root,"Road verge");Ribbon(f.points,f.widthM,new Color(.76f,.72f,.59f),.24f,root,"Mapped road centreline");}
            BuildVegetation();
            foreach(var site in world.sites)
            {
                var holder=new GameObject("Baseline "+site.name).transform;holder.SetParent(root,false);baselines[site.id]=holder;
                if(!site.protectedSite)BaselineRows(site,holder);
                outlineBackings[site.id]=Outline(site.polygon,new Color(.10f,.19f,.15f),2.4f,.30f,root);
                outlines[site.id]=Outline(site.polygon,new Color(.65f,.73f,.50f),1.1f,.36f,root);
            }
            var sparkles=new Geometry();int count=0;
            foreach(var f in world.features.Where(f=>f.kind=="water"))foreach(var p in f.points)
            {
                if(count++%17!=0)continue;
                Surface(Rectangle(p.x-1.2f,p.z-.35f,p.x+1.2f,p.z+.35f),.145f,sparkles);
            }
            glintMaterial=Mat(new Color(.66f,.79f,.73f));MeshObject("Decorative water glints",sparkles,glintMaterial,root);
            Debug.Log($"TERRA_SCENERY: buildMs={timer.Elapsed.TotalMilliseconds:0}; meshes={meshes.Count}; vertices={meshes.Sum(m=>m.vertexCount)}; materials={materials.Count}");
        }
        void BuildEdge()
        {
            var soil=new Geometry();var strata=new Geometry();
            var corners=new[]{new Vector2(xmin,zmin),new Vector2(xmax,zmin),new Vector2(xmax,zmax),new Vector2(xmin,zmax)};
            for(int side=0;side<4;side++)
            {
                Vector2 a=corners[side],b=corners[(side+1)%4];int steps=Mathf.RoundToInt(Vector2.Distance(a,b)/cell);
                for(int i=0;i<steps;i++)
                {
                    var p=Vector2.Lerp(a,b,(float)i/steps);var q=Vector2.Lerp(a,b,(float)(i+1)/steps);
                    float ph=Height(p.x,p.y),qh=Height(q.x,q.y);
                    Wall(soil,new Vector3(p.x,ph,p.y),new Vector3(q.x,qh,q.y),new Vector3(q.x,qh-3,q.y),new Vector3(p.x,ph-3,p.y));
                    Wall(strata,new Vector3(p.x,ph-3,p.y),new Vector3(q.x,qh-3,q.y),new Vector3(q.x,-24,q.y),new Vector3(p.x,-24,p.y));
                }
            }
            MeshObject("Topsoil edge",soil,Mat(new Color(.50f,.37f,.23f)),root);MeshObject("Earth foundation",strata,Mat(new Color(.36f,.28f,.20f)),root);
        }
        static void Wall(Geometry g,Vector3 a,Vector3 b,Vector3 c,Vector3 d)
        {
            int i=g.vertices.Count;g.vertices.AddRange(new[]{a,b,c,d});var n=Vector3.Cross(b-a,c-a).normalized;g.normals.AddRange(new[]{n,n,n,n});g.triangles.AddRange(new[]{i,i+1,i+2,i,i+2,i+3});
        }
        void BuildVegetation()
        {
            var groves=new[]{new List<Matrix4x4>(),new List<Matrix4x4>(),new List<Matrix4x4>()};var fieldMarks=new Geometry();
            for(float z=zmin+12;z<zmax-8;z+=15)for(float x=xmin+12;x<xmax-8;x+=15)
            {
                float px=x+Range(-6,6),pz=z+Range(-6,6);
                if(CoverAt(px,pz)!="woodland"||NearLine(px,pz,6))continue;
                // Broad canopy groups, with occasional small clearings. Decoration only;
                // neither the source cover geometry nor the analytical land state changes.
                float group=Mathf.PerlinNoise((px+1500)*.012f,(pz+1500)*.012f);
                if(group<.28f&&random.NextDouble()<.55)continue;
                int tone=group<.43f?0:group<.59f?1:2;
                groves[tone].Add(PlantMatrix(tree,px,pz,Range(22,30)*(group<.32f?.8f:1),Range(0,360)));
            }
            var tones=new[]{new Color(.24f,.40f,.29f),new Color(.32f,.47f,.29f),new Color(.38f,.51f,.30f)};
            for(int i=0;i<groves.Length;i++)BatchPrefab("Decorative canopy group",tree,groves[i],root,tones[i]);
            for(float z=zmin+6;z<zmax-6;z+=13)for(float x=xmin+6;x<xmax-6;x+=12)
            {
                if(CoverAt(x,z)!="cultivated"||world.sites.Any(s=>Contains(s.polygon,x,z))||NearLine(x,z,7))continue;
                if(CoverAt(x,z-4)!="cultivated"||CoverAt(x,z+4)!="cultivated")continue;
                Surface(Rectangle(x-.35f,z-4,x+.35f,z+4),.055f,fieldMarks);
            }
            MeshObject("Decorative field rows",fieldMarks,Mat(new Color(.58f,.55f,.30f)),root);
        }
        bool NearLine(float x,float z,float margin)
        {
            var p=new Vector2(x,z);
            foreach(var f in world.features)for(int i=1;i<f.points.Length;i++)
            {
                var a=new Vector2(f.points[i-1].x,f.points[i-1].z);var b=new Vector2(f.points[i].x,f.points[i].z);var d=b-a;
                float t=d.sqrMagnitude==0?0:Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);
                if(Vector2.Distance(p,a+t*d)<f.widthM*.5f+margin)return true;
            }
            return false;
        }
        void BaselineRows(Site site,Transform parent)
        {
            var marks=new Geometry();float left=site.polygon.Min(p=>p.x),right=site.polygon.Max(p=>p.x),bottom=site.polygon.Min(p=>p.z),top=site.polygon.Max(p=>p.z);
            for(float x=left+4;x<right-3;x+=8)for(float z=bottom+4;z<top-4;z+=12)
                if(CoverAt(x,z)=="cultivated")Surface(Rectangle(x-.4f,z-4,x+.4f,z+4),.055f,marks);
            MeshObject("Baseline field rows",marks,Mat(new Color(.58f,.55f,.30f)),parent);
        }
        Matrix4x4 PlantMatrix(GameObject prefab,float x,float z,float scale,float yaw)
        {
            float bottom=0;
            if(prefab!=null)
            {
                bottom=float.MaxValue;
                foreach(var f in prefab.GetComponentsInChildren<MeshFilter>(true))
                {
                    var matrix=prefab.transform.worldToLocalMatrix*f.transform.localToWorldMatrix;var b=f.sharedMesh.bounds;
                    for(int i=0;i<8;i++){var point=b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));bottom=Mathf.Min(bottom,matrix.MultiplyPoint3x4(point).y);}
                }
                if(bottom==float.MaxValue)bottom=0;
            }
            float radius=(prefab==tree?.04f:.15f)*scale;
            float ground=Mathf.Min(Height(x-radius,z),Height(x+radius,z),Height(x,z-radius),Height(x,z+radius));
            return Matrix4x4.TRS(new Vector3(x,ground-bottom*scale,z),Quaternion.Euler(0,yaw,0),Vector3.one*scale);
        }
        void BatchPrefab(string name,GameObject prefab,List<Matrix4x4> placements,Transform parent,Color tint)
        {
            if(prefab==null||placements.Count==0)return;
            foreach(var filter in prefab.GetComponentsInChildren<MeshFilter>(true))
            {
                var source=filter.sharedMesh;if(!source.isReadable)throw new InvalidOperationException("Mesh needs Read/Write enabled for batching: "+source.name);
                var local=prefab.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                var sourceMaterials=filter.GetComponent<MeshRenderer>().sharedMaterials;
                for(int sub=0;sub<source.subMeshCount;sub++)for(int start=0;start<placements.Count;start+=180)
                {
                    var pieces=new CombineInstance[Mathf.Min(180,placements.Count-start)];
                    for(int i=0;i<pieces.Length;i++)pieces[i]=new CombineInstance{mesh=source,subMeshIndex=sub,transform=placements[start+i]*local};
                    var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.CombineMeshes(pieces,true,true);meshes.Add(mesh);
                    var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;
                    var renderer=go.AddComponent<MeshRenderer>();var sourceName=sourceMaterials[Mathf.Min(sub,sourceMaterials.Length-1)].name;
                    renderer.sharedMaterial=Mat(sourceName.IndexOf("Bark",StringComparison.OrdinalIgnoreCase)>=0?new Color(.46f,.35f,.23f):tint);
                }
            }
        }
        public void Tick(float time,bool reducedMotion)
        {
            if(glintMaterial!=null)glintMaterial.color=Color.Lerp(new Color(.52f,.69f,.67f),new Color(.72f,.83f,.74f),reducedMotion?.4f:(Mathf.Sin(time*.65f)+1)*.5f);
            if(settlingOutline!=null){float t=reducedMotion?1:Mathf.Clamp01((time-settleStarted)/.45f);settlingOutline.startWidth=settlingOutline.endWidth=Mathf.Lerp(4.4f,1.8f,t*t*(3-2*t))*boundaryScale;if(t>=1)settlingOutline=null;}
        }
        public void Settle(string id){settlingOutline=outlines[id];settleStarted=Time.unscaledTime;}
        public Site Hit(Ray ray)
        {
            float previous=ray.origin.y-Height(ray.origin.x,ray.origin.z);
            for(float distance=5;distance<6000;distance+=5)
            {
                var p=ray.GetPoint(distance);float now=p.y-Height(p.x,p.z);
                if(previous>=0&&now<=0&&Inside(p.x,p.z))
                {
                    float lo=distance-5,hi=distance;
                    for(int i=0;i<9;i++){float mid=(lo+hi)*.5f;var q=ray.GetPoint(mid);if(q.y>Height(q.x,q.z))lo=mid;else hi=mid;}
                    var hit=ray.GetPoint((lo+hi)*.5f);return world.sites.FirstOrDefault(s=>Contains(s.polygon,hit.x,hit.z));
                }
                previous=now;
            }
            return null;
        }
        public static bool Contains(Point[] p,float x,float z)
        {for(int i=0;i<p.Length;i++){var a=p[i];var b=p[(i+1)%p.Length];if((b.x-a.x)*(z-a.z)-(b.z-a.z)*(x-a.x)<-.001f)return false;}return true;}
        public void Highlight(string hover,string selected,bool valid)
        {
            activeOutline=hover;chosenOutline=selected;settlingOutline=null;
            foreach(var pair in outlines){bool active=pair.Key==hover,chosen=pair.Key==selected;pair.Value.sharedMaterial=BoundaryMat(active?(valid?new Color(1,.84f,.36f):new Color(1,.38f,.23f)):chosen?new Color(.96f,.98f,.83f):new Color(.65f,.73f,.50f));}
            ViewScale(boundaryScale);
        }
        Material BoundaryMat(Color color){if(!boundaryMaterials.TryGetValue(color,out var material)){material=new Material(Resources.Load<Shader>("TerraBoundary"));material.SetColor("_BaseColor",color);boundaryMaterials.Add(color,material);}return material;}
        public void ViewScale(float metresPerCssPixel)
        {
            boundaryScale=metresPerCssPixel;
            foreach(var pair in outlines){float width=pair.Key==activeOutline?2.4f:pair.Key==chosenOutline?1.8f:1;
                pair.Value.startWidth=pair.Value.endWidth=width*boundaryScale;
                var backing=outlineBackings[pair.Key];backing.startWidth=backing.endWidth=(width+1.4f)*boundaryScale;}
        }
        public void ClearGhost(){if(ghost!=null)DestroyGroup(ghost);ghost=null;}
        public void Ghost(Site site,bool valid,string treatment)
        {
            ClearGhost();if(site==null)return;ghost=new GameObject("Placement preview").transform;ghost.SetParent(root,false);
            Patch("Preview footprint",site.polygon,valid?new Color(.60f,.72f,.55f):new Color(.70f,.43f,.32f),.19f,ghost);
            if(!valid)return;
            float l=site.polygon.Min(p=>p.x),r=site.polygon.Max(p=>p.x),b=site.polygon.Min(p=>p.z),t=site.polygon.Max(p=>p.z);
            if(treatment.StartsWith("cultivate")){
                if(treatment=="cultivate-margin"){Patch("Proposed living margin",Rectangle(l,b,r,b+(t-b)*.25f),new Color(.40f,.64f,.45f),.23f,ghost);b+=(t-b)*.25f;}
                var rows=new Geometry();for(float x=l+5;x<r-3;x+=8)Surface(Rectangle(x-.65f,b+3,x+.65f,t-3),.24f,rows);
                MeshObject("Proposed crop rhythm",rows,Mat(new Color(.43f,.55f,.37f)),ghost);
            }
            else if(treatment=="water-swale")Ribbon(new[]{new Point(l+8,(b+t)*.5f),new Point(r-8,(b+t)*.5f)},9,new Color(.35f,.58f,.51f),.24f,ghost,"Proposed swale footprint");
            else{
                var plants=new List<Matrix4x4>();for(float x=l+5;x<r-4;x+=12)for(float z=b+5;z<t-4;z+=12)if(Contains(site.polygon,x,z))plants.Add(PlantMatrix(grass,x,z,4,0));
                BatchPrefab("Proposed meadow texture",grass,plants,ghost,new Color(.68f,.79f,.52f));
            }
        }
        public Transform Apply(Site site,string treatment)
        {
            if(proposals.TryGetValue(site.id,out var previous)){DestroyGroup(previous);proposals.Remove(site.id);}
            baselines[site.id].gameObject.SetActive(treatment==null);if(treatment==null)return null;
            var holder=new GameObject("Proposal "+site.id).transform;holder.SetParent(root,false);proposals[site.id]=holder;
            bool cultivated=treatment.StartsWith("cultivate");Patch(treatment,site.polygon,cultivated?new Color(.49f,.36f,.22f):new Color(.46f,.63f,.36f),.10f,holder);
            if(cultivated)PlantRows(site,holder,treatment=="cultivate-margin");
            else if(treatment=="restore-grass")
            {
                var plants=new List<Matrix4x4>();var p=site.polygon;
                for(float x=p.Min(a=>a.x)+3;x<p.Max(a=>a.x)-3;x+=6)for(float z=p.Min(a=>a.z)+3;z<p.Max(a=>a.z)-3;z+=6)
                    if(Contains(p,x,z))plants.Add(PlantMatrix(grass,x,z,Range(4,6),Range(0,360)));
                BatchPrefab("Established meadow illustration",grass,plants,holder,new Color(.48f,.67f,.34f));
            }
            else
            {
                var p=site.polygon;float z=p.Average(a=>a.z);var points=new[]{new Point(p.Min(a=>a.x)+8,z),new Point(p.Max(a=>a.x)-8,z)};
                Ribbon(points,9,new Color(.34f,.49f,.30f),.16f,holder,"Hypothetical vegetated swale");
                Ribbon(points,3,new Color(.38f,.58f,.56f),.20f,holder,"Illustrative swale bed");
            }
            return holder;
        }
        void PlantRows(Site s,Transform parent,bool margin)
        {
            float left=s.polygon.Min(p=>p.x),right=s.polygon.Max(p=>p.x),bottom=s.polygon.Min(p=>p.z),top=s.polygon.Max(p=>p.z);
            if(margin){float end=bottom+(top-bottom)*.25f;Patch("Living margin",Rectangle(left,bottom,right,end),new Color(.45f,.62f,.34f),.14f,parent);bottom=end;}
            var plants=new List<Matrix4x4>();
            for(float x=left+4;x<right-3;x+=6)
            {
                Ribbon(new[]{new Point(x,bottom+2),new Point(x,top-2)},1.2f,new Color(.38f,.29f,.18f),.16f,parent,"Cultivation row");
                for(float z=bottom+4;z<top-3;z+=6)if(Contains(s.polygon,x,z))plants.Add(PlantMatrix(crop,x,z,3.4f,0));
            }
            BatchPrefab("Established crop illustration",crop,plants,parent,new Color(.79f,.72f,.38f));
        }
        static Point[] Rectangle(float l,float b,float r,float t)=>new[]{new Point(l,b),new Point(r,b),new Point(r,t),new Point(l,t)};
        float Range(float a,float b)=>a+(float)random.NextDouble()*(b-a);
        void Patch(string name,Point[] p,Color color,float lift,Transform parent)
        {var geometry=new Geometry();Surface(p,lift,geometry);MeshObject(name,geometry,GroundMat(color),parent);}
        // Clip every overlay against terrain triangles. Shared boundaries therefore share heights.
        void Surface(Point[] points,float lift,Geometry g)
        {
            int x0=IndexX(points.Min(p=>p.x)),x1=IndexX(points.Max(p=>p.x)),z0=IndexZ(points.Min(p=>p.z)),z1=IndexZ(points.Max(p=>p.z));
            var polygon=points.Select(p=>new Vector2(p.x,p.z)).ToList();
            for(int z=z0;z<=z1;z++)for(int x=x0;x<=x1;x++)
            {
                float px=xmin+x*cell,pz=zmin+z*cell;var a=new Vector2(px,pz);var b=new Vector2(px+cell,pz);var c=new Vector2(px,pz+cell);var d=new Vector2(px+cell,pz+cell);
                Add(ClipTriangle(polygon,a,b,c));Add(ClipTriangle(polygon,b,d,c));
            }
            void Add(List<Vector2> poly)
            {
                if(poly.Count<3)return;
                for(int j=1;j<poly.Count-1;j++)
                {
                    if(Mathf.Abs(Cross(poly[j]-poly[0],poly[j+1]-poly[0]))<.00001f)continue;
                    int n=g.vertices.Count;
                    foreach(var p in new[]{poly[0],poly[j+1],poly[j]}){g.vertices.Add(new Vector3(p.x,Height(p.x,p.y)+lift,p.y));g.normals.Add(Normal(p.x,p.y));}
                    g.triangles.AddRange(new[]{n,n+1,n+2});
                }
            }
        }
        static float Cross(Vector2 a,Vector2 b)=>a.x*b.y-a.y*b.x;
        static List<Vector2> ClipTriangle(List<Vector2> input,Vector2 a,Vector2 b,Vector2 c)
        {
            var output=input;var triangle=new[]{a,b,c};
            for(int i=0;i<3&&output.Count>0;i++)
            {
                var start=triangle[i];var edge=triangle[(i+1)%3]-start;var next=new List<Vector2>();var previous=output[output.Count-1];float pd=Cross(edge,previous-start);
                foreach(var current in output)
                {
                    float cd=Cross(edge,current-start);
                    if((cd>=0)!=(pd>=0))next.Add(Vector2.LerpUnclamped(previous,current,pd/(pd-cd)));
                    if(cd>=0)next.Add(current);previous=current;pd=cd;
                }
                output=next;
            }
            return output;
        }
        void Ribbon(Point[] points,float width,Color color,float lift,Transform parent,string name)
        {
            var geometry=new Geometry();
            for(int i=1;i<points.Length;i++)
            {
                var a=new Vector2(points[i-1].x,points[i-1].z);var b=new Vector2(points[i].x,points[i].z);var dir=(b-a).normalized;var side=new Vector2(-dir.y,dir.x)*width*.5f;
                if(dir.sqrMagnitude==0)continue;
                var p=new[]{a-side,b-side,b+side,a+side};Surface(p.Select(v=>new Point(v.x,v.y)).ToArray(),lift,geometry);
            }
            MeshObject(name,geometry,Mat(color),parent);
        }
        LineRenderer Outline(Point[] p,Color color,float width,float lift,Transform parent)
        {
            var go=new GameObject("Hypothetical site outline");go.transform.SetParent(parent,false);var line=go.AddComponent<LineRenderer>();line.useWorldSpace=false;var vertices=new List<Vector3>();
            for(int i=0;i<p.Length;i++)
            {
                var a=p[i];var b=p[(i+1)%p.Length];int count=Mathf.CeilToInt(Vector2.Distance(new Vector2(a.x,a.z),new Vector2(b.x,b.z))/3);
                for(int j=0;j<count;j++){float x=Mathf.Lerp(a.x,b.x,(float)j/count),z=Mathf.Lerp(a.z,b.z,(float)j/count);vertices.Add(new Vector3(x,Height(x,z)+lift,z));}
            }
            line.positionCount=vertices.Count;line.SetPositions(vertices.ToArray());line.loop=true;line.startWidth=line.endWidth=width;line.sharedMaterial=BoundaryMat(color);line.shadowCastingMode=ShadowCastingMode.Off;return line;
        }
        void MeshObject(string name,Geometry geometry,Material material,Transform parent)
        {
            if(geometry.vertices.Count==0)return;
            var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.SetVertices(geometry.vertices);mesh.SetTriangles(geometry.triangles,0);mesh.SetNormals(geometry.normals);mesh.RecalculateBounds();meshes.Add(mesh);
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        void DestroyGroup(Transform group)
        {foreach(var f in group.GetComponentsInChildren<MeshFilter>()){meshes.Remove(f.sharedMesh);Object.Destroy(f.sharedMesh);}Object.Destroy(group.gameObject);}
        public void Dispose(){foreach(var mesh in meshes)Object.Destroy(mesh);foreach(var material in materials.Values)Object.Destroy(material);foreach(var material in groundMaterials.Values)Object.Destroy(material);foreach(var material in boundaryMaterials.Values)Object.Destroy(material);}
    }
}
