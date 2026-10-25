using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terra.Rendering
{
    // Spatial batches keep the new foliage family shared in memory rather than copying every vertex per tree.
    public sealed class InstancedNature : MonoBehaviour
    {
        sealed class Batch { public Mesh mesh,medium,distant;public Material material;public int submesh;public Matrix4x4[] transforms,animated;public Bounds bounds;public float maxHeight;public bool crop,groundcover; }
        readonly List<Batch> batches=new List<Batch>();
        readonly List<Material> ownedMaterials=new List<Material>();
        static int cameraFrame=-1;static bool cameraValid,orthographic;
        static Vector3 cameraPosition,cameraForward;static float pixelFactor,viewSpan;
        public static float DisplayPixelRatio{get;set;}=1;
        public bool ShadowsOnly {get;set;}
        float revealStarted=-1,revealDuration;
        public void Reveal(float duration){revealStarted=Time.unscaledTime;revealDuration=duration;foreach(var batch in batches)batch.animated=new Matrix4x4[batch.transforms.Length];}
        public void Add(GameObject prefab,List<Matrix4x4> transforms,Color tint,bool softGroundCover=false)
        {
            if(prefab==null||transforms.Count==0)return;
            var filters=prefab.GetComponentsInChildren<MeshFilter>(true);
            bool hasOakLods=prefab.name.StartsWith("SolidOak");
            foreach(var filter in filters)
            {
                if(hasOakLods&&!filter.name.EndsWith("LOD0"))continue;
                var medium=hasOakLods?System.Array.Find(filters,f=>f.name==filter.name.Replace("LOD0","LOD1"))?.sharedMesh:null;
                var distant=hasOakLods?System.Array.Find(filters,f=>f.name==filter.name.Replace("LOD0","LOD2"))?.sharedMesh:null;
                var materials=filter.GetComponent<MeshRenderer>().sharedMaterials;
                var local=prefab.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                for(int sub=0;sub<filter.sharedMesh.subMeshCount;sub++)
                {
                    var sourceMaterial=materials[Mathf.Min(sub,materials.Length-1)];
                    var material=new Material(softGroundCover?Resources.Load<Material>("Nature/Groundcover"):sourceMaterial);
                    if(softGroundCover)
                    {
                        material.SetTexture("_BaseMap",sourceMaterial.mainTexture);
                        material.SetColor("_BaseColor",tint);
                        material.SetFloat("_Cutoff",.5f);
                        var plantBounds=filter.sharedMesh.bounds;
                        material.SetVector("_PlantBounds",new Vector4(plantBounds.min.y,plantBounds.max.y,0,0));
                    }
                    material.enableInstancing=true;
                    if(tint!=Color.white&&prefab.name.StartsWith("Grass"))material.color=tint;
                    ownedMaterials.Add(material);
                    // Dense crop clusters use larger batches; woodland retains smaller spatial groups.
                    // RenderMeshInstanced stores both transform matrices: stay below its default 511-instance limit.
                    int batchSize=(prefab.name=="GeometricWheat"||prefab.name=="FieldCluster")?500:128;
                    for(int start=0;start<transforms.Count;start+=batchSize)
                    {
                        var matrices=new Matrix4x4[Mathf.Min(batchSize,transforms.Count-start)];
                        for(int i=0;i<matrices.Length;i++)matrices[i]=transforms[start+i]*local;
                        var localBounds=filter.sharedMesh.bounds;
                        if(medium!=null)localBounds.Encapsulate(medium.bounds);if(distant!=null)localBounds.Encapsulate(distant.bounds);
                        var bounds=TransformBounds(localBounds,matrices[0]);float maxHeight=bounds.size.y;
                        for(int i=1;i<matrices.Length;i++){var plantBounds=TransformBounds(localBounds,matrices[i]);bounds.Encapsulate(plantBounds);maxHeight=Mathf.Max(maxHeight,plantBounds.size.y);}
                        batches.Add(new Batch{mesh=filter.sharedMesh,medium=medium,distant=distant,submesh=sub,material=material,transforms=matrices,bounds=bounds,maxHeight=maxHeight,crop=prefab.name=="FieldCluster",groundcover=softGroundCover});
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
            if(cameraFrame!=Time.frameCount)
            {
                cameraFrame=Time.frameCount;var camera=Camera.main;cameraValid=camera!=null;
                if(cameraValid)
                {
                    var transform=camera.transform;cameraPosition=transform.position;cameraForward=transform.forward;
                    orthographic=camera.orthographic;pixelFactor=camera.pixelHeight/(2*Mathf.Max(1,DisplayPixelRatio));
                    viewSpan=orthographic?camera.orthographicSize:Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad);
                }
            }
            foreach(var b in batches)
            {
                float pixels=float.MaxValue;
                if(cameraValid&&(b.crop||b.groundcover||b.medium!=null))
                {
                    // RenderMeshInstanced retains its native bounds/frustum and shadow culling.
                    // Use the nearest part of a spatial batch, retaining detail conservatively at its near edge.
                    float depth=Mathf.Max(1,Vector3.Dot(b.bounds.center-cameraPosition,cameraForward)-b.bounds.extents.magnitude);
                    pixels=b.maxHeight*pixelFactor/(orthographic?viewSpan:depth*viewSpan);
                    // The crop shader fades individual stalks before this conservative batch cutoff.
                    if(b.crop&&pixels<=2.4f||b.groundcover&&pixels<=1.5f)continue;
                }
                var mesh=pixels<18&&b.distant!=null?b.distant:pixels<42&&b.medium!=null?b.medium:b.mesh;
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
                Graphics.RenderMeshInstanced(parameters,mesh,b.submesh,matrices);
            }
            if(revealStarted>=0&&Time.unscaledTime-revealStarted>revealDuration+.2f)revealStarted=-1;
        }
        void OnDestroy(){foreach(var material in ownedMaterials)Destroy(material);}
    }
}
