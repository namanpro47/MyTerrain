var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var source=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>("Assets/Editor/CozyCropTrial/wheat.fbx").GetComponentInChildren<UnityEngine.MeshFilter>();
var mesh=UnityEngine.Object.Instantiate(source.sharedMesh);mesh.name="Cozy wheat trial";
var vertices=mesh.vertices.Select(v=>source.transform.TransformVector(v)).ToArray();
var min=new UnityEngine.Vector3(vertices.Min(v=>v.x),vertices.Min(v=>v.y),vertices.Min(v=>v.z));
var max=new UnityEngine.Vector3(vertices.Max(v=>v.x),vertices.Max(v=>v.y),vertices.Max(v=>v.z));
var center=(min+max)*.5f;center.y=min.y;float scale=2.3f/(max.y-min.y);
mesh.vertices=vertices.Select(v=>(v-center)*scale).ToArray();mesh.normals=mesh.normals.Select(n=>source.transform.TransformDirection(n).normalized).ToArray();mesh.RecalculateBounds();
var mat=new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Unlit"));mat.name="Cozy native palette trial";mat.SetTexture("_BaseMap",UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Texture2D>("Assets/Editor/CozyCropTrial/palette.png"));mat.SetColor("_BaseColor",UnityEngine.Color.white);mat.SetFloat("_Cull",0);mat.enableInstancing=true;
int count=0,plants=0;
var boundsMethod=typeof(Terra.Rendering.InstancedNature).GetMethod("TransformBounds",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
foreach(var nature in UnityEngine.Object.FindObjectsByType<Terra.Rendering.InstancedNature>(UnityEngine.FindObjectsSortMode.None)){
var batches=(System.Collections.IEnumerable)typeof(Terra.Rendering.InstancedNature).GetField("batches",flags).GetValue(nature);
foreach(var batch in batches){var bt=batch.GetType();if(!(bool)bt.GetField("crop").GetValue(batch))continue;bt.GetField("mesh").SetValue(batch,mesh);bt.GetField("material").SetValue(batch,mat);
var transforms=(UnityEngine.Matrix4x4[])bt.GetField("transforms").GetValue(batch);var bounds=new UnityEngine.Bounds();float height=0;
for(int i=0;i<transforms.Length;i++){var b=(UnityEngine.Bounds)boundsMethod.Invoke(null,new object[]{mesh.bounds,transforms[i]});if(i==0)bounds=b;else bounds.Encapsulate(b);height=UnityEngine.Mathf.Max(height,b.size.y);}
bt.GetField("bounds").SetValue(batch,bounds);bt.GetField("maxHeight").SetValue(batch,height);count++;plants+=transforms.Length;
}}
return new {batches=count,plants,vertices=mesh.vertexCount,sourceHeight=max.y-min.y,bounds=mesh.bounds.ToString()};
