using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityMeshSimplifier;
using Object=UnityEngine.Object;

public static class CultivatedCropAssets
{
    public static string Build()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Bake outside Play mode.");
        const string source="Assets/Terra/Art/QuaterniusCrops/Lettuce_4.fbx";
        const string folder="Assets/Terra/Resources/CultivatedBeds/";
        if(!AssetDatabase.IsValidFolder(folder.TrimEnd('/')))AssetDatabase.CreateFolder("Assets/Terra/Resources","CultivatedBeds");
        var model=AssetDatabase.LoadAssetAtPath<GameObject>(source);
        var filter=model.GetComponentInChildren<MeshFilter>();
        var transform=model.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
        // The FBX root itself carries the centimetre/axis conversion.
        transform=model.transform.localToWorldMatrix*transform;
        var original=Object.Instantiate(filter.sharedMesh);
        original.vertices=original.vertices.Select(transform.MultiplyPoint3x4).ToArray();
        original.normals=original.normals.Select(n=>transform.inverse.transpose.MultiplyVector(n).normalized).ToArray();
        original.RecalculateBounds();
        var sourceMaterials=filter.GetComponent<MeshRenderer>().sharedMaterials;
        var materials=new Material[sourceMaterials.Length];
        Material SaveMaterial(string name,Color color,Texture texture=null)
        {
            string path=folder+name+".mat";var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Simple Lit"));AssetDatabase.CreateAsset(material,path);}
            material.name=name;material.enableInstancing=true;material.SetColor("_BaseColor",color);
            material.SetColor("_SpecColor",Color.black);material.SetFloat("_Smoothness",0);
            if(texture!=null)material.SetTexture("_BaseMap",texture);EditorUtility.SetDirty(material);return material;
        }
        for(int i=0;i<materials.Length;i++)materials[i]=SaveMaterial("Leaf-"+i,sourceMaterials[i].color);
        SaveMaterial("Soil",new Color(.765f,.68f,.565f),Resources.Load<Texture2D>("JulioGround/EarthColor"));
        var root=new GameObject("LeafCrop");var lods=new LOD[3];var rows=new List<string>();
        try
        {
            for(int level=0;level<3;level++)
            {
                Mesh mesh;
                if(level==0)mesh=original;
                else{var simplify=new MeshSimplifier(original);simplify.SimplifyMesh(level==1?.28f:.06f);mesh=simplify.ToMesh();mesh.RecalculateBounds();}
                mesh.name="LeafCrop-lod"+level;string path=folder+mesh.name+".asset";
                var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                if(saved==null){AssetDatabase.CreateAsset(mesh,path);saved=mesh;}
                else{EditorUtility.CopySerialized(mesh,saved);EditorUtility.SetDirty(saved);if(level!=0)Object.DestroyImmediate(mesh);}
                var part=new GameObject("LOD"+level);part.transform.SetParent(root.transform,false);
                part.AddComponent<MeshFilter>().sharedMesh=saved;var renderer=part.AddComponent<MeshRenderer>();renderer.sharedMaterials=materials;
                lods[level]=new LOD(level==0?.5f:level==1?.18f:.002f,new Renderer[]{renderer});
                rows.Add(level+": "+saved.triangles.Length/3+" triangles; "+saved.bounds.size);
            }
            root.AddComponent<LODGroup>().SetLODs(lods);root.GetComponent<LODGroup>().RecalculateBounds();
            root.AddComponent<Terra.Rendering.NatureLodProfile>();
            PrefabUtility.SaveAsPrefabAsset(root,folder+"LeafCrop.prefab");AssetDatabase.SaveAssets();
        }
        finally{Object.DestroyImmediate(root);if(!AssetDatabase.Contains(original))Object.DestroyImmediate(original);}
        return string.Join("\n",rows);
    }
}
