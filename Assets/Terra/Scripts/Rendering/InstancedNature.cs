using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terra.Rendering
{
    // Spatial batches keep the new foliage family shared in memory rather than copying every vertex per tree.
    public sealed class InstancedNature : MonoBehaviour
    {
        sealed class Batch { public Mesh mesh;public Material material;public int submesh;public Matrix4x4[] transforms,animated; }
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
                    if(material.name.Contains("Leaves")){material.shader=Resources.Load<Shader>("TerraFoliage");if(tint!=Color.white)material.color=tint;if(prefab.name.StartsWith("CommonTree")||prefab.name.StartsWith("Bush")){material.SetTexture("_BaseMap",Resources.Load<Texture2D>("LeafPaint"));material.SetFloat("_Painted",1);}material.EnableKeyword("_ALPHATEST_ON");var bounds=filter.sharedMesh.bounds;material.SetVector("_CrownCenter",bounds.center+Vector3.up*bounds.extents.y*.3f);}
                    ownedMaterials.Add(material);
                    // At most 128 instances; input groups are spatial so offscreen woodland is culled as small clusters.
                    for(int start=0;start<transforms.Count;start+=128)
                    {
                        var matrices=new Matrix4x4[Mathf.Min(128,transforms.Count-start)];
                        for(int i=0;i<matrices.Length;i++)matrices[i]=transforms[start+i]*local;
                        batches.Add(new Batch{mesh=filter.sharedMesh,submesh=sub,material=material,transforms=matrices});
                    }
                }
            }
        }
        void LateUpdate()
        {
            foreach(var b in batches)
            {
                var parameters=new RenderParams(b.material){shadowCastingMode=ShadowsOnly?ShadowCastingMode.ShadowsOnly:ShadowCastingMode.On,receiveShadows=true};
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
