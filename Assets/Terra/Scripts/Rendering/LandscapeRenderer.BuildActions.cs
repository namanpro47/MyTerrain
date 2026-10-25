using System.Collections.Generic;
using Terra.Art;
using Terra.Core;
using Terra.Contracts;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        BuildActionCatalog buildCatalog;
        BuildActionRules buildRules;
        public void ConfigureBuildActions(BuildActionCatalog catalog,BuildActionRules rules){buildCatalog=catalog;buildRules=rules;}

        bool BuildCatalogAction(Site site,string id,Transform parent,bool preview)
        {
            var action=buildRules?.Find(id);if(action==null)return false;
            var entry=buildCatalog.Find(action.artId);var layout=buildRules.Layout(site,action);
            if(entry?.prefab==null||!layout.Valid)return false;
            var instance=Object.Instantiate(entry.prefab,parent);instance.name=entry.title+(preview?" preview":"");
            instance.transform.localPosition=new Vector3(layout.x,action.landscape?0:layout.groundY,layout.z);
            instance.transform.localRotation=Quaternion.Euler(0,layout.yaw,0);instance.transform.localScale=Vector3.one;
            if(action.landscape)
            {
                Patch("Managed ground",site.polygon,action.category=="Farming"?new Color(.43f,.35f,.22f):new Color(.45f,.57f,.33f),.09f,parent);
                // A metric sample illustrates each landscape practice; conform its vertices to the
                // actual terrain instead of making a floating flat board or stretching a building.
                foreach(var filter in instance.GetComponentsInChildren<MeshFilter>())
                {
                    var mesh=Object.Instantiate(filter.sharedMesh);meshes.Add(mesh);var vertices=mesh.vertices;
                    for(int i=0;i<vertices.Length;i++){var worldPoint=filter.transform.TransformPoint(vertices[i]);worldPoint.y+=Height(worldPoint.x,worldPoint.z)+.12f;vertices[i]=filter.transform.InverseTransformPoint(worldPoint);}
                    mesh.vertices=vertices;mesh.RecalculateBounds();mesh.RecalculateNormals();filter.sharedMesh=mesh;
                }
            }
            else
            {
                // A level foundation with terrain-following sides fills the bounded gap. Existing
                // terrain heights stay intact; the rules reject >3 m fill and protected land.
                var geometry=new Geometry();var ring=layout.foundation;
                for(int edge=0;edge<4;edge++)
                {
                    var a=ring[edge];var b=ring[(edge+1)%4];int steps=Mathf.CeilToInt(Vector2.Distance(new Vector2(a.x,a.z),new Vector2(b.x,b.z))/2);
                    for(int j=0;j<steps;j++)
                    {
                        var p=Vector2.Lerp(new Vector2(a.x,a.z),new Vector2(b.x,b.z),j/(float)steps);var q=Vector2.Lerp(new Vector2(a.x,a.z),new Vector2(b.x,b.z),(j+1)/(float)steps);int n=geometry.vertices.Count;
                        geometry.vertices.AddRange(new[]{new Vector3(p.x,Height(p.x,p.y)-.04f,p.y),new Vector3(q.x,Height(q.x,q.y)-.04f,q.y),new Vector3(q.x,layout.groundY+.06f,q.y),new Vector3(p.x,layout.groundY+.06f,p.y)});
                        var normal=Vector3.Cross(Vector3.up,geometry.vertices[n+1]-geometry.vertices[n]).normalized;for(int k=0;k<4;k++)geometry.normals.Add(normal);
                        geometry.triangles.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});
                    }
                }
                MeshObject("Level foundation",geometry,Mat(new Color(.52f,.48f,.38f)),parent);
            }
            if(preview)
            {
                var go=new GameObject("Actual occupied footprint");go.transform.SetParent(parent,false);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=BoundaryMat(new Color(1,.84f,.36f));line.useWorldSpace=true;line.loop=true;line.positionCount=4;line.startWidth=line.endWidth=.5f;
                for(int i=0;i<4;i++){var p=layout.footprint[i];line.SetPosition(i,new Vector3(p.x,action.landscape?Height(p.x,p.z)+.4f:layout.groundY+.3f,p.z));}
            }
            return true;
        }
    }
}
