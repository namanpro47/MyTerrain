using System.IO;
using UnityEditor;
using UnityEngine;

// Reproducible selected-subset import. The source archive/license is recorded in the visual rebuild register.
public static class VisualRebuildAssets
{
    public static void PrepareGroundNormals()
    {
        foreach(string kind in new[]{"Grass","Dirt"})
        {
            string destination="Assets/Terra/Resources/Artist"+kind+"Normal.png";
            if(AssetDatabase.LoadAssetAtPath<Texture2D>(destination)==null)
                AssetDatabase.CopyAsset("Assets/Stylised Nature/Materials/Textures/Terrain/Stylised"+kind+"Normal.png",destination);
            var importer=(TextureImporter)AssetImporter.GetAtPath(destination);
            importer.textureType=TextureImporterType.NormalMap;importer.maxTextureSize=1024;
            importer.wrapMode=TextureWrapMode.Repeat;importer.filterMode=FilterMode.Trilinear;importer.anisoLevel=4;
            importer.SaveAndReimport();
        }
    }
    public static void PrepareSolidOaks()
    {
        const string sourceRoot="Assets/Terra/Art/SymphonieOak/";
        for(int i=1;i<=4;i++)
        {
            var source=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(sourceRoot+"URP/Prefabs/Oak0"+i+".prefab"));
            var lod=source.GetComponent<LODGroup>();if(lod!=null)Object.DestroyImmediate(lod);
            foreach(var renderer in source.GetComponentsInChildren<MeshRenderer>(true))
            {
                var original=renderer.sharedMaterial;
                string path="Assets/Terra/Resources/Nature/SolidOak0"+i+".mat";
                var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
                material.SetTexture("_BaseMap",original.GetTexture("_BaseMap"));
                material.SetTexture("_BumpMap",original.GetTexture("_BumpMap"));
                material.SetTexture("_OcclusionMap",original.GetTexture("_OcclusionMap"));
                // Supplied metallic setup rendered almost black without a reflection environment.
                // Keep the artist's sculpted forms and painted maps in a matte foliage material.
                material.SetFloat("_Metallic",0);material.SetFloat("_Smoothness",.15f);material.SetFloat("_BumpScale",.65f);
                material.color=new Color(.72f,.84f,.68f);material.EnableKeyword("_NORMALMAP");material.EnableKeyword("_OCCLUSIONMAP");
                material.enableInstancing=true;EditorUtility.SetDirty(material);renderer.sharedMaterial=material;renderer.enabled=true;
            }
            var bounds=System.Array.Find(source.GetComponentsInChildren<MeshRenderer>(true),r=>r.name.EndsWith("LOD0")).bounds;
            var wrapper=new GameObject("SolidOak0"+i);source.transform.SetParent(wrapper.transform,false);
            float scale=8/bounds.size.y;source.transform.localScale*=scale;source.transform.localPosition=Vector3.down*bounds.min.y*scale;
            PrefabUtility.SaveAsPrefabAsset(wrapper,"Assets/Terra/Resources/Nature/SolidOak0"+i+".prefab");Object.DestroyImmediate(wrapper);
        }
        AssetDatabase.SaveAssets();
    }
    public static void PrepareFieldCluster()
    {
        var source=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Terra/Resources/Nature/ArtistWheat.prefab"));
        var filter=source.GetComponentInChildren<MeshFilter>();
        var copies=new System.Collections.Generic.List<CombineInstance>();
        // A narrow planted row, not nine overlapping crosses covering every square metre.
        for(int z=0;z<3;z++)
            copies.Add(new CombineInstance{mesh=filter.sharedMesh,transform=Matrix4x4.TRS(new Vector3(0,0,(z-1)*.55f),Quaternion.Euler(0,z*37,0),new Vector3(1.05f,1.15f,1.05f))*filter.transform.localToWorldMatrix});
        const string meshPath="Assets/Terra/Resources/Nature/FieldCluster.asset";
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,meshPath);}else mesh.Clear();
        mesh.CombineMeshes(copies.ToArray(),true,true);EditorUtility.SetDirty(mesh);
        const string materialPath="Assets/Terra/Resources/Nature/FieldCluster.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(material==null){material=new Material(Resources.Load<Shader>("TerraCropCards"));AssetDatabase.CreateAsset(material,materialPath);}
        material.shader=Resources.Load<Shader>("TerraCropCards");
        material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Stylised Nature/Materials/Textures/Grass/GrassWheat.png"));
        material.color=new Color(.72f,.62f,.34f);material.enableInstancing=true;EditorUtility.SetDirty(material);
        var wrapper=new GameObject("FieldCluster");wrapper.AddComponent<MeshFilter>().sharedMesh=mesh;wrapper.AddComponent<MeshRenderer>().sharedMaterial=material;
        PrefabUtility.SaveAsPrefabAsset(wrapper,"Assets/Terra/Resources/Nature/FieldCluster.prefab");
        Object.DestroyImmediate(wrapper);Object.DestroyImmediate(source);AssetDatabase.SaveAssets();
    }
    const string Root="Assets/Terra/Art/QuaterniusNature";
    public static void PrepareGeometricWheat()
    {
        const string sourcePath="Assets/Terra/Art/QuaterniusCrops/Wheat_4.fbx";
        var importer=(ModelImporter)AssetImporter.GetAtPath(sourcePath);
        importer.isReadable=true;importer.importAnimation=false;importer.SaveAndReimport();
        var source=Object.Instantiate(AssetDatabase.LoadAssetAtPath<GameObject>(sourcePath));
        var filters=source.GetComponentsInChildren<MeshFilter>();
        var copies=new System.Collections.Generic.List<CombineInstance>();
        foreach(var position in new[]{new Vector3(-.45f,0,-.4f),new Vector3(.45f,0,.1f),new Vector3(0,0,.45f)})
            foreach(var filter in filters)
                copies.Add(new CombineInstance{mesh=filter.sharedMesh,transform=Matrix4x4.TRS(position,Quaternion.Euler(0,position.x*47,0),new Vector3(9,2.7f,9))*filter.transform.localToWorldMatrix});
        const string meshPath="Assets/Terra/Art/QuaterniusCrops/FieldWheat.asset";
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
        if(mesh==null){mesh=new Mesh();AssetDatabase.CreateAsset(mesh,meshPath);}else mesh.Clear();
        mesh.CombineMeshes(copies.ToArray(),true,true);EditorUtility.SetDirty(mesh);
        const string materialPath="Assets/Terra/Resources/Nature/FieldWheat.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
        if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));AssetDatabase.CreateAsset(material,materialPath);}
        material.shader=Resources.Load<Shader>("TerraCrop");material.color=new Color(.74f,.65f,.38f);material.enableInstancing=true;EditorUtility.SetDirty(material);
        var wrapper=new GameObject("GeometricWheat");wrapper.AddComponent<MeshFilter>().sharedMesh=mesh;wrapper.AddComponent<MeshRenderer>().sharedMaterial=material;
        PrefabUtility.SaveAsPrefabAsset(wrapper,"Assets/Terra/Resources/Nature/GeometricWheat.prefab");
        Object.DestroyImmediate(wrapper);Object.DestroyImmediate(source);AssetDatabase.SaveAssets();
    }
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
        const string path="Assets/Terra/Art/ArchivedStudies/PaintedTreeMaterial.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null){material=new Material(Shader.Find("Terra/Painted Tree Study"));AssetDatabase.CreateAsset(material,path);}
        material.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Terra/Art/ArchivedStudies/PaintedTrees.png"));
        material.enableInstancing=true;EditorUtility.SetDirty(material);AssetDatabase.SaveAssets();
    }
    public static void PreparePaintedUnderstory()
    {
        const string texturePath="Assets/Terra/Art/ArchivedStudies/PaintedUnderstory.png";
        var importer=(TextureImporter)AssetImporter.GetAtPath(texturePath);
        importer.alphaIsTransparency=true;importer.mipmapEnabled=true;importer.mipMapsPreserveCoverage=true;importer.alphaTestReferenceValue=.32f;
        importer.maxTextureSize=2048;importer.filterMode=FilterMode.Trilinear;importer.wrapMode=TextureWrapMode.Clamp;importer.SaveAndReimport();
        const string path="Assets/Terra/Art/ArchivedStudies/PaintedUnderstoryMaterial.mat";
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
