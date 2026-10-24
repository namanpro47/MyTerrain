using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;
using UnityEngine;
namespace Terra.Rendering
{
    // This renderer only composes the synthetic contract fixture. No GIS is inferred here.
    public sealed class FixtureLandscape
    {
        readonly WorldData world; readonly Material template; readonly Transform root;
        readonly GameObject tree,crop,grass,shrub; readonly Dictionary<string,Transform> proposals=new Dictionary<string,Transform>();
        readonly Dictionary<string,LineRenderer> outlines=new Dictionary<string,LineRenderer>();
        readonly Dictionary<string,Transform> baselines=new Dictionary<string,Transform>();
        Transform ghost; readonly System.Random random=new System.Random(418);
        public FixtureLandscape(WorldData w,Material material,Transform parent,GameObject tree,GameObject crop,GameObject grass,GameObject shrub)
        {world=w;template=material;root=parent;this.tree=tree;this.crop=crop;this.grass=grass;this.shrub=shrub;}
        public float Height(float x,float z)
        {
            var t=world.terrain;float fx=Mathf.Clamp((x-t.xMin)/t.cellSizeM,0,t.columns-1.001f),fz=Mathf.Clamp((z-t.zMin)/t.cellSizeM,0,t.rows-1.001f);
            int ix=(int)fx,iz=(int)fz;float a=fx-ix,b=fz-iz;int n=iz*t.columns+ix;
            return Mathf.Lerp(Mathf.Lerp(t.heights[n],t.heights[n+1],a),Mathf.Lerp(t.heights[n+t.columns],t.heights[n+t.columns+1],a),b);
        }
        Material Mat(Color c){var m=new Material(template);m.color=c;return m;}
        public void Build()
        {
            var t=world.terrain;var verts=new List<Vector3>();var triangles=new List<int>();var colors=new List<Color>();
            for(int z=0;z<40;z++)for(int x=0;x<50;x++)
            {
                float px=-50+x*2,pz=-40+z*2;int n=verts.Count;
                verts.Add(new Vector3(px,Height(px,pz),pz));verts.Add(new Vector3(px,Height(px,pz+2),pz+2));verts.Add(new Vector3(px+2,Height(px+2,pz+2),pz+2));verts.Add(new Vector3(px+2,Height(px+2,pz),pz));
                triangles.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            MeshObject("Rolling grassland",verts.ToArray(),triangles.ToArray(),new Color(.38f,.49f,.26f),root);
            // Soil edge follows the height-grid surface; bottoms share a flat datum.
            var edge=new List<Vector3>();var tris=new List<int>();
            var perimeter=new[]{new Vector2(-50,-40),new Vector2(50,-40),new Vector2(50,40),new Vector2(-50,40)};
            for(int j=0;j<4;j++)for(int i=0;i<50;i++)
            {
                var a=Vector2.Lerp(perimeter[j],perimeter[(j+1)%4],i/50f);var b=Vector2.Lerp(perimeter[j],perimeter[(j+1)%4],(i+1)/50f);int n=edge.Count;
                edge.Add(new Vector3(a.x,Height(a.x,a.y),a.y));edge.Add(new Vector3(b.x,Height(b.x,b.y),b.y));edge.Add(new Vector3(b.x,-4,b.y));edge.Add(new Vector3(a.x,-4,a.y));tris.AddRange(new[]{n,n+1,n+2,n,n+2,n+3});
            }
            MeshObject("Ochre earth edge",edge.ToArray(),tris.ToArray(),new Color(.42f,.29f,.19f),root);
            foreach(var c in world.covers)Patch(c.id,c.polygon,c.coverId=="woodland"?new Color(.32f,.45f,.23f):new Color(.56f,.65f,.34f),.025f,root);
            foreach(var f in world.features)
            {
                if(f.kind=="water")Ribbon(new Feature{id="Vegetated banks",kind="bank",widthM=f.widthM+3,points=f.points});
                Ribbon(f);
            }
            for(float x=-46;x<22;x+=5.5f)for(float z=31;z<39;z+=4.5f)Spawn(tree,x+Range(-1,1),z+Range(-1,1),Range(7,11),root);
            for(float z=8;z<30;z+=5)Spawn(tree,Range(-46,-41),z,Range(8,11),root);
            foreach(var site in world.sites)
            {
                var b=new GameObject("Baseline "+site.name).transform;b.SetParent(root);baselines[site.id]=b;
                Patch(site.name,site.polygon,site.protectedSite?new Color(.27f,.43f,.23f):site.baselineProduction>0?new Color(.66f,.64f,.33f):new Color(.61f,.70f,.37f),.06f,b);
                if(site.baselineProduction>0)PlantRows(site,b,6,false);
                if(site.protectedSite)for(float x=site.polygon.Min(p=>p.x)+3;x<site.polygon.Max(p=>p.x)-1;x+=5)for(float z=site.polygon.Min(p=>p.z)+3;z<site.polygon.Max(p=>p.z)-1;z+=5)Spawn(tree,x,z,Range(7,9),b);
                outlines[site.id]=Outline(site.polygon,new Color(.94f,.91f,.68f,.75f),.25f,.13f,root);
            }
            for(int i=0;i<100;i++)
            {
                float x=Range(-47,47),z=Range(-37,37);
                if(z>29||x<-40|| (x>39&&z>3))Spawn(tree,x,z,Range(6,9),root);
                else if(!world.sites.Any(s=>Contains(s.polygon,x,z))&&Mathf.Abs(x-33)>5&&Mathf.Abs(z+26)>4)Spawn(i%3==0?shrub:grass,x,z,Range(1.2f,2.3f),root);
            }
        }
        public Site Hit(Ray ray)
        {
            // Intersect the continuous height surface; camera bounds keep it in a safe range.
            float prev=ray.origin.y-Height(ray.origin.x,ray.origin.z);
            for(float d=1;d<400;d+=1)
            {
                var p=ray.GetPoint(d);float now=p.y-Height(p.x,p.z);
                if(prev>=0&&now<=0){var q=ray.GetPoint(d-1+prev/(prev-now));return world.sites.FirstOrDefault(s=>Contains(s.polygon,q.x,q.z));}prev=now;
            }
            return null;
        }
        public static bool Contains(Point[] p,float x,float z)
        {for(int i=0;i<p.Length;i++){var a=p[i];var b=p[(i+1)%p.Length];if((b.x-a.x)*(z-a.z)-(b.z-a.z)*(x-a.x)<0)return false;}return true;}
        public void Highlight(string hover,string selected,bool valid)
        {
            foreach(var pair in outlines)
            {bool active=pair.Key==hover;bool chosen=pair.Key==selected;pair.Value.startWidth=pair.Value.endWidth=active?.65f:chosen?.45f:.23f;pair.Value.material.color=active?(valid?new Color(1,.90f,.50f):new Color(1,.43f,.28f)):chosen?Color.white:new Color(.88f,.89f,.65f);}
        }
        public void ClearGhost(){if(ghost!=null)Object.Destroy(ghost.gameObject);ghost=null;}
        public void Ghost(Site site,bool valid)
        {ClearGhost();if(site==null)return;ghost=new GameObject("Placement preview").transform;ghost.SetParent(root);Patch("Ghost footprint",site.polygon,valid?new Color(.72f,.72f,.41f):new Color(.7f,.40f,.24f),.2f,ghost);Outline(site.polygon,valid?new Color(1,.94f,.59f):new Color(1,.39f,.24f),.45f,.7f,ghost);}
        public Transform Apply(Site site,string treatment)
        {
            if(proposals.TryGetValue(site.id,out var previous)){Object.Destroy(previous.gameObject);proposals.Remove(site.id);}
            baselines[site.id].gameObject.SetActive(treatment==null);
            if(treatment==null)return null;
            var holder=new GameObject("Proposal "+site.id).transform;holder.SetParent(root);proposals[site.id]=holder;
            bool cultivated=treatment.StartsWith("cultivate");Patch(treatment,site.polygon,cultivated?new Color(.50f,.32f,.17f):new Color(.37f,.57f,.25f),.1f,holder);
            if(cultivated)PlantRows(site,holder,9,treatment=="cultivate-margin");
            else if(treatment=="restore-grass")
            {var p=site.polygon;for(float x=p.Min(a=>a.x)+2;x<p.Max(a=>a.x)-1;x+=3)for(float z=p.Min(a=>a.z)+2;z<p.Max(a=>a.z)-1;z+=3)Spawn(grass,x,z,2.2f,holder);}
            else
            {var p=site.polygon;float z=p.Average(a=>a.z);var f=new Feature{id="Swale proposal",widthM=3,kind="water",points=new[]{new Point(p.Min(a=>a.x)+2,z),new Point(p.Max(a=>a.x)-2,z)}};Ribbon(f,holder);}
            return holder;
        }
        void PlantRows(Site s,Transform parent,int rows,bool margin)
        {
            float left=s.polygon.Min(p=>p.x),right=s.polygon.Max(p=>p.x),bottom=s.polygon.Min(p=>p.z),top=s.polygon.Max(p=>p.z);
            if(margin){Patch("Living margin",new[]{new Point(left,bottom),new Point(right,bottom),new Point(right,bottom+(top-bottom)*.25f),new Point(left,bottom+(top-bottom)*.25f)},new Color(.48f,.67f,.28f),.14f,parent);bottom+=(top-bottom)*.25f;}
            for(int row=0;row<rows;row++){float x=Mathf.Lerp(left+1,right-1,(row+.5f)/rows);Ribbon(new Feature{id="Cultivation row",kind="furrow",widthM=.45f,points=new[]{new Point(x,bottom+.5f),new Point(x,top-.5f)}},parent);for(float z=bottom+1;z<top-1;z+=2)Spawn(crop,x,z,1.7f,parent);}
        }
        void Spawn(GameObject prefab,float x,float z,float scale,Transform parent)
        {if(prefab==null)return;var go=Object.Instantiate(prefab,new Vector3(x,Height(x,z)+.14f,z),Quaternion.Euler(0,prefab==crop?0:Range(0,360),0),parent);go.transform.localScale=Vector3.one*scale;}
        float Range(float a,float b)=>a+(float)random.NextDouble()*(b-a);
        void Patch(string name,Point[] p,Color c,float lift,Transform parent)
        {
            // Subdivide each fan triangle so overlays conform to the terrain, not a flat quad.
            var verts=new List<Vector3>();var tris=new List<int>();
            for(int i=1;i<p.Length-1;i++)
            {
                var a=new Vector2(p[0].x,p[0].z);var b=new Vector2(p[i].x,p[i].z);var d=new Vector2(p[i+1].x,p[i+1].z);const int n=8;
                for(int row=0;row<n;row++)for(int col=0;col<n-row;col++)
                {var v0=a+(b-a)*row/n+(d-a)*col/n;var v1=a+(b-a)*(row+1)/n+(d-a)*col/n;var v2=a+(b-a)*row/n+(d-a)*(col+1)/n;Add(v0,v2,v1);if(col<n-row-1){var v3=a+(b-a)*(row+1)/n+(d-a)*(col+1)/n;Add(v1,v2,v3);}}
            }
            MeshObject(name,verts.ToArray(),tris.ToArray(),c,parent);
            void Add(Vector2 a,Vector2 b,Vector2 d){int n=verts.Count;foreach(var v in new[]{a,b,d})verts.Add(new Vector3(v.x,Height(v.x,v.y)+lift,v.y));tris.AddRange(new[]{n,n+1,n+2});}
        }
        void Ribbon(Feature f,Transform parent=null)
        {
            float lift=f.kind=="bank"?.11f:f.kind=="path"?.30f:f.kind=="furrow"?.16f:.24f;
            var pts=new List<Vector3>();for(int i=0;i<f.points.Length-1;i++){var a=f.points[i];var b=f.points[i+1];for(int j=0;j<12;j++){float x=Mathf.Lerp(a.x,b.x,j/12f),z=Mathf.Lerp(a.z,b.z,j/12f);pts.Add(new Vector3(x,Height(x,z)+lift,z));}}
            var last=f.points.Last();pts.Add(new Vector3(last.x,Height(last.x,last.z)+lift,last.z));
            var go=new GameObject(f.id);go.transform.SetParent(parent??root);var line=go.AddComponent<LineRenderer>();line.useWorldSpace=true;line.positionCount=pts.Count;line.SetPositions(pts.ToArray());line.startWidth=line.endWidth=f.widthM;line.alignment=LineAlignment.TransformZ;go.transform.rotation=Quaternion.Euler(90,0,0);line.numCornerVertices=4;line.numCapVertices=3;
            line.material=Mat(f.kind=="furrow"?new Color(.38f,.28f,.14f):f.kind=="path"?new Color(.78f,.69f,.48f):f.kind=="connection"?new Color(.7f,.84f,.78f):f.kind=="bank"?new Color(.33f,.43f,.23f):new Color(.22f,.55f,.59f));
        }
        LineRenderer Outline(Point[] p,Color c,float width,float lift,Transform parent)
        {var go=new GameObject("Site outline");go.transform.SetParent(parent);var line=go.AddComponent<LineRenderer>();line.useWorldSpace=true;var v=new List<Vector3>();for(int i=0;i<p.Length;i++)for(int j=0;j<10;j++){var a=p[i];var b=p[(i+1)%p.Length];float x=Mathf.Lerp(a.x,b.x,j/10f),z=Mathf.Lerp(a.z,b.z,j/10f);v.Add(new Vector3(x,Height(x,z)+lift,z));}line.positionCount=v.Count;line.SetPositions(v.ToArray());line.loop=true;line.startWidth=line.endWidth=width;line.material=Mat(c);return line;}
        void MeshObject(string name,Vector3[] v,int[] t,Color color,Transform parent)
        {var go=new GameObject(name);go.transform.SetParent(parent);var mesh=new Mesh{name=name};mesh.vertices=v;mesh.triangles=t;mesh.RecalculateNormals();mesh.RecalculateBounds();go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=Mat(color);}
    }
}
