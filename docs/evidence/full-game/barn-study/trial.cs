var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
var landscape=(Terra.Rendering.LandscapeRenderer)main.GetType().GetField("landscape",flags).GetValue(main);
var proposals=(System.Collections.Generic.Dictionary<string,UnityEngine.Transform>)typeof(Terra.Rendering.LandscapeRenderer).GetField("proposals",flags).GetValue(landscape);
var instance=proposals["south-01"].Find(main.buildActionCatalog.Find("agricultural-storage").title);
foreach(var part in instance.GetComponentsInChildren<UnityEngine.MeshFilter>()){
 if(part.name=="grass"||part.name=="path")continue;
 if(part.name!="roof"&&part.name!="steel"){part.GetComponent<UnityEngine.Renderer>().enabled=false;continue;}
 var source=part.sharedMesh;var v=source.vertices;var t=source.triangles;var kept=new System.Collections.Generic.List<int>();
 for(int i=0;i<t.Length;i+=3)if((v[t[i]].x+v[t[i+1]].x+v[t[i+2]].x)/3>12)kept.AddRange(new[]{t[i],t[i+1],t[i+2]});
 var copy=UnityEngine.Object.Instantiate(source);copy.triangles=kept.ToArray();copy.RecalculateBounds();part.sharedMesh=copy;
}
var sourceModel=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Editor/BarnStudy/barnlvl1.fbx");
var filter=sourceModel.GetComponentInChildren<UnityEngine.MeshFilter>();var original=filter.sharedMesh;var matrix=filter.transform.localToWorldMatrix;
var vertices=original.vertices.Select(v=>matrix.MultiplyPoint3x4(v)).ToArray();var bounds=new UnityEngine.Bounds(vertices[0],UnityEngine.Vector3.zero);foreach(var p in vertices)bounds.Encapsulate(p);
float scale=8.5f/bounds.size.y;var shift=new UnityEngine.Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
var mesh=UnityEngine.Object.Instantiate(original);mesh.vertices=vertices.Select(v=>(v-shift)*scale).ToArray();mesh.normals=original.normals.Select(n=>matrix.MultiplyVector(n).normalized).ToArray();mesh.RecalculateBounds();
var material=new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Unlit"));material.SetTexture("_BaseMap",UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>("Assets/Editor/BarnStudy/barnlvl1-palette-0.png"));material.SetColor("_BaseColor",UnityEngine.Color.white);material.SetFloat("_Cull",0);
foreach(float x in new[]{-14f,-4f,6f}){var go=new UnityEngine.GameObject("Artist barn trial");go.transform.SetParent(instance,false);go.transform.localPosition=new UnityEngine.Vector3(x,.12f,-6);go.AddComponent<UnityEngine.MeshFilter>().sharedMesh=mesh;go.AddComponent<UnityEngine.MeshRenderer>().sharedMaterial=material;}
return new{frame=UnityEngine.Time.frameCount,nativeBounds=bounds.ToString(),uniformScale=scale,adaptedBounds=mesh.bounds.ToString(),triangles=mesh.triangles.Length/3};
