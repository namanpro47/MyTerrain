using System.IO;
using UnityEditor;
using UnityEngine;

// Reproducible selected-subset import. The source archive/license is recorded in the visual rebuild register.
public static class VisualRebuildAssets
{
    const string Root="Assets/Terra/Art/QuaterniusNature";
    public static void PrepareArtistTrees()
    {
        foreach(var name in new[]{"STOak1","STOak2","STOak4","STWillow2"})
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Trees/Prefabs/"+name+".prefab");
            var wrapper=new GameObject("Artist"+name);
            var form=new GameObject("Broad canopy proportions");form.transform.SetParent(wrapper.transform,false);
            var plant=(GameObject)PrefabUtility.InstantiatePrefab(source);
            plant.transform.SetParent(form.transform,false);
            plant.transform.localPosition=Vector3.zero;
            var renderers=plant.GetComponentsInChildren<MeshRenderer>();
            var bounds=renderers[0].bounds;
            foreach(var renderer in renderers)bounds.Encapsulate(renderer.bounds);
            float scale=8f/bounds.size.y;
            plant.transform.localScale*=scale;
            plant.transform.localPosition=Vector3.down*bounds.min.y*scale;
            form.transform.localScale=new Vector3(1.6f,1,1.6f);
            foreach(var renderer in renderers)
            {
                var materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {
                    var original=materials[i];
                    string path="Assets/Terra/Resources/Nature/Artist"+original.name+".mat";
                    var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                    if(material==null){material=new Material(original);AssetDatabase.CreateAsset(material,path);}
                    if(material.HasProperty("Color_BDDC3AD5"))
                    {
                        material.SetColor("Color_BDDC3AD5",new Color(.52f,.65f,.28f));
                        material.SetColor("Color_2561972A",new Color(.28f,.43f,.24f));
                    }
                    material.enableInstancing=true;EditorUtility.SetDirty(material);materials[i]=material;
                }
                renderer.sharedMaterials=materials;
            }
            PrefabUtility.SaveAsPrefabAsset(wrapper,"Assets/Terra/Resources/Nature/Artist"+name+".prefab");
            Object.DestroyImmediate(wrapper);
        }
        AssetDatabase.SaveAssets();
    }
    public static void PrepareArtistWheat()
    {
        // Preserve the artist's centimetre-to-metre root scale inside an identity wrapper.
        var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Stylised Nature/Prefabs/WheatPatch.prefab");
        var wrapper=new GameObject("ArtistWheat");
        var plant=(GameObject)PrefabUtility.InstantiatePrefab(source);
        plant.transform.SetParent(wrapper.transform,false);
        plant.transform.localPosition=Vector3.zero;
        foreach(var renderer in plant.GetComponentsInChildren<MeshRenderer>())
            foreach(var material in renderer.sharedMaterials){material.enableInstancing=true;EditorUtility.SetDirty(material);}
        PrefabUtility.SaveAsPrefabAsset(wrapper,"Assets/Terra/Resources/Nature/ArtistWheat.prefab");
        Object.DestroyImmediate(wrapper);AssetDatabase.SaveAssets();
    }
    public static void PreparePaintedTrees()
    {
        const string path="Assets/Terra/Resources/PaintedTreeMaterial.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null){material=new Material(Shader.Find("Terra/Painted Tree Study"));AssetDatabase.CreateAsset(material,path);}
        material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Terra/Resources/PaintedTrees.png"));
        material.enableInstancing=true;EditorUtility.SetDirty(material);AssetDatabase.SaveAssets();
    }
    public static void PreparePaintedUnderstory()
    {
        const string texturePath="Assets/Terra/Resources/PaintedUnderstory.png";
        var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);
        importer.alphaIsTransparency=true;importer.mipmapEnabled=true;importer.mipMapsPreserveCoverage=true;importer.alphaTestReferenceValue=.32f;
        importer.maxTextureSize=2048;importer.filterMode=FilterMode.Trilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
        const string path="Assets/Terra/Resources/PaintedUnderstoryMaterial.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null){material=new Material(Shader.Find("Terra/Painted Tree Study"));AssetDatabase.CreateAsset(material,path);}
        material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath));material.enableInstancing=true;
        EditorUtility.SetDirty(material);AssetDatabase.SaveAssets();
    }
    public static void Import()
    {
        AssetDatabase.Refresh();
        Directory.CreateDirectory(Root+"/Materials");
        Directory.CreateDirectory("Assets/Terra/Resources/Nature");
        foreach(var path in Directory.GetFiles(Root+"/Textures","*.png"))
        {
            var importer=(TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType=path.Contains("_Normal.png")?TextureImporterType.NormalMap:TextureImporterType.Default;
            importer.alphaIsTransparency=true;importer.mipmapEnabled=true;importer.maxTextureSize=1024;
            importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=4;importer.SaveAndReimport();
        }
        foreach(var path in Directory.GetFiles(Root+"/Models","*.fbx"))
        {
            var importer=(ModelImporter)AssetImporter.GetAtPath(path);
            importer.isReadable=true;importer.importCameras=false;importer.importLights=false;importer.importAnimation=false;
            importer.SaveAndReimport();
            var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var go=(GameObject)PrefabUtility.InstantiatePrefab(model);
            foreach(var renderer in go.GetComponentsInChildren<MeshRenderer>())
            {
                var mats=renderer.sharedMaterials;
                for(int i=0;i<mats.Length;i++)mats[i]=MaterialFor(mats[i].name);
                renderer.sharedMaterials=mats;
            }
            PrefabUtility.SaveAsPrefabAsset(go,"Assets/Terra/Resources/Nature/"+Path.GetFileNameWithoutExtension(path)+".prefab");
            Object.DestroyImmediate(go);
        }
        AssetDatabase.SaveAssets();
        Debug.Log("TERRA_ART_IMPORT: selected CC0 nature models configured for URP; study not accepted.");
    }
    static Material MaterialFor(string name)
    {
        string key=name.Replace(" (Instance)","");string path=Root+"/Materials/"+key+".mat";
        var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));AssetDatabase.CreateAsset(mat,path);}
        bool leaf=key.Contains("Leaves"),flower=key.Contains("Flower");
        mat.shader=Shader.Find(leaf?"Terra/Painted Canopy":"Universal Render Pipeline/Simple Lit");
        string texture=key.Contains("Bark")?"Bark_NormalTree":key.Contains("Twisted")?"Leaves_TwistedTree":key=="Leaves"?"Leaves":leaf?"Leaves_NormalTree":key.Contains("Rock")?"Rocks_Diffuse":flower?"Flowers":"Grass";
        mat.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Root+"/Textures/"+texture+".png"));
        mat.SetColor("_BaseColor",leaf?new Color(.56f,.73f,.25f):Color.white);
        mat.SetColor("_SpecColor",Color.black);mat.SetFloat("_Smoothness",.05f);mat.SetFloat("_Cull",0);
        mat.SetFloat("_AlphaClip",leaf||flower?1:0);mat.SetFloat("_Cutoff",.35f);
        if(leaf||flower){mat.EnableKeyword("_ALPHATEST_ON");mat.renderQueue=2450;}
        mat.enableInstancing=true;EditorUtility.SetDirty(mat);return mat;
    }
}
