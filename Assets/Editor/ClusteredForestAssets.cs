using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityMeshSimplifier;
using Object = UnityEngine.Object;

// Source provenance and the cluster-normal bake are recorded in study44.
// Distant foliage keeps whole artist leaf cards; trunk reduction is independent.
public static class ClusteredForestAssets
{
    const string Source = "Assets/Terra/Art/QuaterniusUltimateNature/ClusterStudy/";
    const string Output = "Assets/Terra/Resources/ClusterForest";

    public static string Build()
    {
        if (EditorApplication.isPlaying) throw new InvalidOperationException("Bake outside Play mode.");
        if (!AssetDatabase.IsValidFolder(Output)) AssetDatabase.CreateFolder("Assets/Terra/Resources", "ClusterForest");
        var results = new List<string>();
        foreach (var id in new[] { 1, 3, 5 })
        {
            string name = "NormalTree_" + id;
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(Source + name + ".prefab");
            var original = source.GetComponent<MeshFilter>().sharedMesh;
            var materials = source.GetComponent<MeshRenderer>().sharedMaterials;
            var levels = new[] { original, Reduce(original, .55f, .35f), Reduce(original, .20f, .12f) };
            var root = new GameObject("ClusterTree_" + id);
            var lods = new LOD[3];
            for (int level = 0; level < 3; level++)
            {
                var mesh = levels[level];
                if (level > 0)
                {
                    mesh.name = name + "-lod" + level;
                    mesh = SaveMesh(mesh, Output + "/" + mesh.name + ".asset");
                }
                if (mesh.subMeshCount != materials.Length || mesh.GetIndexCount(1) == 0)
                    throw new InvalidOperationException("Lost foliage/material correspondence: " + name);
                var part = new GameObject("LOD" + level);
                part.transform.SetParent(root.transform, false);
                part.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = part.AddComponent<MeshRenderer>();
                renderer.sharedMaterials = materials;
                lods[level] = new LOD(level == 0 ? .5f : level == 1 ? .18f : .002f, new Renderer[] { renderer });
                results.Add(name + " LOD" + level + ": " + mesh.triangles.Length / 3 + " triangles");
            }
            var group = root.AddComponent<LODGroup>();
            group.SetLODs(lods);
            group.RecalculateBounds();
            PrefabUtility.SaveAsPrefabAsset(root, Output + "/ClusterTree_" + id + ".prefab");
            Object.DestroyImmediate(root);
        }
        AssetDatabase.SaveAssets();
        return string.Join("\n", results);
    }

    static Mesh SaveMesh(Mesh mesh, string path)
    {
        var existing = AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if (existing == null) { AssetDatabase.CreateAsset(mesh, path); return mesh; }
        EditorUtility.CopySerialized(mesh, existing);
        EditorUtility.SetDirty(existing);
        Object.DestroyImmediate(mesh);
        return existing;
    }

    static Mesh Reduce(Mesh source, float leafFraction, float barkFraction)
    {
        var barkSource = Object.Instantiate(source);
        barkSource.subMeshCount = 1;
        barkSource.SetTriangles(source.GetTriangles(0), 0);
        var simplifier = new MeshSimplifier(barkSource);
        var options = SimplificationOptions.Default;
        options.PreserveUVSeamEdges = true;
        options.PreserveUVFoldoverEdges = true;
        options.PreserveBorderEdges = true;
        simplifier.SimplificationOptions = options;
        simplifier.SimplifyMesh(barkFraction);
        var bark = simplifier.ToMesh();
        var leaves = ReduceCards(source, leafFraction);
        var result = new Mesh();
        result.CombineMeshes(new[]
        {
            new CombineInstance { mesh = bark, transform = Matrix4x4.identity },
            new CombineInstance { mesh = leaves, transform = Matrix4x4.identity }
        }, false, true);
        var normals = result.normals;
        for (int i = 0; i < normals.Length; i++) normals[i] = normals[i].normalized;
        result.normals = normals;
        result.RecalculateBounds();
        Object.DestroyImmediate(barkSource);
        Object.DestroyImmediate(bark);
        Object.DestroyImmediate(leaves);
        return result;
    }

