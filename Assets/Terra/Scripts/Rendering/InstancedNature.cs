using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terra.Rendering
{
    // Spatial batches keep the new foliage family shared in memory rather than copying every vertex per tree.
    public sealed class InstancedNature : MonoBehaviour
    {
        sealed class Batch { public Mesh mesh;public Material material;public int submesh;public Matrix4x4[] transforms,animated;public Bounds bounds; }
        readonly List<Batch> batches=new List<Batch>();
        readonly List<Material> ownedMaterials=new List<Material>();
        public bool ShadowsOnly {get;set;}
        float revealStarted=-1,revealDuration;
        public void Reveal(float duration){revealStarted=Time.unscaledTime;revealDuration=duration;foreach(var batch in batches)batch.animated=new Matrix4x4[batch.transforms.Length];}
        public void Add(GameObject prefab,List<Matrix4x4> transforms,Color tint)
        {
            if(prefab==null||transforms.Count==0)return;
            foreach(var filter in prefab.GetComponentsInChildren<MeshFilter>())
            {
                var materials=filter.GetComponent<MeshRenderer>().sharedMaterials;
                var local=prefab.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                for(int sub=0;sub<filter.sharedMesh.subMeshCount;sub++)
                {
                    var material=new Material(materials[Mathf.Min(sub,materials.Length-1)]);material.enableInstancing=true;
                    if(tint!=Color.white&&prefab.name.StartsWith("Grass"))material.color=tint;
                    ownedMaterials.Add(material);
                    // Dense crop clusters use larger batches; woodland retains smaller spatial groups.
                    // RenderMeshInstanced stores both transform matrices: stay below its default 511-instance limit.
                    int batchSize=(prefab.name=="GeometricWheat"||prefab.name=="FieldCluster")?500:128;
                    for(int start=0;start<transforms.Count;start+=batchSize)
                    {
                        var matrices=new Matrix4x4[Mathf.Min(batchSize,transforms.Count-start)];
                        for(int i=0;i<matrices.Length;i++)matrices[i]=transforms[start+i]*local;
                        var bounds=TransformBounds(filter.sharedMesh.bounds,matrices[0]);
                        for(int i=1;i<matrices.Length;i++)bounds.Encapsulate(TransformBounds(filter.sharedMesh.bounds,matrices[i]));
                        batches.Add(new Batch{mesh=filter.sharedMesh,submesh=sub,material=material,transforms=matrices,bounds=bounds});
                    }
                }
            }
        }
        static Bounds TransformBounds(Bounds local,Matrix4x4 matrix)
        {
            var e=local.extents;
            var x=matrix.MultiplyVector(new Vector3(e.x,0,0));
            var y=matrix.MultiplyVector(new Vector3(0,e.y,0));
            var z=matrix.MultiplyVector(new Vector3(0,0,e.z));
            var extent=new Vector3(Mathf.Abs(x.x)+Mathf.Abs(y.x)+Mathf.Abs(z.x),Mathf.Abs(x.y)+Mathf.Abs(y.y)+Mathf.Abs(z.y),Mathf.Abs(x.z)+Mathf.Abs(y.z)+Mathf.Abs(z.z));
            return new Bounds(matrix.MultiplyPoint3x4(local.center),extent*2);
        }
        void LateUpdate()
        {
            foreach(var b in batches)
            {
                var parameters=new RenderParams(b.material){worldBounds=b.bounds,shadowCastingMode=ShadowsOnly?ShadowCastingMode.ShadowsOnly:ShadowCastingMode.On,receiveShadows=true};
                var matrices=b.transforms;
                if(revealStarted>=0)
                {
                    for(int i=0;i<b.transforms.Length;i++)
                    {
                        var position=b.transforms[i].GetColumn(3);
                        float delay=Mathf.Repeat(position.x*.013f+position.z*.021f,1)*.20f;
                        float t=Mathf.Clamp01((Time.unscaledTime-revealStarted-delay)/revealDuration);
                        float growth=Mathf.Lerp(.04f,1,Mathf.SmoothStep(0,1,t));
                        b.animated[i]=b.transforms[i]*Matrix4x4.Scale(new Vector3(growth,growth,growth));
                    }
                    matrices=b.animated;
                }
                Graphics.RenderMeshInstanced(parameters,b.mesh,b.submesh,matrices);
            }
            if(revealStarted>=0&&Time.unscaledTime-revealStarted>revealDuration+.2f)revealStarted=-1;
        }
        void OnDestroy(){foreach(var material in ownedMaterials)Destroy(material);}
    }
}
