using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terra.Rendering
{
    // An alternative art study: original painted silhouettes, rooted in the same 3D landscape.
    public sealed class PaintedCanopy : MonoBehaviour
    {
        sealed class Batch {public Mesh mesh;public Matrix4x4[] transforms;}
        readonly List<Batch> batches=new List<Batch>();
        readonly List<Mesh> ownedMeshes=new List<Mesh>();
        Material material;
        public bool Understory {get;set;}
        public void Add(List<Matrix4x4> transforms,int variant)
        {
            if(transforms.Count==0)return;
            // A referenced instanced material keeps the required variant in player exports.
            if(material==null)material=new Material(Resources.Load<Material>(Understory?"PaintedUnderstoryMaterial":"PaintedTreeMaterial"));
            // UVs and root pivots follow the inspected atlas, which has intentionally unequal tree silhouettes.
            bool left=variant%2==0,top=variant<2;
            float u0=left?0:.5375f,u1=left?.5375f:1,v0=top?.525f:0,v1=top?1:.525f;
            float pivot=variant==0?.52f:variant==1?.48f:variant==2?.645f:.51f;
            float width=7*(u1-u0)/(v1-v0),height=7,rootOffset=top?.15f:.65f;
            if(Understory)
            {
                u0=left?0:.56f;u1=left?.56f:1;v0=top?.5f:0;v1=top?1:.5f;
                height=2.1f;width=height*(u1-u0)/(v1-v0);pivot=.5f;rootOffset=top?.16f:.40f;
            }
            var mesh=new Mesh{name="Painted tree variant "+variant};
            mesh.vertices=new[]{new Vector3(-pivot*width,-rootOffset,0),new Vector3((1-pivot)*width,-rootOffset,0),new Vector3(-pivot*width,height-rootOffset,0),new Vector3((1-pivot)*width,height-rootOffset,0)};
            mesh.uv=new[]{new Vector2(u0,v0),new Vector2(u1,v0),new Vector2(u0,v1),new Vector2(u1,v1)};mesh.triangles=new[]{0,2,1,1,2,3};
            mesh.bounds=new Bounds(new Vector3(0,3.5f,0),new Vector3(16,9,16));ownedMeshes.Add(mesh);
            for(int start=0;start<transforms.Count;start+=128)
            {
                int count=Mathf.Min(128,transforms.Count-start);var matrices=new Matrix4x4[count];transforms.CopyTo(start,matrices,0,count);batches.Add(new Batch{mesh=mesh,transforms=matrices});
            }
        }
        void LateUpdate()
        {
            var camera=Camera.main;if(camera==null)return;
            Shader.SetGlobalVector("_PaintedTreeRight",camera.transform.right);
            foreach(var b in batches)Graphics.RenderMeshInstanced(new RenderParams(material){shadowCastingMode=ShadowCastingMode.Off,receiveShadows=true},b.mesh,0,b.transforms);
        }
        void OnDestroy(){if(material!=null)Destroy(material);foreach(var mesh in ownedMeshes)Destroy(mesh);}
    }
}