    static Mesh ReduceCards(Mesh source, float fraction)
    {
        var points = source.vertices;
        var normals = source.normals;
        var uv = source.uv;
        var tangents = source.tangents;
        var indices = source.GetTriangles(1);
        var parent = Enumerable.Range(0, source.vertexCount).ToArray();
        int Find(int i)
        {
            while (parent[i] != i) { parent[i] = parent[parent[i]]; i = parent[i]; }
            return i;
        }
        for (int i = 0; i < indices.Length; i += 3)
        {
            parent[Find(indices[i + 1])] = Find(indices[i]);
            parent[Find(indices[i + 2])] = Find(indices[i]);
        }
        var cards = indices.Distinct().GroupBy(Find).Select(g => g.ToArray()).ToArray();
        if (cards.Any(c => c.Length != 4)) throw new InvalidOperationException("Expected independent four-vertex artist leaf cards.");
        var centers = cards.Select(c => c.Aggregate(Vector3.zero, (sum, i) => sum + points[i]) / c.Length).ToArray();
        int wanted = Mathf.Max(1, Mathf.RoundToInt(cards.Length * fraction));
        var selected = new HashSet<int>();
        var distance = Enumerable.Repeat(float.MaxValue, cards.Length).ToArray();
        int next = Enumerable.Range(0, cards.Length).OrderByDescending(i => centers[i].y).First();
        while (selected.Count < wanted)
        {
            selected.Add(next);
            var center = centers[next];
            float farthest = -1;
            for (int i = 0; i < cards.Length; i++)
            {
                distance[i] = Mathf.Min(distance[i], (centers[i] - center).sqrMagnitude);
                if (!selected.Contains(i) && distance[i] > farthest) { next = i; farthest = distance[i]; }
            }
        }
        var newPoints = new List<Vector3>();
        var newNormals = new List<Vector3>();
        var newUv = new List<Vector2>();
        var newTangents = new List<Vector4>();
        var map = new Dictionary<int, int>();
        // Preserve projected coverage as card count falls; do not shrink the crown into bare branches.
        float breadth = Mathf.Sqrt(1 / fraction) * .96f;
        var envelope = source.bounds;
        envelope.Expand(source.bounds.size * .04f);
        foreach (int card in selected.OrderBy(i => i))
        {
            float scale = breadth;
            // Compensate inside the crown, without expanding its outer silhouette at a switch.
            foreach (int vertex in cards[card]) for (int axis = 0; axis < 3; axis++)
            {
                float d = points[vertex][axis] - centers[card][axis];
                if (d > .00001f) scale = Mathf.Min(scale, (envelope.max[axis] - centers[card][axis]) / d);
                else if (d < -.00001f) scale = Mathf.Min(scale, (envelope.min[axis] - centers[card][axis]) / d);
            }
            foreach (int vertex in cards[card])
            {
                map.Add(vertex, newPoints.Count);
                newPoints.Add(centers[card] + (points[vertex] - centers[card]) * scale);
                newNormals.Add(normals[vertex]);
                newUv.Add(uv[vertex]);
                if (tangents.Length == points.Length) newTangents.Add(tangents[vertex]);
            }
        }
        var triangles = new List<int>();
        for (int i = 0; i < indices.Length; i += 3)
            if (map.TryGetValue(indices[i], out int a))
            { triangles.Add(a); triangles.Add(map[indices[i + 1]]); triangles.Add(map[indices[i + 2]]); }
        var mesh = new Mesh();
        mesh.SetVertices(newPoints); mesh.SetNormals(newNormals); mesh.SetUVs(0, newUv);
        if (newTangents.Count > 0) mesh.SetTangents(newTangents);
        mesh.SetTriangles(triangles, 0); mesh.RecalculateBounds();
        return mesh;
    }
}
