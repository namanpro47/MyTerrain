using System.Linq;
using Terra.Contracts;
using Terra.Core;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        // Authored illustrations of the four homes already counted by FullAnnualGameplay.
        // These fixed Wisconsin placements are outside management opportunities. They
        // never create annual assets, change source cover or award services/income.
        static readonly Vector2[] ExistingHomeSites={new Vector2(-285,4),new Vector2(-321,18),new Vector2(-355,35),new Vector2(-140,-20)};

        void BuildExistingCommunity()
        {
            if(!clusteredWoodland)return;
            var prefab=buildCatalog?.Find("low-density-housing")?.prefab;if(prefab==null)return;
            var community=new GameObject("Existing fictional community").transform;community.SetParent(root,false);
            foreach(var site in ExistingHomeSites)
            {
                // The reviewed catalog house is 12 x 9 m, offset (-2,-3) on its lot.
                // Face its entrance toward the road; level only the building footprint.
                var ring=new[]{new Point{x=site.x-4,z=site.y-1.5f},new Point{x=site.x+8,z=site.y-1.5f},new Point{x=site.x+8,z=site.y+7.5f},new Point{x=site.x-4,z=site.y+7.5f}};
                var layout=VisualLayout(new BuildActionLayout{x=site.x,z=site.y,yaw=180,foundation=ring,footprint=ring});
                var instance=Object.Instantiate(prefab,community);instance.name="Existing fictional home";
                instance.transform.localPosition=new Vector3(site.x,layout.groundY,site.y);
                instance.transform.localRotation=Quaternion.Euler(0,180,0);instance.transform.localScale=Vector3.one;
                // Keep the continuous landscape as the yard, rather than inserting a
                // raised square lawn. The driveway is rebuilt against this terrain.
                foreach(var renderer in instance.GetComponentsInChildren<MeshRenderer>())
                    if(renderer.name=="grass"||renderer.name=="path")renderer.enabled=false;
                GroundExistingGarden(instance,layout.groundY);
                BuildHomeFoundation(ring,layout.groundY,community);
                var start=instance.transform.TransformPoint(new Vector3(6,0,0));
                BuildHomeAccess(new Vector2(start.x,start.z),layout.groundY,community);
            }
        }

        void GroundExistingGarden(GameObject instance,float floor)
        {
            // Catalog meshes combine parts by material. This isolated garden region
            // contains the tree only; house, porch and metric building scale stay intact.
            var treePoint=instance.transform.TransformPoint(new Vector3(-9,0,10));
            float offset=Height(treePoint.x,treePoint.z)-floor;
            foreach(var filter in instance.GetComponentsInChildren<MeshFilter>())
            {
                if(filter.name=="grass"||filter.name=="path")continue;
                var source=filter.sharedMesh;var vertices=source.vertices;bool changed=false;
                for(int i=0;i<vertices.Length;i++)
                {
                    var p=instance.transform.InverseTransformPoint(filter.transform.TransformPoint(vertices[i]));
                    if(p.x< -13||p.x> -5||p.z<6||p.z>14)continue;
                    vertices[i]+=filter.transform.InverseTransformVector(Vector3.up*offset);changed=true;
                }
                if(!changed)continue;
                var mesh=Object.Instantiate(source);mesh.name="Grounded home garden "+source.name;
                mesh.vertices=vertices;mesh.RecalculateBounds();meshes.Add(mesh);filter.sharedMesh=mesh;
            }
        }

        void BuildHomeFoundation(Point[] ring,float floor,Transform parent)
        {
            var geometry=new Geometry();
            for(int edge=0;edge<4;edge++)
            {
                var a=ring[edge];var b=ring[(edge+1)%4];int steps=Mathf.CeilToInt(Vector2.Distance(new Vector2(a.x,a.z),new Vector2(b.x,b.z)));
                for(int j=0;j<steps;j++)
                {
                    var p=Vector2.Lerp(new Vector2(a.x,a.z),new Vector2(b.x,b.z),j/(float)steps);
                    var q=Vector2.Lerp(new Vector2(a.x,a.z),new Vector2(b.x,b.z),(j+1)/(float)steps);
                    Wall(geometry,new Vector3(p.x,Height(p.x,p.y)-.08f,p.y),new Vector3(p.x,floor+.12f,p.y),new Vector3(q.x,floor+.12f,q.y),new Vector3(q.x,Height(q.x,q.y)-.08f,q.y));
                }
            }
            MeshObject("Existing-home foundation",geometry,Mat(new Color(.42f,.40f,.32f)),parent);
        }

        void BuildHomeAccess(Vector2 start,float floor,Transform parent)
        {
            float distance=float.MaxValue;var finish=start;
            foreach(var path in world.features.Where(f=>f.kind=="path"))for(int i=1;i<path.points.Length;i++)
            {
                var a=new Vector2(path.points[i-1].x,path.points[i-1].z);var d=new Vector2(path.points[i].x,path.points[i].z)-a;
                var q=a+d*Mathf.Clamp01(Vector2.Dot(start-a,d)/Mathf.Max(.0001f,d.sqrMagnitude));
                float length=Vector2.Distance(start,q);if(length>=distance)continue;distance=length;finish=q;
            }
            if(distance==float.MaxValue||distance<.01f)return;
            var delta=finish-start;var side=new Vector2(-delta.y,delta.x).normalized*2;
            int steps=Mathf.CeilToInt(distance);var geometry=new Geometry();
            for(int i=0;i<=steps;i++)
            {
                var q=Vector2.Lerp(start,finish,i/(float)steps);
                foreach(int sign in new[]{-1,1})
                {
                    var v=q+side*sign;float ground=Height(v.x,v.y)+.08f;
                    float ramp=Mathf.Lerp(floor+.18f,ground,Mathf.Clamp01(i/4f));
                    geometry.vertices.Add(new Vector3(v.x,Mathf.Max(ground,ramp),v.y));geometry.normals.Add(Normal(v.x,v.y));
                }
                if(i==0)continue;int n=i*2;geometry.triangles.AddRange(new[]{n-2,n-1,n,n-1,n+1,n});
            }
            MeshObject("Existing-home road access",geometry,ProjectGround(true),parent);
        }
    }
}
