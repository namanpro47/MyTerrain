using System.Collections.Generic;
using Terra.Core;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        // Grade only the outer three metres of the authored lot. Buildings remain at
        // their metric dimensions and level elevation; analytical terrain is untouched.
        void GradeProjectEdges(GameObject instance,BuildActionLayout layout)
        {
            float left=layout.foundation[0].x,right=layout.foundation[2].x;
            float bottom=layout.foundation[0].z,top=layout.foundation[2].z;
            float Drop(float x,float z)
            {
                float edge=Mathf.Min(Mathf.Min(x-left,right-x),Mathf.Min(z-bottom,top-z));
                float blend=1-Mathf.SmoothStep(0,1,Mathf.Clamp01(edge/3));
                return Mathf.Max(0,layout.groundY-Height(x,z)-.025f)*blend;
            }
            foreach(var filter in instance.GetComponentsInChildren<MeshFilter>())
            {
                if(filter.name!="grass"&&filter.name!="path"&&filter.name!="asphalt")continue;
                var source=filter.sharedMesh;var points=source.vertices;var indices=source.triangles;
                var geometry=new Geometry();var transform=filter.transform;
                void Emit(List<Vector3> polygon,Vector3 normal)
                {
                    if(polygon.Count<3)return;
                    int start=geometry.vertices.Count;
                    foreach(var p in polygon)
                    {
                        var q=p;q.y-=Drop(p.x,p.z);
                        var n=normal;
                        if(normal.y>.95f)
                        {
                            const float d=.1f;
                            n=new Vector3((Drop(p.x+d,p.z)-Drop(p.x-d,p.z))/(2*d),1,
                                (Drop(p.x,p.z+d)-Drop(p.x,p.z-d))/(2*d)).normalized;
                        }
                        geometry.vertices.Add(transform.InverseTransformPoint(q));
                        geometry.normals.Add(transform.InverseTransformDirection(n));
                    }
                    for(int i=1;i<polygon.Count-1;i++)
                        if(Vector3.Cross(geometry.vertices[start+i]-geometry.vertices[start],geometry.vertices[start+i+1]-geometry.vertices[start]).sqrMagnitude>1e-10f)
                            geometry.triangles.AddRange(new[]{start,start+i,start+i+1});
                }
                for(int i=0;i<indices.Length;i+=3)
                {
                    var a=transform.TransformPoint(points[indices[i]]);
                    var b=transform.TransformPoint(points[indices[i+1]]);
                    var c=transform.TransformPoint(points[indices[i+2]]);
                    var normal=Vector3.Cross(b-a,c-a).normalized;
                    var triangle=new List<Vector3>{a,b,c};
                    float minX=Mathf.Min(a.x,b.x,c.x),maxX=Mathf.Max(a.x,b.x,c.x);
                    float minZ=Mathf.Min(a.z,b.z,c.z),maxZ=Mathf.Max(a.z,b.z,c.z);
                    if(minX>=left+3&&maxX<=right-3&&minZ>=bottom+3&&maxZ<=top-3)
                    {Emit(triangle,normal);continue;}
                    // A common two-metre grid gives adjoining triangles the same edge
                    // samples, without recursively tessellating the whole model.
                    int x0=Mathf.FloorToInt((minX-left)/2),x1=Mathf.FloorToInt((maxX-left)/2);
                    int z0=Mathf.FloorToInt((minZ-bottom)/2),z1=Mathf.FloorToInt((maxZ-bottom)/2);
                    for(int x=x0;x<=x1;x++)
                    {
                        var strip=ClipProjectEdge(ClipProjectEdge(triangle,0,left+x*2,true),0,left+(x+1)*2,false);
                        if(strip.Count<3)continue;
                        for(int z=z0;z<=z1;z++)
                            Emit(ClipProjectEdge(ClipProjectEdge(strip,2,bottom+z*2,true),2,bottom+(z+1)*2,false),normal);
                    }
                }
                var mesh=new Mesh{name="Graded project "+source.name,indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};
                mesh.SetVertices(geometry.vertices);mesh.SetNormals(geometry.normals);mesh.SetTriangles(geometry.triangles,0);mesh.RecalculateBounds();
                meshes.Add(mesh);filter.sharedMesh=mesh;
            }
        }

        static List<Vector3> ClipProjectEdge(List<Vector3> polygon,int axis,float value,bool lower)
        {
            var result=new List<Vector3>();if(polygon.Count==0)return result;
            var previous=polygon[polygon.Count-1];float pd=lower?previous[axis]-value:value-previous[axis];
            foreach(var current in polygon)
            {
                float cd=lower?current[axis]-value:value-current[axis];
                if((pd>=0)!=(cd>=0))result.Add(Vector3.Lerp(previous,current,pd/(pd-cd)));
                if(cd>=0)result.Add(current);previous=current;pd=cd;
            }
            return result;
        }
    }
}
