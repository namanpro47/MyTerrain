using System;
using System.IO;
using Terra.Gameplay;
using UnityEngine;

public static class CommunityChecks
{
    static int checks;
    static void Need(bool condition,string why){checks++;if(!condition)throw new Exception("Community check: "+why);}
    public static void Run()
    {
        checks=0;CommunityBuild.Stage();
        foreach(var dir in Directory.GetDirectories(Path.Combine(Application.streamingAssetsPath,"Communities")))
        {
            var pack=JsonUtility.FromJson<CommunityPack>(File.ReadAllText(Path.Combine(dir,"community.json")));
            CommunityTerrain.Validate(pack);var mesh=CommunityTerrain.CreateMesh(pack);
            var vertices=mesh.vertices;var uv=mesh.uv;var triangles=mesh.triangles;var t=pack.terrain;
            Need(vertices.Length==t.rows*t.columns,"all source nodes");Need(triangles.Length==(t.rows-1)*(t.columns-1)*6,"complete source triangulation");
            Need(uv[0]==Vector2.zero&&uv[uv.Length-1]==Vector2.one,"SW and NE UV anchors");
            Need(vertices[0].x==-pack.extentM/2&&vertices[0].z==-pack.extentM/2,"southwest metric origin");
            Need(vertices[vertices.Length-1].x==pack.extentM/2&&vertices[vertices.Length-1].z==pack.extentM/2,"northeast metric extent");
            for(int i=0;i<vertices.Length;i++)Need(Math.Abs(vertices[i].y-t.heights[i])<.0001f,"no height distortion");
            for(int i=0;i<triangles.Length;i+=3)
                Need(Vector3.Cross(vertices[triangles[i+1]]-vertices[triangles[i]],vertices[triangles[i+2]]-vertices[triangles[i]]).y>0,"upward triangle winding");
            var image=new Texture2D(2,2);Need(image.LoadImage(File.ReadAllBytes(Path.Combine(dir,"imagery.jpg"))),"source JPEG decodes in Unity");
            Need(image.width==pack.imageWidth&&image.height==pack.imageHeight,"Unity image dimensions");
            Need(Resources.Load<Shader>("CommunityImagery")!=null,"render shader included");
            Capture(pack,mesh,image);
            // JSON guard must reject damaged geometry before allocation/rendering.
            t.heights[0]=float.NaN;bool rejected=false;try{CommunityTerrain.Validate(pack);}catch(ArgumentException){rejected=true;}Need(rejected,"NaN rejected");
            UnityEngine.Object.DestroyImmediate(mesh);UnityEngine.Object.DestroyImmediate(image);
        }
        Directory.CreateDirectory("docs/evidence/community-import");
        File.WriteAllText("docs/evidence/community-import/unity-checks.json","{\"checks\":"+checks+",\"passed\":true}");
        Debug.Log("COMMUNITY_CHECKS_PASSED: "+checks);
    }
    static void Capture(CommunityPack pack,Mesh mesh,Texture2D source)
    {
        source.filterMode=FilterMode.Bilinear;source.mipMapBias=-10;source.wrapMode=TextureWrapMode.Clamp;
        var root=new GameObject("Community verification geometry");root.layer=29;root.transform.position=new Vector3(0,20000,0);
        var material=new Material(Resources.Load<Shader>("CommunityImagery"));material.SetTexture("_BaseMap",source);
        root.AddComponent<MeshFilter>().sharedMesh=mesh;root.AddComponent<MeshRenderer>().sharedMaterial=material;
        var cameraObject=new GameObject("Community verification camera");var camera=cameraObject.AddComponent<Camera>();
        var target=new RenderTexture(1024,1024,24,RenderTextureFormat.ARGB32);target.Create();
        var result=new Texture2D(1024,1024,TextureFormat.RGB24,false);
        var old=RenderTexture.active;
        try
        {
            camera.targetTexture=target;camera.cullingMask=1<<29;camera.orthographic=true;camera.orthographicSize=pack.extentM/2;
            camera.nearClipPlane=1;camera.farClipPlane=5000;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.black;
            camera.transform.position=root.transform.position+new Vector3(0,2000,0);camera.transform.rotation=Quaternion.Euler(90,0,0);
            camera.Render();RenderTexture.active=target;result.ReadPixels(new Rect(0,0,1024,1024),0,0);result.Apply();
            Directory.CreateDirectory("docs/evidence/community-import");
            File.WriteAllBytes("docs/evidence/community-import/"+pack.id+"-overhead.png",result.EncodeToPNG());
            // Independent source-color registration across the image, including all four quadrants.
            float maximum=0;int samples=0;
            foreach(int y in new[]{96,287,512,739,927})foreach(int x in new[]{96,287,512,739,927})
            {
                var expected=source.GetPixelBilinear((x+.5f)/1024,(y+.5f)/1024);var actual=result.GetPixel(x,y);
                maximum=Mathf.Max(maximum,Mathf.Abs(expected.r-actual.r),Mathf.Abs(expected.g-actual.g),Mathf.Abs(expected.b-actual.b));samples++;
            }
            File.WriteAllText("docs/evidence/community-import/"+pack.id+"-registration.json","{\"samples\":"+samples+",\"maxChannelError\":"+maximum.ToString(System.Globalization.CultureInfo.InvariantCulture)+"}");
            Need(maximum<.08f,"rendered source registration/color agreement");
        }
        finally
        {
            RenderTexture.active=old;camera.targetTexture=null;target.Release();
            UnityEngine.Object.DestroyImmediate(cameraObject);UnityEngine.Object.DestroyImmediate(root);UnityEngine.Object.DestroyImmediate(material);UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(result);
        }
    }
}
