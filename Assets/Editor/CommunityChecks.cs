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
            Need(vertices.Length==14641,"121 × 121 source nodes");Need(triangles.Length==86400,"complete source triangulation");
            Need(uv[0]==Vector2.zero&&uv[uv.Length-1]==Vector2.one,"SW and NE UV anchors");
            Need(vertices[0].x==-600&&vertices[0].z==-600,"southwest metric origin");
            Need(vertices[vertices.Length-1].x==600&&vertices[vertices.Length-1].z==600,"northeast metric extent");
            for(int i=0;i<vertices.Length;i++)Need(Math.Abs(vertices[i].y-t.heights[i])<.0001f,"no height distortion");
            for(int i=0;i<triangles.Length;i+=3)
                Need(Vector3.Cross(vertices[triangles[i+1]]-vertices[triangles[i]],vertices[triangles[i+2]]-vertices[triangles[i]]).y>0,"upward triangle winding");
            var image=new Texture2D(2,2);Need(image.LoadImage(File.ReadAllBytes(Path.Combine(dir,"imagery.jpg"))),"source JPEG decodes in Unity");
            Need(image.width==pack.imageWidth&&image.height==pack.imageHeight,"Unity image dimensions");
            Need(Resources.Load<Shader>("CommunityImagery")!=null,"render shader included");
            // JSON guard must reject damaged geometry before allocation/rendering.
            t.heights[0]=float.NaN;bool rejected=false;try{CommunityTerrain.Validate(pack);}catch(ArgumentException){rejected=true;}Need(rejected,"NaN rejected");
            UnityEngine.Object.DestroyImmediate(mesh);UnityEngine.Object.DestroyImmediate(image);
        }
        Directory.CreateDirectory("docs/evidence/community-import");
        File.WriteAllText("docs/evidence/community-import/unity-checks.json","{\"checks\":"+checks+",\"passed\":true}");
        Debug.Log("COMMUNITY_CHECKS_PASSED: "+checks);
    }
}
