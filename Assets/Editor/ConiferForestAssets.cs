using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Object=UnityEngine.Object;

// Same licensed Quaternius family as the broadleaf canopy; preserve artist geometry and pixels.
public static class ConiferForestAssets
{
    const string Source="Assets/Terra/Art/QuaterniusUltimateNature/Pine/";
    const string Output="Assets/Terra/Resources/ClusterForest/";

    public static string Build()
    {
        if(EditorApplication.isPlaying)throw new InvalidOperationException("Bake outside Play mode.");
        var leaf=AssetDatabase.LoadAssetAtPath<Material>(Source+"PineCanopy.mat");
        if(leaf==null)
        {
            leaf=new Material(AssetDatabase.LoadAssetAtPath<Shader>("Assets/Terra/Art/QuaterniusUltimateNature/ClusterStudy/ClusterLeaves.shader")){name="Pine canopy",enableInstancing=true};
            leaf.SetTexture("_BaseMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"PineTree_Leaves.png"));
            leaf.SetColor("_BaseColor",new Color(.65f,.85f,.8f));leaf.SetFloat("_Cutoff",.5f);
            AssetDatabase.CreateAsset(leaf,Source+"PineCanopy.mat");
        }
        var bark=AssetDatabase.LoadAssetAtPath<Material>(Source+"PineTree_Bark.mat");
        var result=new List<string>();
        foreach(int id in new[]{1,3})
        {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Source+"PineTree_"+id+".prefab");
            var filters=prefab.GetComponentsInChildren<MeshFilter>();
            if(filters.Length!=1||filters[0].sharedMesh.subMeshCount!=2)
                throw new InvalidOperationException("Expected source bark and leaf slots on one mesh.");
            var filter=filters[0];var full=Object.Instantiate(filter.sharedMesh);
            var transform=prefab.transform.worldToLocalMatrix*filter.transform.localToWorldMatrix;
            var points=full.vertices.Select(transform.MultiplyPoint3x4).ToArray();float bottom=points.Min(p=>p.y);
            for(int i=0;i<points.Length;i++)points[i].y-=bottom;
            full.vertices=points;
            var normalTransform=transform.inverse.transpose;
            full.normals=full.normals.Select(n=>normalTransform.MultiplyVector(n).normalized).ToArray();
            full.tangents=full.tangents.Select(t=>
            {
                var direction=transform.MultiplyVector(new Vector3(t.x,t.y,t.z)).normalized;
                return new Vector4(direction.x,direction.y,direction.z,t.w);
            }).ToArray();
            full.RecalculateBounds();
            // The pine's 23–28 bent branch fans are already inexpensive (502–632 triangles).
            // Preserve every fan at each distance; simplify the more expensive bark only.
            var levels=new[]{full,ClusteredForestAssets.Reduce(full,1,.35f),ClusteredForestAssets.Reduce(full,1,.15f)};
            var root=new GameObject("ClusterPine_"+id);
            try
            {
                var lods=new LOD[3];
                for(int i=0;i<3;i++)
                {
                    var mesh=levels[i];mesh.name="PineTree_"+id+"-lod"+i;
                    if(mesh.GetIndexCount(1)==0)throw new InvalidOperationException("Lost pine foliage.");
                    var path=Output+mesh.name+".asset";var existing=AssetDatabase.LoadAssetAtPath<Mesh>(path);
                    if(existing==null)AssetDatabase.CreateAsset(mesh,path);
                    else{EditorUtility.CopySerialized(mesh,existing);EditorUtility.SetDirty(existing);Object.DestroyImmediate(mesh);mesh=existing;}
                    var part=new GameObject("LOD"+i);part.transform.SetParent(root.transform,false);
                    part.AddComponent<MeshFilter>().sharedMesh=mesh;
                    var renderer=part.AddComponent<MeshRenderer>();renderer.sharedMaterials=new[]{bark,leaf};
                    lods[i]=new LOD(i==0?.5f:i==1?.18f:.002f,new Renderer[]{renderer});
                    result.Add(root.name+" LOD"+i+": "+mesh.triangles.Length/3+" triangles; "+mesh.bounds.size.y+"m");
                }
                var group=root.AddComponent<LODGroup>();group.SetLODs(lods);group.RecalculateBounds();
                PrefabUtility.SaveAsPrefabAsset(root,Output+root.name+".prefab");
            }
            finally{Object.DestroyImmediate(root);}
        }
        AssetDatabase.SaveAssets();return string.Join("\n",result);
    }
}
