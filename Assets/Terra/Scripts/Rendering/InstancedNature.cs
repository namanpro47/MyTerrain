using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terra.Rendering
{
    // Spatial batches keep the new foliage family shared in memory rather than copying every vertex per tree.
    public sealed partial class InstancedNature : MonoBehaviour
    {
        sealed class Batch { public Mesh mesh,medium,distant;public Material material,mediumMaterial,distantMaterial;public int submesh;public Matrix4x4[] transforms,animated;public Bounds bounds;public float maxHeight,detailExtent,cullPixels=2,highDetailPixels=42,mediumDetailPixels=18;public bool crop,groundcover,distantEmpty,bakedGrass; }
        readonly List<Batch> batches=new List<Batch>();
        readonly List<Material> ownedMaterials=new List<Material>();
        static int cameraFrame=-1;static bool cameraValid,orthographic;
        static Vector3 cameraPosition,cameraForward;static float pixelFactor,viewSpan;
        static readonly Plane[] viewPlanes=new Plane[6];
        public int LastSubmittedBatches{get;private set;}
        public int LastCulledGroundcoverBatches{get;private set;}
        public int LastCulledBatches{get;private set;}
#if UNITY_EDITOR
        // Matched render comparison: same geometry and LODs, with off-screen submission restored.
        public static bool SubmitOffscreenForComparison {get;set;}
#endif
        public static float DisplayPixelRatio{get;set;}=1;
        public bool ShadowsOnly {get;set;}
        float revealStarted=-1,revealDuration;
        void OnEnable(){NatureSubmission.Ensure();}
        public void Reveal(float duration){revealStarted=Time.unscaledTime;revealDuration=duration;foreach(var batch in batches)batch.animated=new Matrix4x4[batch.transforms.Length];}
        public void Add(GameObject prefab,List<Matrix4x4> transforms,Color tint,bool softGroundCover=false)
        {
            if(prefab==null||transforms.Count==0)return;
            var profile=prefab.GetComponent<NatureLodProfile>();
            var lodGroup=prefab.GetComponent<LODGroup>();
            var lods=lodGroup!=null?lodGroup.GetLODs():null;
            var filters=lods!=null&&lods.Length>0?System.Array.ConvertAll(lods[0].renderers,r=>r.GetComponent<MeshFilter>()):prefab.GetComponentsInChildren<MeshFilter>(true);
            bool hasOakLods=prefab.name.StartsWith("SolidOak");
            foreach(var filter in filters)
            {
                if(hasOakLods&&!filter.name.EndsWith("LOD0"))continue;
                var medium=hasOakLods?System.Array.Find(filters,f=>f.name==filter.name.Replace("LOD0","LOD1"))?.sharedMesh:null;
                var distant=hasOakLods?System.Array.Find(filters,f=>f.name==filter.name.Replace("LOD0","LOD2"))?.sharedMesh:null;
                // Baked forest assets align renderer/material slots across all three levels.
                // Submit one level per batch; never traverse and draw every LOD together.
                if(lods!=null&&lods.Length>1)
                {
                    int part=System.Array.IndexOf(filters,filter);
                    medium=lods[1].renderers[part].GetComponent<MeshFilter>().sharedMesh;
                    if(lods.Length>2)distant=lods[2].renderers[part].GetComponent<MeshFilter>().sharedMesh;
                }
                // Baked reductions of the same source grass retain full geometry close up.
                // Keep this explicit: unreviewed plants must not silently acquire simplified LODs.
                bool grassLods=prefab.name=="Grass_Common_Short"||prefab.name=="Grass_Wispy_Short"||prefab.name=="Grass_Common_Tall";
                if(grassLods)
                {
                    medium=Resources.Load<Mesh>("NatureLods/"+filter.sharedMesh.name+"-lod1");
                    distant=Resources.Load<Mesh>("NatureLods/"+filter.sharedMesh.name+"-lod2");
                }
                var materials=filter.GetComponent<MeshRenderer>().sharedMaterials;
                var local=prefab.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
                for(int sub=0;sub<filter.sharedMesh.subMeshCount;sub++)
                {
                    var sourceMaterial=materials[Mathf.Min(sub,materials.Length-1)];
                    var material=BorrowMaterial(sourceMaterial,tint,softGroundCover,filter.sharedMesh.bounds,
                        tint!=Color.white&&prefab.name.StartsWith("Grass"));
                    var mediumMaterial=material;var distantMaterial=material;
                    if(lods!=null&&lods.Length>1)
                    {
                        int part=System.Array.IndexOf(filters,filter);
                        Material LevelMaterial(int level)
                        {
                            var source=lods[level].renderers[part].sharedMaterials[sub];
                            if(source==sourceMaterial)return material;
                            return BorrowMaterial(source,Color.white,false,default,false);
                        }
                        mediumMaterial=LevelMaterial(1);
                        if(lods.Length>2)distantMaterial=LevelMaterial(2);
                    }
                    // Dense crop clusters use larger batches; woodland retains smaller spatial groups.
                    // RenderMeshInstanced stores both transform matrices: stay below its default 511-instance limit.
                    int batchSize=(softGroundCover||prefab.name=="GeometricWheat"||prefab.name=="FieldCluster")?500:128;
                    for(int start=0;start<transforms.Count;start+=batchSize)
                    {
                        var matrices=new Matrix4x4[Mathf.Min(batchSize,transforms.Count-start)];
                        for(int i=0;i<matrices.Length;i++)matrices[i]=transforms[start+i]*local;
                        var localBounds=filter.sharedMesh.bounds;
                        if(medium!=null)localBounds.Encapsulate(medium.bounds);if(distant!=null)localBounds.Encapsulate(distant.bounds);
                        var bounds=TransformBounds(localBounds,matrices[0]);float maxHeight=bounds.size.y;
                        for(int i=1;i<matrices.Length;i++){var plantBounds=TransformBounds(localBounds,matrices[i]);bounds.Encapsulate(plantBounds);maxHeight=Mathf.Max(maxHeight,plantBounds.size.y);}
                        float detailExtent=maxHeight;
                        if(profile!=null&&profile.includeWidth)
                        {
                            foreach(var matrix in matrices)
                            {
                                var scale=matrix.lossyScale;
                                detailExtent=Mathf.Max(detailExtent,localBounds.size.x*Mathf.Abs(scale.x),localBounds.size.z*Mathf.Abs(scale.z));
                            }
                        }
                        batches.Add(new Batch{detailExtent=detailExtent,cullPixels=profile!=null?profile.cullPixels:2,mesh=filter.sharedMesh,medium=medium,distant=distant,submesh=sub,material=material,mediumMaterial=mediumMaterial,distantMaterial=distantMaterial,transforms=matrices,bounds=bounds,maxHeight=maxHeight,crop=prefab.name=="FieldCluster",groundcover=softGroundCover,bakedGrass=grassLods,highDetailPixels=profile!=null?profile.nearPixels:grassLods?20:lods!=null?120:42,mediumDetailPixels=profile!=null?profile.farPixels:grassLods?8:lods!=null?50:18,distantEmpty=distant!=null&&distant.GetIndexCount(sub)==0});
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
                    GeometryUtility.CalculateFrustumPlanes(camera,viewPlanes);
                    var transform=camera.transform;cameraPosition=transform.position;cameraForward=transform.forward;
                    orthographic=camera.orthographic;pixelFactor=camera.pixelHeight/(2*Mathf.Max(1,DisplayPixelRatio));
                    viewSpan=orthographic?camera.orthographicSize:Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad);
                }
            }
            LastSubmittedBatches=0;LastCulledGroundcoverBatches=0;LastCulledBatches=0;
            bool checkBounds=cameraValid&&!ShadowsOnly;
#if UNITY_EDITOR
            checkBounds&=!SubmitOffscreenForComparison;
#endif
            foreach(var b in batches)
            {
                if(checkBounds)
                {
                    // Skip submissions that cannot affect the view. The broad margin retains
                    // off-screen tree shadows on slopes under the fixed 52-degree sun.
                    // It includes eight plant heights plus terrain relief and 16 metres.
                    // Geometry, material and distance-level selection remain unchanged.
                    var shadowBounds=b.bounds;
                    shadowBounds.Expand(b.maxHeight*8+b.bounds.size.y*2+16);
                    if(!GeometryUtility.TestPlanesAABB(viewPlanes,shadowBounds))
                    {LastCulledBatches++;if(b.groundcover)LastCulledGroundcoverBatches++;continue;}
                }
                float pixels=float.MaxValue;
                if(cameraValid&&(b.crop||b.groundcover||b.medium!=null))
                {
                    // RenderMeshInstanced retains its native bounds/frustum and shadow culling.
                    // Use the nearest part of a spatial batch, retaining detail conservatively at its near edge.
                    float depth=Mathf.Max(1,Vector3.Dot(b.bounds.center-cameraPosition,cameraForward)-b.bounds.extents.magnitude);
                    pixels=b.detailExtent*pixelFactor/(orthographic?viewSpan:depth*viewSpan);
                    // The crop shader fades individual stalks before this conservative batch cutoff.
                    if(b.crop&&pixels<=.6f||b.groundcover&&pixels<=1.5f||b.medium!=null&&!b.bakedGrass&&pixels<b.cullPixels)continue;
                }
                bool far=pixels<b.mediumDetailPixels&&b.distant!=null,mid=!far&&pixels<b.highDetailPixels&&b.medium!=null;
                if(far&&b.distantEmpty)continue;
                var mesh=far?b.distant:mid?b.medium:b.mesh;
                var material=far?b.distantMaterial:mid?b.mediumMaterial:b.material;
                var parameters=new RenderParams(material){worldBounds=b.bounds,shadowCastingMode=ShadowsOnly?ShadowCastingMode.ShadowsOnly:ShadowCastingMode.On,receiveShadows=true};
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
                NatureSubmission.Submit(parameters,mesh,b.submesh,matrices);
                LastSubmittedBatches++;
            }
            if(revealStarted>=0&&Time.unscaledTime-revealStarted>revealDuration+.2f)revealStarted=-1;
        }
        void OnDestroy()
        {
            foreach(var entry in borrowedMaterials.Values)ReleaseMaterial(entry);
            borrowedMaterials.Clear();
            foreach(var material in ownedMaterials)Destroy(material);
        }
    }
}
