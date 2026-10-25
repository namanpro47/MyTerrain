using System;
using System.Collections;
using Terra.Contracts;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;

namespace Terra.Gameplay
{
    [Serializable] public sealed class CommunityPlace
    { public string id,name,region,imageYear; }
    [Serializable] public sealed class CommunityCatalog
    { public int schemaVersion; public CommunityPlace[] places; }
    [Serializable] public sealed class CommunityPack
    {
        public int schemaVersion,imageWidth,imageHeight;
        public string id,name,region,status,crs,imageYear,imageryKind,attribution,limitations,rasterRows,imageRows;
        public float extentM,originElevationM,imageMetersPerPixel;
        public double latitude,longitude;
        public TerrainGrid terrain;
    }

    // Additive atlas companion: photo/terrain study only; never touches the annual session or saves.
    public sealed class CommunityTerrain : MonoBehaviour
    {
        public static CommunityTerrain Instance {get;private set;}
        public CommunityPack Pack {get;private set;}
        public string Error {get;private set;}
        public bool Loading {get;private set;}
        public Mesh TerrainMesh {get;private set;}
        public Texture2D SourceImage {get;private set;}
        public RenderTexture Preview {get;private set;}
        Camera terrainCamera; GameObject surface; Material material;
        Transform stage; float yaw=0,pitch=48,zoom=1;
        WorldAtlas installed;
        Coroutine loading;
        UnityEngine.UI.RawImage display; TextMeshProUGUI status;
        bool sourceView;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Bootstrap()
        {
            if(Instance!=null)return;
            var go=new GameObject("Community import companion");
            DontDestroyOnLoad(go);go.AddComponent<CommunityTerrain>();
        }
        void Awake(){Instance=this;}
        void Update()
        {
            if(WorldAtlas.Instance!=null&&installed!=WorldAtlas.Instance)
            {installed=WorldAtlas.Instance;installed.InstallCommunityBrowser(this);}
            if(installed!=null)installed.TickCommunityBrowser();
            if(terrainCamera!=null)terrainCamera.enabled=display!=null&&display.gameObject.activeInHierarchy&&!sourceView;
        }
        public static string Root
        {
            get
            {
                string path=Application.streamingAssetsPath+"/Communities/";
#if !UNITY_WEBGL || UNITY_EDITOR
                path="file://"+path;
#endif
                return path;
            }
        }
        public IEnumerator ReadCatalog(Action<CommunityCatalog> done,Action<string> fail)
        {
            using(var request=UnityWebRequest.Get(Root+"catalog.json"))
            {
                request.timeout=20;yield return request.SendWebRequest();
                if(request.result!=UnityWebRequest.Result.Success){fail("Prepared communities are unavailable. Please return to the globe and try again.");yield break;}
                CommunityCatalog catalog=null;
                try {catalog=JsonUtility.FromJson<CommunityCatalog>(request.downloadHandler.text);if(catalog.schemaVersion!=1||catalog.places==null)throw new Exception("Unsupported catalog");}
                catch(Exception e){fail(e.Message);yield break;}
                done(catalog);
            }
        }
        public void Load(CommunityPlace place,UnityEngine.UI.RawImage image,TextMeshProUGUI label)
        {
            Cancel();Clear();display=image;status=label;Loading=true;Error=null;sourceView=false;
            status.text="Loading the prepared imagery and elevation…";
            loading=StartCoroutine(LoadPack(place));
        }
        public void Cancel(){if(loading!=null)StopCoroutine(loading);loading=null;Loading=false;}
        IEnumerator LoadPack(CommunityPlace place)
        {
            if(!System.Text.RegularExpressions.Regex.IsMatch(place.id,"^[a-z0-9]+(?:-[a-z0-9]+)*$"))
            {Fail("Invalid community ID");yield break;}
            CommunityPack pack=null;
            using(var request=UnityWebRequest.Get(Root+place.id+"/community.json"))
            {
                request.timeout=20;yield return request.SendWebRequest();
                if(request.result!=UnityWebRequest.Result.Success){Fail("The terrain could not load. Choose the community again to retry.");yield break;}
                try {pack=JsonUtility.FromJson<CommunityPack>(request.downloadHandler.text);Validate(pack);if(pack.id!=place.id)throw new Exception("Community identity mismatch");}
                catch(Exception e){Fail(e.Message);yield break;}
            }
            using(var request=UnityWebRequest.Get(Root+place.id+"/imagery.jpg"))
            {
                request.timeout=20;yield return request.SendWebRequest();
                if(request.result!=UnityWebRequest.Result.Success){Fail("The source image could not load. Choose the community again to retry.");yield break;}
                SourceImage=new Texture2D(2,2,TextureFormat.RGB24,true);
                if(!SourceImage.LoadImage(request.downloadHandler.data)){Fail("The source image is not a valid JPEG.");Clear();yield break;}
            }
            if(SourceImage.width!=pack.imageWidth||SourceImage.height!=pack.imageHeight){Fail("Image dimensions do not match the terrain manifest.");Clear();yield break;}
            SourceImage.wrapMode=TextureWrapMode.Clamp;SourceImage.anisoLevel=8;
            Pack=pack;Build();Loading=false;loading=null;ShowTerrain();
            Debug.Log("TERRA_COMMUNITY_READY: "+Pack.id);
        }
        void Fail(string message){Error=message;Loading=false;loading=null;if(status!=null)status.text=message;}
        public static void Validate(CommunityPack p)
        {
            if(p==null||p.schemaVersion!=1||p.status!="terrain-study"||p.rasterRows!="south-to-north"||p.imageRows!="north-to-south")throw new ArgumentException("Unsupported community pack");
            var t=p.terrain;
            if(t==null||t.rows<2||t.rows>257||t.columns!=t.rows||t.heights==null||t.heights.Length!=t.rows*t.columns)throw new ArgumentException("Incomplete elevation grid");
            foreach(float value in new[]{t.cellSizeM,t.xMin,t.zMin,p.extentM})if(float.IsNaN(value)||float.IsInfinity(value))throw new ArgumentException("Non-finite geography metadata");
            if(float.IsNaN(p.extentM)||float.IsInfinity(p.extentM)||p.extentM<400||p.extentM>2000||Math.Abs(t.cellSizeM*(t.rows-1)-p.extentM)>.01f||Math.Abs(t.xMin+p.extentM/2)>.01f||Math.Abs(t.zMin+p.extentM/2)>.01f)throw new ArgumentException("Image/terrain extent mismatch");
            if(p.imageWidth<512||p.imageWidth>4000||p.imageWidth!=p.imageHeight)throw new ArgumentException("Invalid image size");
            foreach(float h in t.heights)if(float.IsNaN(h)||float.IsInfinity(h)||h<0||h>9500)throw new ArgumentException("Invalid elevation");
        }
        public static Mesh CreateMesh(CommunityPack pack)
        {
            Validate(pack);var t=pack.terrain;int n=t.rows;
            var vertices=new Vector3[n*n];var uv=new Vector2[n*n];var triangles=new int[(n-1)*(n-1)*6];int k=0;
            for(int z=0;z<n;z++)for(int x=0;x<n;x++)
            {
                int i=z*n+x;vertices[i]=new Vector3(t.xMin+x*t.cellSizeM,t.heights[i],t.zMin+z*t.cellSizeM);
                uv[i]=new Vector2((float)x/(n-1),(float)z/(n-1));
                if(z==n-1||x==n-1)continue;
                triangles[k++]=i;triangles[k++]=i+n;triangles[k++]=i+1;
                triangles[k++]=i+1;triangles[k++]=i+n;triangles[k++]=i+n+1;
            }
            var mesh=new Mesh{name=pack.id+" · metric source terrain",indexFormat=UnityEngine.Rendering.IndexFormat.UInt32};
            mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }
        void Build()
        {
            stage=new GameObject("Community source terrain").transform;stage.SetParent(transform,false);stage.localPosition=new Vector3(0,20000,0);
            TerrainMesh=CreateMesh(Pack);material=new Material(Resources.Load<Shader>("CommunityImagery"));material.SetTexture("_BaseMap",SourceImage);
            surface=new GameObject(Pack.name,typeof(MeshFilter),typeof(MeshRenderer));surface.layer=29;surface.transform.SetParent(stage,false);
            surface.GetComponent<MeshFilter>().sharedMesh=TerrainMesh;surface.GetComponent<MeshRenderer>().sharedMaterial=material;
            terrainCamera=new GameObject("Community preview camera",typeof(Camera)).GetComponent<Camera>();terrainCamera.transform.SetParent(stage,false);
            terrainCamera.cullingMask=1<<29;terrainCamera.clearFlags=CameraClearFlags.SolidColor;terrainCamera.backgroundColor=new Color(.035f,.075f,.10f);
            terrainCamera.orthographic=true;terrainCamera.nearClipPlane=1;terrainCamera.farClipPlane=10000;
            Preview=new RenderTexture(1200,900,24,RenderTextureFormat.ARGB32){name="Community terrain preview"};Preview.Create();terrainCamera.targetTexture=Preview;
            yaw=0;pitch=48;zoom=1;ApplyCamera();
        }
        void ApplyCamera()
        {
            if(terrainCamera==null)return;
            terrainCamera.aspect=4f/3;terrainCamera.orthographicSize=Pack.extentM*.63f*zoom;
            var target=TerrainMesh.bounds.center;var rotation=Quaternion.Euler(pitch,yaw,0);
            terrainCamera.transform.localPosition=target+rotation*new Vector3(0,0,-Pack.extentM*2);
            terrainCamera.transform.localRotation=rotation;
        }
        public void ShowTerrain()
        {if(Pack==null)return;sourceView=false;display.texture=Preview;display.GetComponent<UnityEngine.UI.AspectRatioFitter>().aspectRatio=4f/3;ApplyCamera();status.text=$"{Pack.name} · {Pack.extentM/1000:0.0} × {Pack.extentM/1000:0.0} km\nReal terrain · photo surface · heights at actual scale";}
        public void ShowSource()
        {if(Pack==null)return;sourceView=true;display.texture=SourceImage;display.GetComponent<UnityEngine.UI.AspectRatioFitter>().aspectRatio=1;status.text=$"{Pack.imageryKind} · {Pack.imageYear}\n{Pack.imageMetersPerPixel:0.00} m/export pixel · grid north ↑ · USDA / USGS";}
        public void Rotate(float amount){if(Pack==null)return;ShowTerrain();yaw+=amount;pitch=48;ApplyCamera();}
        public void Zoom(float amount){if(Pack==null)return;ShowTerrain();zoom=Mathf.Clamp(zoom+amount,.35f,1.3f);ApplyCamera();}
        public void Overhead(){if(Pack==null)return;ShowTerrain();yaw=0;pitch=90;zoom=1;ApplyCamera();}
        public void Clear()
        {
            if(display!=null)display.texture=null;
            if(terrainCamera!=null)terrainCamera.targetTexture=null;
            if(Preview!=null){Preview.Release();Destroy(Preview);}if(stage!=null)Destroy(stage.gameObject);
            if(TerrainMesh!=null)Destroy(TerrainMesh);if(material!=null)Destroy(material);if(SourceImage!=null)Destroy(SourceImage);
            Preview=null;stage=null;terrainCamera=null;TerrainMesh=null;material=null;SourceImage=null;Pack=null;
        }
        void OnDestroy(){Cancel();Clear();if(Instance==this)Instance=null;}
    }
}
