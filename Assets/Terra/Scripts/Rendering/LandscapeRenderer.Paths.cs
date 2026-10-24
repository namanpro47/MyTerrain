using System.Collections.Generic;
using Terra.Contracts;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
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
            if(pathMaterial==null)pathMaterial=new Material(Resources.Load<Shader>("TerraPath"));
            var go=new GameObject("Mapped route · illustrative surface");go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;go.AddComponent<MeshRenderer>().sharedMaterial=pathMaterial;
        }
    }
}
