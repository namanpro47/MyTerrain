if(!UnityEngine.Application.isPlaying)throw new System.Exception("Play required");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
string PathOf(UnityEngine.Transform t){return t.parent==null?t.name:PathOf(t.parent)+"/"+t.name+"["+t.GetSiblingIndex()+"]";}
var rows=new System.Collections.Generic.List<string>();
string Number(float x)=>x.ToString("R",System.Globalization.CultureInfo.InvariantCulture);
string Vector(UnityEngine.Vector4 v)=>Number(v.x)+","+Number(v.y)+","+Number(v.z)+","+Number(v.w);
foreach(var n in UnityEngine.Object.FindObjectsByType<Terra.Rendering.InstancedNature>(UnityEngine.FindObjectsSortMode.None).OrderBy(n=>PathOf(n.transform))){int index=0;
foreach(var batch in (System.Collections.IEnumerable)n.GetType().GetField("batches",flags).GetValue(n)){
string prefix=PathOf(n.transform)+"/component"+System.Array.IndexOf(n.GetComponents<Terra.Rendering.InstancedNature>(),n)+"/batch"+(index++);var t=batch.GetType();
foreach(var f in t.GetFields(flags).OrderBy(f=>f.Name)){
if(f.Name=="animated")continue;var v=f.GetValue(batch);string key=prefix+"/"+f.Name;
if(v is UnityEngine.Material m){
 rows.Add(key+"/shader="+m.shader.name);rows.Add(key+"/keywords="+string.Join(",",m.shaderKeywords.OrderBy(x=>x)));rows.Add(key+"/queue="+m.renderQueue);rows.Add(key+"/instancing="+m.enableInstancing);
 for(int i=0;i<m.shader.GetPropertyCount();i++){string p=m.shader.GetPropertyName(i);var type=m.shader.GetPropertyType(i);string value;
 if(type==UnityEngine.Rendering.ShaderPropertyType.Color)value=Vector(m.GetColor(p));
 else if(type==UnityEngine.Rendering.ShaderPropertyType.Vector)value=Vector(m.GetVector(p));
 else if(type==UnityEngine.Rendering.ShaderPropertyType.Texture){var texture=m.GetTexture(p);value=texture==null?"null":UnityEditor.AssetDatabase.GetAssetPath(texture)+":"+texture.name;rows.Add(key+"/"+p+"/scale="+Vector(m.GetTextureScale(p)));rows.Add(key+"/"+p+"/offset="+Vector(m.GetTextureOffset(p)));}
 else if(type==UnityEngine.Rendering.ShaderPropertyType.Int)value=m.GetInteger(p).ToString();else value=Number(m.GetFloat(p));
 rows.Add(key+"/"+p+"="+value);
 }
}else if(v is UnityEngine.Mesh mesh){rows.Add(key+"="+mesh.name+":"+mesh.vertexCount+":"+mesh.subMeshCount);}
else if(v is UnityEngine.Matrix4x4[] matrices){using(var stream=new System.IO.MemoryStream())using(var w=new System.IO.BinaryWriter(stream)){foreach(var matrix in matrices)for(int i=0;i<16;i++)w.Write(matrix[i]);w.Flush();rows.Add(key+"="+System.BitConverter.ToString(System.Security.Cryptography.SHA256.Create().ComputeHash(stream.ToArray())));}}
else if(v is UnityEngine.Bounds b)rows.Add(key+"="+Vector(b.center)+","+Vector(b.size));
else rows.Add(key+"="+(v==null?"null":System.Convert.ToString(v,System.Globalization.CultureInfo.InvariantCulture)));
}}}
System.IO.File.WriteAllLines("Builds/MaterialSharing/appearance.txt",rows);
return new{rows=rows.Count,frame=UnityEngine.Time.frameCount};
