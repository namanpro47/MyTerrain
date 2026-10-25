var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var checks=new System.Collections.Generic.List<string>();
void Assert(bool v,string message){if(!v)throw new System.Exception(message);checks.Add(message);}
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
var session=(Terra.Gameplay.AnnualTownSession)main.GetType().GetField("annual",flags).GetValue(main);
var world=(Terra.Contracts.WorldData)main.GetType().GetField("world",flags).GetValue(main);
var landscape=(Terra.Rendering.LandscapeRenderer)main.GetType().GetField("landscape",flags).GetValue(main);
string saved=UnityEngine.JsonUtility.ToJson(session.State),terrain=UnityEngine.JsonUtility.ToJson(world.terrain);
var textureShader=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Shader>("Assets/Editor/TintTypeFixture/Texture.shader");
var textureMaterial=new UnityEngine.Material(textureShader);textureMaterial.SetTexture("_BaseColor",UnityEngine.Texture2D.blackTexture);
var colorMaterial=new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Unlit"));var original=new UnityEngine.Color(.2f,.4f,.6f,.37f);colorMaterial.SetColor("_BaseColor",original);
Assert(textureMaterial.HasProperty("_BaseColor")&&!textureMaterial.HasColor("_BaseColor"),"fixture exercises texture/color name collision");
var host=new UnityEngine.GameObject("Tint type fixture");var nature=host.AddComponent<Terra.Rendering.InstancedNature>();
var prefab=UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Quad);prefab.name="Tint fixture source";
var renderer=prefab.GetComponent<UnityEngine.MeshRenderer>();renderer.sharedMaterial=textureMaterial;nature.Add(prefab,new System.Collections.Generic.List<UnityEngine.Matrix4x4>{UnityEngine.Matrix4x4.identity},UnityEngine.Color.white);
renderer.sharedMaterial=colorMaterial;nature.Add(prefab,new System.Collections.Generic.List<UnityEngine.Matrix4x4>{UnityEngine.Matrix4x4.identity},UnityEngine.Color.white);
var owned=(System.Collections.Generic.List<UnityEngine.Material>)typeof(Terra.Rendering.InstancedNature).GetField("ownedMaterials",flags).GetValue(nature);
var tint=new UnityEngine.Color(.7f,.1f,.2f,.1f);nature.SetTint(tint,.65f);
var expected=UnityEngine.Color.Lerp(original,tint,.65f);expected.a=original.a;
Assert(UnityEngine.Vector4.Distance(owned[1].GetColor("_BaseColor"),expected)<.00001f,"color material receives requested tint and preserves alpha");
Assert(owned[0].GetTexture("_BaseColor")==UnityEngine.Texture2D.blackTexture,"texture property remains intact");
var originals=(System.Collections.Generic.Dictionary<UnityEngine.Material,UnityEngine.Color>)typeof(Terra.Rendering.InstancedNature).GetField("originalTints",flags).GetValue(nature);
Assert(originals.Count==1&&!originals.ContainsKey(owned[0]),"unsupported texture property never enters original-color cache");
nature.SetTint(tint,.65f);Assert(UnityEngine.Vector4.Distance(owned[1].GetColor("_BaseColor"),expected)<.00001f,"supported tint remains idempotent with mixed shader types");
nature.SetTint(tint,0);Assert(UnityEngine.Vector4.Distance(owned[1].GetColor("_BaseColor"),original)<.00001f,"supported color and alpha restore exactly");
Assert(colorMaterial.GetColor("_BaseColor")==original&&textureMaterial.GetTexture("_BaseColor")==UnityEngine.Texture2D.blackTexture,"source materials remain unchanged");
var site=world.sites.First(s=>s.id=="south-01");var holder=landscape.Apply(site,"cover-crop");
var child=UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Quad);child.name="Texture property ordinary Renderer";child.transform.SetParent(holder,false);var ordinary=child.GetComponent<UnityEngine.MeshRenderer>();ordinary.sharedMaterial=textureMaterial;
var sentinel=new UnityEngine.MaterialPropertyBlock();sentinel.SetFloat("_FixtureSentinel",42);ordinary.SetPropertyBlock(sentinel);
foreach(var stage in new[]{"PROPOSED","SUSPENDED","ESTABLISHING"}){
 landscape.AnnualStage(site,stage,.25);var block=new UnityEngine.MaterialPropertyBlock();ordinary.GetPropertyBlock(block);
 Assert(block.GetFloat("_FixtureSentinel")==42,stage+" leaves unsupported ordinary Renderer property block intact");
 Assert(ordinary.sharedMaterial.GetTexture("_BaseColor")==UnityEngine.Texture2D.blackTexture,stage+" preserves native texture assignment");
}
landscape.Apply(site,null);
Assert(UnityEngine.JsonUtility.ToJson(session.State)==saved,"live annual ledger byte-identical");Assert(UnityEngine.JsonUtility.ToJson(world.terrain)==terrain,"analytical terrain byte-identical");
UnityEngine.Object.DestroyImmediate(host);UnityEngine.Object.DestroyImmediate(prefab);UnityEngine.Object.DestroyImmediate(colorMaterial);UnityEngine.Object.DestroyImmediate(textureMaterial);
return new{checks=checks.Count,results=checks,frame=UnityEngine.Time.frameCount};
