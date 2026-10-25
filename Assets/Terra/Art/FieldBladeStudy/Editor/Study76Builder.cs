using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

// Local Phase2A study builder. No scene or Resources asset is modified.
public static class Study76Builder
{
    const string Root="Assets/Terra/Art/FieldBladeStudy/";
    public static void Build()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Build outside Play mode.");
        var source=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"GDQuest/grass_triangle.obj");
        if(source==null)throw new InvalidOperationException("Missing credited GDQuest source blade.");
        var filter=source.GetComponentInChildren<MeshFilter>();
        var original=Object.Instantiate(filter.sharedMesh);
        var transform=filter.transform.localToWorldMatrix;
        original.vertices=original.vertices.Select(transform.MultiplyPoint3x4).ToArray();
        original.RecalculateBounds();var bounds=original.bounds;
        if(bounds.size.y<1)throw new InvalidOperationException("Unexpected source orientation/scale.");
        float scale=1.2f/bounds.size.y;
        original.vertices=original.vertices.Select(v=>(v-new Vector3(bounds.center.x,bounds.min.y,bounds.center.z))*scale).ToArray();
        original.uv=original.vertices.Select(v=>new Vector2(v.x/.304f+.5f,v.y/1.2f)).ToArray();
        original.RecalculateBounds();original.RecalculateNormals();
        var shader=AssetDatabase.LoadAssetAtPath<Shader>(Root+"Blade.shader");
        if(shader==null||!shader.isSupported)throw new InvalidOperationException("Unsupported study shader.");
        var messages=ShaderUtil.GetShaderMessages(shader);
        if(messages.Any(m=>m.severity.ToString()=="Error"))throw new InvalidOperationException(string.Join("\n",messages.Select(m=>m.message)));
        var noise=new Texture2D(128,128,TextureFormat.RGBA32,true,true){name="Study76 periodic wind noise",wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
        var pixels=new Color[128*128];
        for(int y=0;y<128;y++)for(int x=0;x<128;x++)
        {
            float u=x/128f,v=y/128f;
            float Sample(float px,float py)=>Mathf.PerlinNoise(px*4+76,py*4+23);
            float value=Mathf.Lerp(Mathf.Lerp(Sample(u,v),Sample(u-1,v),u),Mathf.Lerp(Sample(u,v-1),Sample(u-1,v-1),u),v);
            pixels[y*128+x]=new Color(value,value,value,1);
        }
        noise.SetPixels(pixels);noise.Apply();noise=Save(noise,"WindNoise.asset");
        var material=new Material(shader){name="Study76 native gradient",enableInstancing=true};material.SetTexture("_WindNoise",noise);material.SetFloat("_StudyTime",0);material=Save(material,"NativeGradient.mat");
        float bladeHalfWidth=original.bounds.size.x*.5f;
        var root=new GameObject("FieldCluster");var lods=new LOD[3];var metrics=new List<object>();
        try
        {
            for(int level=0;level<3;level++)
            {
                Mesh mesh=level==0?original:new Mesh();
                if(level>0)
                {
                    float width=bladeHalfWidth;
                    mesh.vertices=level==1?new[]{new Vector3(-width,0,0),new Vector3(width,0,0),new Vector3(-width*.5f,.6f,0),new Vector3(width*.5f,.6f,0),new Vector3(0,1.2f,0)}:new[]{new Vector3(-width,0,0),new Vector3(width,0,0),new Vector3(0,1.2f,0)};
                    mesh.triangles=level==1?new[]{0,1,2,1,3,2,2,3,4}:new[]{0,1,2};
                    mesh.uv=mesh.vertices.Select(v=>new Vector2(v.x/(width*2)+.5f,v.y/1.2f)).ToArray();mesh.RecalculateNormals();mesh.RecalculateBounds();
                }
                var geometricBounds=mesh.bounds;
                // Maximum default wind displacement: ±0.6m horizontally, +0.27m vertically.
                // GPU displacement must remain inside the bounds used for native instancing.
                var windBounds=geometricBounds;windBounds.center+=Vector3.up*.135f;
                windBounds.Expand(new Vector3(1.2f,.27f,1.2f));mesh.bounds=windBounds;
                mesh.name="Study76 blade LOD"+level;mesh=Save(mesh,"Blade-lod"+level+".asset");
                var go=new GameObject("LOD"+level);go.transform.SetParent(root.transform,false);go.AddComponent<MeshFilter>().sharedMesh=mesh;var renderer=go.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;
                lods[level]=new LOD(level==0?.5f:level==1?.2f:.001f,new Renderer[]{renderer});
                metrics.Add(new{level,vertices=mesh.vertexCount,triangles=mesh.triangles.Length/3,height=geometricBounds.size.y,width=geometricBounds.size.x,root=geometricBounds.min.y,windBoundsHeight=mesh.bounds.size.y});
            }
            root.AddComponent<LODGroup>().SetLODs(lods);root.GetComponent<LODGroup>().RecalculateBounds();var profile=root.AddComponent<Terra.Rendering.NatureLodProfile>();profile.nearPixels=12;profile.farPixels=4;profile.cullPixels=.5f;profile.includeWidth=false;
            PrefabUtility.SaveAsPrefabAsset(root,Root+"FieldCluster.prefab");AssetDatabase.SaveAssets();
            Directory.CreateDirectory("Builds/ArtResearch/Study76");File.WriteAllText("Builds/ArtResearch/Study76/build.json",Newtonsoft.Json.JsonConvert.SerializeObject(new{sourceRevision="f8fbea9cc6dd69cf74a56c2fc41b0ba6c3362265",levels=metrics,shaderMessages=messages.Select(m=>new{m.message,m.severity}).ToArray(),sceneUnchanged=true,scope="Isolated source-derived geometry and URP gradient/wind adaptation; not rendered scene acceptance."},Newtonsoft.Json.Formatting.Indented));
            Debug.Log("STUDY76_BUILD_COMPLETE");
        }
        finally{Object.DestroyImmediate(root);}
    }
    static T Save<T>(T value,string relative) where T:Object
    {
        string path=Root+relative;var old=AssetDatabase.LoadAssetAtPath<T>(path);
        if(old==null){AssetDatabase.CreateAsset(value,path);return value;}
        EditorUtility.CopySerialized(value,old);EditorUtility.SetDirty(old);Object.DestroyImmediate(value);return old;
    }
}
