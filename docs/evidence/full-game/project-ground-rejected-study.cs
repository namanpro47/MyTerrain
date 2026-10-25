var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var landscape=typeof(Terra.Gameplay.TerraMain).GetField("landscape",flags).GetValue(main);
var proposals=(System.Collections.Generic.Dictionary<string,UnityEngine.Transform>)landscape.GetType().GetField("proposals",flags).GetValue(landscape);
var holder=proposals["south-04"];
var results=new System.Collections.Generic.List<object>();
foreach(var filter in holder.GetComponentsInChildren<UnityEngine.MeshFilter>())
{
    if(filter.name!="grass"&&filter.name!="path")continue;
    bool grass=filter.name=="grass";
    var mesh=UnityEngine.Object.Instantiate(filter.sharedMesh);mesh.name="Temporary painted "+filter.name;
    var vertices=mesh.vertices;var normals=mesh.normals;var uv=new UnityEngine.Vector2[vertices.Length];
    for(int i=0;i<uv.Length;i++)
    {
        var p=filter.transform.TransformPoint(vertices[i]);var n=filter.transform.TransformDirection(normals[i]);
        uv[i]=(UnityEngine.Mathf.Abs(n.y)>.6f?new UnityEngine.Vector2(p.x,p.z):UnityEngine.Mathf.Abs(n.x)>.6f?new UnityEngine.Vector2(p.z,p.y):new UnityEngine.Vector2(p.x,p.y))/(grass?40:9);
    }
    mesh.uv=uv;mesh.RecalculateTangents();filter.sharedMesh=mesh;
    var renderer=filter.GetComponent<UnityEngine.MeshRenderer>();var material=new UnityEngine.Material(renderer.sharedMaterial);
    material.name="Temporary painted "+filter.name;
    material.SetTexture("_BaseMap",UnityEngine.Resources.Load<UnityEngine.Texture2D>(grass?"JulioGround/MeadowColor":"JulioGround/EarthColor"));
    material.SetTexture("_BumpMap",UnityEngine.Resources.Load<UnityEngine.Texture2D>(grass?"JulioGround/MeadowNormal":"JulioGround/EarthNormal"));
    material.EnableKeyword("_NORMALMAP");material.SetFloat("_BumpScale",.25f);material.SetFloat("_Smoothness",.08f);
    material.SetColor("_BaseColor",grass?new UnityEngine.Color(.70f,.93f,.72f):new UnityEngine.Color(.85f,.87f,.83f));
    renderer.sharedMaterial=material;
    results.Add(new{part=filter.name,vertices=mesh.vertexCount,bounds=renderer.bounds.ToString()});
}
return results;
