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
    public sealed partial class LandscapeRenderer : IDisposable
    {
        readonly WorldData world; readonly Material template; readonly Transform root;
        readonly bool clusteredWoodland;
        readonly GameObject tree,crop,grass; readonly System.Random random=new System.Random(418);
        readonly Dictionary<string,Transform> proposals=new Dictionary<string,Transform>();
        readonly Dictionary<string,Transform> baselines=new Dictionary<string,Transform>();
        readonly Dictionary<string,LineRenderer> outlines=new Dictionary<string,LineRenderer>();
        readonly HashSet<string> availableFields=new HashSet<string>();
        readonly Dictionary<Color,Material> boundaryMaterials=new Dictionary<Color,Material>();
        string activeOutline,chosenOutline;float boundaryScale=2;
        readonly Dictionary<Color,Material> materials=new Dictionary<Color,Material>();
        readonly Dictionary<Color,Material> groundMaterials=new Dictionary<Color,Material>();
        readonly List<Mesh> meshes=new List<Mesh>();
        readonly Dictionary<GameObject,float> plantFloors=new Dictionary<GameObject,float>();
        Texture2D visualCover,riverMask,pathMask;Material streamMaterial,swaleMaterial,pathMaterial;
        readonly List<CoverPatch>[,] coverIndex; readonly float xmin,zmin,xmax,zmax,cell;
        GameObject meshCanopy;
        Transform ghost;string ghostSiteId;Material glintMaterial;LineRenderer settlingOutline;float settleStarted;
        static readonly Color Wood=new Color(.20f,.35f,.24f),Field=new Color(.64f,.60f,.34f),Meadow=new Color(.46f,.59f,.34f),Water=new Color(.22f,.48f,.51f);
        sealed class Geometry
        {public readonly List<Vector3> vertices=new List<Vector3>();public readonly List<Vector3> normals=new List<Vector3>();public readonly List<int> triangles=new List<int>();public readonly List<Vector2> uv=new List<Vector2>();}
        public LandscapeRenderer(WorldData w,Material material,Transform parent,GameObject tree,GameObject crop,GameObject grass,GameObject shrub,bool clusteredWoodland=false)
        {
            world=w;template=material;root=parent;this.tree=tree;this.crop=crop;this.grass=grass;this.clusteredWoodland=clusteredWoodland;
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
        // Rendering and picking share this display surface. Analytical rules read
        // WorldData through the core terrain sampler, never this visual abstraction.
        public float Height(float x,float z)
        {
            if(!clusteredWoodland)return SourceHeight(x,z);
            float dx=(xmax-xmin)/Mathf.Ceil((xmax-xmin)/1.5f),dz=(zmax-zmin)/Mathf.Ceil((zmax-zmin)/1.5f);
            float gx=Mathf.Clamp((x-xmin)/dx,0,(xmax-xmin)/dx),gz=Mathf.Clamp((z-zmin)/dz,0,(zmax-zmin)/dz);
            int ix=Mathf.Min(Mathf.FloorToInt(gx),Mathf.RoundToInt((xmax-xmin)/dx)-1),iz=Mathf.Min(Mathf.FloorToInt(gz),Mathf.RoundToInt((zmax-zmin)/dz)-1);
            float px=xmin+ix*dx,pz=zmin+iz*dz,u=gx-ix,v=gz-iz;
            float a=ReliefControl(px,pz),b=ReliefControl(px+dx,pz),c=ReliefControl(px,pz+dz),d=ReliefControl(px+dx,pz+dz);
            return u+v<=1?a+(b-a)*u+(c-a)*v:d+(c-d)*(1-u)+(b-d)*(1-v);
        }
        float ReliefControl(float x,float z)
        {
            float bands=SourceHeight(x,z)/6f,part=bands-Mathf.Floor(bands);
            return (Mathf.Floor(bands)+Mathf.SmoothStep(0,1,Mathf.InverseLerp(.38f,.62f,part)))*6f;
        }
        public float SourceHeight(float x,float z)
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
        {if(!groundMaterials.TryGetValue(color,out var material)){material=new Material(Resources.Load<Shader>("TerraGround"));material.SetColor("_BaseColor",color);material.SetTexture("_BaseMap",Resources.Load<Texture2D>("ArtistGrassGround"));material.SetFloat("_MeadowScale",clusteredWoodland?12:40);material.SetFloat("_SurfaceKind",color.b>color.r*1.5f?2:color.g>color.r*1.1f?1:0);groundMaterials.Add(color,material);}return material;}
        static Color CoverColor(string id)=>id=="woodland"?Wood:id=="cultivated"?Field:id=="water"?Water:id=="sealed"?new Color(.62f,.58f,.46f):Meadow;
        public void Build()
        {
            var timer=System.Diagnostics.Stopwatch.StartNew();
            BuildVisualGround();
            BuildEdge();
            foreach(var f in world.features.Where(f=>f.kind=="water"))
            {Ribbon(RoundedWaterRoute(f.points),f.widthM+5.2f,new Color(.13f,.48f,.49f),-.9f,root,"Mapped stream centreline");}
            BuildCrossings();
            BuildVegetation();
            BuildStreamsidePlants();
            BuildCultivatedContext();
            foreach(var site in world.sites)
            {
                var holder=new GameObject("Baseline "+site.name).transform;holder.SetParent(root,false);baselines[site.id]=holder;
                if(!site.protectedSite)BuildBaselinePlants(site,holder);
                outlines[site.id]=Outline(site.polygon,new Color(.65f,.73f,.50f),1.1f,.36f,root);
            }
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
        void BuildVisualGround()
        {
            // Smooth visual classes only. All CoverAt queries, accounting and source polygons stay analytical.
            const int resolution=256;
            visualCover=new Texture2D(resolution,resolution,TextureFormat.RGBA32,true,true){name="Illustrative cover blend",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Bilinear};
            var pixels=new Color[resolution*resolution];
            for(int z=0;z<resolution;z++)for(int x=0;x<resolution;x++)
            {
                float px=Mathf.Lerp(xmin,xmax,(x+.5f)/resolution),pz=Mathf.Lerp(zmin,zmax,(z+.5f)/resolution);
                string cover=CoverAt(px,pz);
                pixels[z*resolution+x]=cover=="woodland"?new Color(1,0,0,0):cover=="cultivated"?new Color(0,1,0,0):cover=="sealed"?new Color(0,0,0,1):new Color(0,0,1,0);
            }
            // A continuous illustrative mask rounds the 30 m source raster; source classifications are untouched.
            var blurred=new Color[pixels.Length];
            for(int pass=0;pass<2;pass++)
            {
                for(int z=0;z<resolution;z++)for(int x=0;x<resolution;x++)
                {
                    Color sum=Color.clear;float weight=0;
                    for(int k=-5;k<=5;k++)
                    {
                        float w=Mathf.Exp(-k*k/18f);
                        int sx=pass==0?Mathf.Clamp(x+k,0,resolution-1):x;
                        int sz=pass==1?Mathf.Clamp(z+k,0,resolution-1):z;
                        sum+=pixels[sz*resolution+sx]*w;weight+=w;
                    }
                    blurred[z*resolution+x]=sum/weight;
                }
                var swap=pixels;pixels=blurred;blurred=swap;
            }
            visualCover.SetPixels(pixels);visualCover.Apply();
            var material=GroundMat(Color.white);material.SetFloat("_SurfaceKind",3);
            // A single artist-authored ground family supplies color and matching relief.
            material.SetTexture("_BaseMap",Resources.Load<Texture2D>("JulioGround/EdgeColor"));
            material.SetTexture("_BankMap",Resources.Load<Texture2D>("JulioGround/EarthColor"));
            material.SetTexture("_GrassNormal",Resources.Load<Texture2D>("JulioGround/EdgeNormal"));
            material.SetTexture("_EarthNormal",Resources.Load<Texture2D>("JulioGround/EarthNormal"));
            material.SetTexture("_MeadowPaint",Resources.Load<Texture2D>("JulioGround/MeadowColor"));
            material.SetTexture("_MeadowRelief",Resources.Load<Texture2D>("JulioGround/MeadowNormal"));
            material.SetTexture("_StonesPaint",Resources.Load<Texture2D>("JulioGround/StonesColor"));
            material.SetTexture("_StonesNormal",Resources.Load<Texture2D>("JulioGround/StonesNormal"));
            material.SetTexture("_LandCoverMap",visualCover);material.SetVector("_LandBounds",new Vector4(xmin,zmin,xmax-xmin,zmax-zmin));
            CreateTreatmentSurface(material);
            BuildPathMask(material);
            BuildChannelTerrain(material);
        }
        bool VisualCropAt(float x,float z)
        {
            float weight=visualCover.GetPixelBilinear((x-xmin)/(xmax-xmin),(z-zmin)/(zmax-zmin)).g;
            return weight>.52f;
        }
        void BuildVegetation()
        {
            var nature=root.gameObject.AddComponent<InstancedNature>();
            var tuft=Resources.Load<GameObject>("Nature/Grass_Common_Short");
            var wispy=Resources.Load<GameObject>("Nature/Grass_Wispy_Short");
            var flower=Resources.Load<GameObject>("Nature/Flower_3_Group");
            var stone=Resources.Load<GameObject>("Nature/Rock_Medium_1");
            // Low plants and stones are decorative; woodland cohorts are built separately.
            // Spatial patches keep detail cullable without changing analytical cover.
            for(float cz=zmin;cz<zmax;cz+=150)for(float cx=xmin;cx<xmax;cx+=150)
            {
                var grasses=new List<Matrix4x4>();var fine=new List<Matrix4x4>();var flowers=new List<Matrix4x4>();var rocks=new List<Matrix4x4>();
                for(float z=cz+8;z<Mathf.Min(cz+150,zmax-6);z+=13)for(float x=cx+8;x<Mathf.Min(cx+150,xmax-6);x+=13)
                {
                    float px=x+Range(-5,5),pz=z+Range(-5,5);string cover=CoverAt(px,pz);
                    if(NearLine(px,pz,4))continue;
                    float grouping=Mathf.PerlinNoise((px+1400)*.014f,(pz+1400)*.014f);
                    if(cover=="grass" || cover=="woodland"&&grouping<.4f)
                    {
                        if(world.sites.Any(site=>Contains(site.polygon,px,pz)))continue;
                        // Metre-scale plants form small stands with quiet ground between them.
                        // These positions illustrate habitat structure, not observed plant locations.
                        if(grouping>.44f||cover=="woodland")
                        {
                            float angle=Range(0,Mathf.PI*2),radius=Range(2.5f,5.5f);
                            int count=random.Next(9,17);
                            for(int i=0;i<count;i++)
                            {
                                float theta=Range(0,Mathf.PI*2),distance=Mathf.Sqrt(Range(0,1))*radius;
                                float u=Mathf.Cos(theta)*distance,v=Mathf.Sin(theta)*distance*.55f;
                                float gx=px+Mathf.Cos(angle)*u-Mathf.Sin(angle)*v,gz=pz+Mathf.Sin(angle)*u+Mathf.Cos(angle)*v;
                                if(CoverAt(gx,gz)!=cover||NearLine(gx,gz,3)||world.sites.Any(site=>Contains(site.polygon,gx,gz)))continue;
                                grasses.Add(PlantMatrix(tuft,gx,gz,Range(.35f,.65f),Range(0,360)));
                                if(i%4==0)fine.Add(PlantMatrix(wispy,gx+.3f,gz,Range(.5f,.85f),Range(0,360)));
                                if(grouping>.57f&&i%5==0)flowers.Add(PlantMatrix(flower,gx,gz+.2f,Range(.25f,.42f),Range(0,360)));
                            }
                        }
                        if(grouping<.30f && random.NextDouble()<.18)rocks.Add(PlantMatrix(stone,px,pz,Range(1.1f,2.2f),Range(0,360)));
                    }
                }
                nature.Add(tuft,grasses,Color.white,true);
                nature.Add(wispy,fine,Color.white,true);nature.Add(flower,flowers,Color.white);nature.Add(stone,rocks,Color.white);
            }
            BuildWoodlandCohorts();
            BuildForestFloor();
            BuildGrassland();
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
            if(prefab!=null&&!plantFloors.TryGetValue(prefab,out bottom))
            {
                bottom=float.MaxValue;
                foreach(var f in prefab.GetComponentsInChildren<MeshFilter>(true))
                {
                    var matrix=prefab.transform.worldToLocalMatrix*f.transform.localToWorldMatrix;var b=f.sharedMesh.bounds;
                    for(int i=0;i<8;i++){var point=b.center+Vector3.Scale(b.extents,new Vector3((i&1)==0?-1:1,(i&2)==0?-1:1,(i&4)==0?-1:1));bottom=Mathf.Min(bottom,matrix.MultiplyPoint3x4(point).y);}
                }
                if(bottom==float.MaxValue)bottom=0;
                plantFloors[prefab]=bottom;
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
            if(streamMaterial!=null)streamMaterial.SetFloat("_MotionTime",reducedMotion?0:time);
            if(swaleMaterial!=null)swaleMaterial.SetFloat("_MotionTime",reducedMotion?0:time);
            if(glintMaterial!=null)glintMaterial.color=Color.Lerp(new Color(.52f,.69f,.67f),new Color(.72f,.83f,.74f),reducedMotion?.4f:(Mathf.Sin(time*.65f)+1)*.5f);
            if(settlingOutline!=null){float t=reducedMotion?1:Mathf.Clamp01((time-settleStarted)/.45f);settlingOutline.startWidth=settlingOutline.endWidth=(Mathf.Lerp(4.4f,1.8f,t*t*(3-2*t))+1.4f)*boundaryScale;if(t>=1)settlingOutline=null;}
        }
        public void Settle(string id,bool reducedMotion=false){settlingOutline=outlines[id];settleStarted=Time.unscaledTime;if(!reducedMotion&&proposals.TryGetValue(id,out var group))foreach(var plants in group.GetComponentsInChildren<InstancedNature>())plants.Reveal(.7f);}
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
        public void Highlight(string hover,string selected,bool valid,IEnumerable<string> available=null)
        {
            availableFields.Clear();if(available!=null)availableFields.UnionWith(available);
            activeOutline=hover;chosenOutline=selected;settlingOutline=null;
            foreach(var pair in outlines){bool active=pair.Key==hover,chosen=pair.Key==selected;pair.Value.sharedMaterial=BoundaryMat(active?(valid?new Color(1,.84f,.36f):new Color(1,.38f,.23f)):availableFields.Contains(pair.Key)?new Color(.35f,1f,.88f):chosen?new Color(.96f,.98f,.83f):new Color(.65f,.73f,.50f));}
            ViewScale(boundaryScale);
        }
        Material BoundaryMat(Color color){if(!boundaryMaterials.TryGetValue(color,out var material)){material=new Material(Resources.Load<Shader>("TerraBoundary"));material.SetColor("_BaseColor",color);boundaryMaterials.Add(color,material);}return material;}
        public void ViewScale(float metresPerCssPixel)
        {
            FlushTreatmentSurface();
            boundaryScale=metresPerCssPixel;
            foreach(var pair in outlines){float width=pair.Key==activeOutline?2.4f:availableFields.Contains(pair.Key)?2f:pair.Key==chosenOutline?1.8f:1;
                pair.Value.enabled=pair.Key==activeOutline||pair.Key==chosenOutline||availableFields.Contains(pair.Key);
                pair.Value.startWidth=pair.Value.endWidth=(width+1.4f)*boundaryScale;}
        }
        public void ClearGhost()
        {
            if(ghost!=null)DestroyGroup(ghost);ghost=null;
            if(ghostSiteId!=null){surfaceDirty=true;ghostTreatment=null;if(proposals.TryGetValue(ghostSiteId,out var prior))prior.gameObject.SetActive(true);baselines[ghostSiteId].gameObject.SetActive(!proposals.ContainsKey(ghostSiteId));ghostSiteId=null;}
        }
        public void Ghost(Site site,bool valid,string treatment)
        {
            ClearGhost();if(site==null)return;ghost=new GameObject("Placement preview").transform;ghost.SetParent(root,false);
            if(!valid){Patch("Unavailable footprint",site.polygon,new Color(.62f,.29f,.23f),.22f,ghost);return;}
            ghostSiteId=site.id;ghostTreatment=treatment;surfaceDirty=true;baselines[site.id].gameObject.SetActive(false);if(proposals.TryGetValue(site.id,out var previous))previous.gameObject.SetActive(false);
            BuildTreatment(site,treatment,ghost,true);
        }
        public Transform Apply(Site site,string treatment)
        {
            if(treatment==null)surfaceTreatments.Remove(site.id);else surfaceTreatments[site.id]=treatment;
            surfaceDirty=true;
            if(proposals.TryGetValue(site.id,out var previous)){DestroyGroup(previous);proposals.Remove(site.id);}
            baselines[site.id].gameObject.SetActive(treatment==null);if(treatment==null)return null;
            var holder=new GameObject("Proposal "+site.id).transform;holder.SetParent(root,false);proposals[site.id]=holder;
            BuildTreatment(site,treatment,holder,false);return holder;
        }
        static Point[] Rectangle(float l,float b,float r,float t)=>new[]{new Point(l,b),new Point(r,b),new Point(r,t),new Point(l,t)};
        float Range(float a,float b)=>a+(float)random.NextDouble()*(b-a);
        void Patch(string name,Point[] p,Color color,float lift,Transform parent)
        {var geometry=new Geometry();Surface(p,lift,geometry);MeshObject(name,geometry,GroundMat(color),parent);}
        // Clip every overlay against terrain triangles. Shared boundaries therefore share heights.
        void Surface(Point[] points,float lift,Geometry g)
        {
            // Overlays use the same grid as the displayed terrain. Coarse source
            // triangles would bridge across the illustrative slopes and float above them.
            float dx=clusteredWoodland?(xmax-xmin)/Mathf.Ceil((xmax-xmin)/1.5f):cell;
            float dz=clusteredWoodland?(zmax-zmin)/Mathf.Ceil((zmax-zmin)/1.5f):cell;
            int nx=Mathf.RoundToInt((xmax-xmin)/dx),nz=Mathf.RoundToInt((zmax-zmin)/dz);
            int x0=Mathf.Clamp(Mathf.FloorToInt((points.Min(p=>p.x)-xmin)/dx),0,nx-1),x1=Mathf.Clamp(Mathf.FloorToInt((points.Max(p=>p.x)-xmin)/dx),0,nx-1);
            int z0=Mathf.Clamp(Mathf.FloorToInt((points.Min(p=>p.z)-zmin)/dz),0,nz-1),z1=Mathf.Clamp(Mathf.FloorToInt((points.Max(p=>p.z)-zmin)/dz),0,nz-1);
            var polygon=points.Select(p=>new Vector2(p.x,p.z)).ToList();
            int width=x1-x0+2;
            var gridVertices=new int[width*(z1-z0+2)];
            var gridInside=new byte[gridVertices.Length];
            for(int z=z0;z<=z1;z++)for(int x=x0;x<=x1;x++)
            {
                float px=xmin+x*dx,pz=zmin+z*dz;var a=new Vector2(px,pz);var b=new Vector2(px+dx,pz);var c=new Vector2(px,pz+dz);var d=new Vector2(px+dx,pz+dz);
                // Interior cells need no polygon clipping. Share their grid vertices,
                // retaining the same triangles, display heights and smooth normals.
                // Only cells touching the polygon edge use the general clipping path.
                if(Inside(x,z,a)&&Inside(x+1,z,b)&&Inside(x,z+1,c)&&Inside(x+1,z+1,d))
                {
                    int ia=Vertex(x,z,a),ib=Vertex(x+1,z,b),ic=Vertex(x,z+1,c),id=Vertex(x+1,z+1,d);
                    g.triangles.Add(ia);g.triangles.Add(ic);g.triangles.Add(ib);
                    g.triangles.Add(ib);g.triangles.Add(ic);g.triangles.Add(id);
                    continue;
                }
                Add(ClipTriangle(polygon,a,b,c));Add(ClipTriangle(polygon,b,d,c));
            }
            bool Inside(int x,int z,Vector2 p)
            {
                int index=(z-z0)*width+x-x0;
                if(gridInside[index]!=0)return gridInside[index]==1;
                for(int i=0;i<polygon.Count;i++)
                    if(Cross(polygon[(i+1)%polygon.Count]-polygon[i],p-polygon[i])<0)
                    {gridInside[index]=2;return false;}
                gridInside[index]=1;return true;
            }
            int Vertex(int x,int z,Vector2 p)
            {
                int index=(z-z0)*width+x-x0;
                if(gridVertices[index]!=0)return gridVertices[index]-1;
                int vertex=g.vertices.Count;gridVertices[index]=vertex+1;
                g.vertices.Add(new Vector3(p.x,Height(p.x,p.y)+lift,p.y));g.normals.Add(Normal(p.x,p.y));
                return vertex;
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
            var left=new Vector2[points.Length];var right=new Vector2[points.Length];
            for(int i=0;i<points.Length;i++)
            {
                var current=new Vector2(points[i].x,points[i].z);
                var previous=new Vector2(points[Mathf.Max(0,i-1)].x,points[Mathf.Max(0,i-1)].z);
                var next=new Vector2(points[Mathf.Min(points.Length-1,i+1)].x,points[Mathf.Min(points.Length-1,i+1)].z);
                var before=(current-previous).normalized;var after=(next-current).normalized;
                if(i==0)before=after;if(i==points.Length-1)after=before;
                var n1=new Vector2(-before.y,before.x);var n2=new Vector2(-after.y,after.x);
                var miter=(n1+n2).normalized;float length=width*.5f/Mathf.Max(.65f,Vector2.Dot(miter,n2));
                left[i]=current-miter*length;right[i]=current+miter*length;
            }
            float along=0;
            for(int i=1;i<points.Length;i++)
            {
                var p=new[]{left[i-1],left[i],right[i],right[i-1]};
                int first=geometry.vertices.Count;
                Surface(p.Select(v=>new Point(v.x,v.y)).ToArray(),lift,geometry);
                var start=new Vector2(points[i-1].x,points[i-1].z);var segment=new Vector2(points[i].x,points[i].z)-start;
                var direction=segment.normalized;var across=new Vector2(-direction.y,direction.x);
                for(int j=first;j<geometry.vertices.Count;j++)
                {
                    var vertex=geometry.vertices[j];var delta=new Vector2(vertex.x,vertex.z)-start;
                    geometry.uv.Add(new Vector2(Vector2.Dot(delta,across)/width+.5f,along+Vector2.Dot(delta,direction)));
                }
                along+=segment.magnitude;
            }
            MeshObject(name,geometry,(name=="Mapped stream centreline"||name=="Illustrative swale water")?StreamMaterial(name=="Mapped stream centreline"):Mat(color),parent);
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
            var mesh=new Mesh{name=name,indexFormat=IndexFormat.UInt32};mesh.SetVertices(geometry.vertices);mesh.SetTriangles(geometry.triangles,0);mesh.SetNormals(geometry.normals);if(geometry.uv.Count==geometry.vertices.Count)mesh.SetUVs(0,geometry.uv);mesh.RecalculateBounds();meshes.Add(mesh);
            var go=new GameObject(name);go.transform.SetParent(parent,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=material;
        }
        void DestroyGroup(Transform group)
        {foreach(var f in group.GetComponentsInChildren<MeshFilter>(true))if(meshes.Remove(f.sharedMesh))Object.Destroy(f.sharedMesh);Object.Destroy(group.gameObject);}
        public void Dispose(){if(forestFloor!=null)Object.Destroy(forestFloor);if(projectGrassMaterial!=null)Object.Destroy(projectGrassMaterial);if(projectPathMaterial!=null)Object.Destroy(projectPathMaterial);if(treatmentSurface!=null)Object.Destroy(treatmentSurface);if(pathMask!=null)Object.Destroy(pathMask);if(visualCover!=null)Object.Destroy(visualCover);if(riverMask!=null)Object.Destroy(riverMask);if(pathMaterial!=null)UnityEngine.Object.Destroy(pathMaterial);if(streamMaterial!=null)Object.Destroy(streamMaterial);if(swaleMaterial!=null)Object.Destroy(swaleMaterial);foreach(var mesh in meshes)Object.Destroy(mesh);foreach(var material in materials.Values)Object.Destroy(material);foreach(var material in groundMaterials.Values)Object.Destroy(material);foreach(var material in boundaryMaterials.Values)Object.Destroy(material);}

    }
}
