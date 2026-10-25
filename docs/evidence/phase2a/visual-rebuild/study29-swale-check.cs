var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var type=typeof(Terra.Rendering.LandscapeRenderer);
var landscape=(Terra.Rendering.LandscapeRenderer)typeof(Terra.Gameplay.TerraMain).GetField("landscape",flags).GetValue(main);
var world=(Terra.Contracts.WorldData)typeof(Terra.Gameplay.TerraMain).GetField("world",flags).GetValue(main);
var site=world.sites.First(s=>s.id=="south-07");
var chunks=((System.Collections.IEnumerable)type.GetField("groundChunks",flags).GetValue(landscape)).Cast<object>().ToArray();
var meshes=chunks.Select(c=>(UnityEngine.Mesh)c.GetType().GetField("mesh").GetValue(c)).ToArray();
var originals=meshes.Select(m=>m.vertices).ToArray();var normals=meshes.Select(m=>m.normals).ToArray();
var height=landscape.Height(-30,-395);var cover=landscape.CoverAt(-30,-395);
var results=new System.Collections.Generic.List<string>();
void Assert(bool value,string label){if(!value)throw new System.Exception(label);results.Add(label);}
void Flush(){landscape.ViewScale(.25f);}
void Baseline(string label){Flush();for(int c=0;c<meshes.Length;c++)if(!meshes[c].vertices.SequenceEqual(originals[c])||!meshes[c].normals.SequenceEqual(normals[c]))throw new System.Exception(label+" chunk "+c);results.Add(label);}
var contains=type.GetMethod("Contains",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public);
int CheckCut(string label){Flush();int changed=0;float deepest=0;var edge=new System.Collections.Generic.Dictionary<UnityEngine.Vector2,(UnityEngine.Vector3 point,UnityEngine.Vector3 normal)>();int duplicates=0;
 for(int c=0;c<meshes.Length;c++){var ps=meshes[c].vertices;var ns=meshes[c].normals;
 for(int i=0;i<ps.Length;i++){var p=ps[i];var o=originals[c][i];float cut=o.y-p.y;
 if(cut<-.0001f||cut>1.1501f||p.x!=o.x||p.z!=o.z)throw new System.Exception(label+" invalid displacement");
 if(cut>.0001f){changed++;deepest=UnityEngine.Mathf.Max(deepest,cut);if(!(bool)contains.Invoke(null,new object[]{site.polygon,p.x,p.z}))throw new System.Exception(label+" outside site");}
 if(p.x<-100||p.x>50||p.z<-470||p.z>-310)continue;
 var key=new UnityEngine.Vector2(p.x,p.z);if(edge.TryGetValue(key,out var prior)){duplicates++;if((prior.point-p).sqrMagnitude>1e-9||(prior.normal-ns[i]).sqrMagnitude>1e-8)throw new System.Exception(label+" chunk seam");}else edge.Add(key,(p,ns[i]));
 }}Assert(changed>100&&deepest>1.1f,label+": "+changed+" lowered vertices; deepest "+deepest+" m; "+duplicates+" shared vertices match");return changed;}
Baseline("initial ground");
var timer=System.Diagnostics.Stopwatch.StartNew();landscape.Ghost(site,true,"water-swale");Flush();timer.Stop();results.Add("Editor preview construction plus surface/ground flush: "+timer.Elapsed.TotalMilliseconds+" ms (not browser latency)");
CheckCut("valid preview");landscape.ClearGhost();Baseline("preview cancel restores positions and normals exactly");
landscape.Apply(site,"water-swale");CheckCut("accepted swale");
landscape.Ghost(site,true,"restore-grass");Baseline("replacement preview removes depression");landscape.ClearGhost();CheckCut("replacement cancel restores accepted depression");
landscape.Apply(site,null);Baseline("undo restores positions and normals exactly");
Assert(landscape.Height(-30,-395)==height&&landscape.CoverAt(-30,-395)==cover,"analytical height and cover unchanged");
var shaderMessages=UnityEditor.ShaderUtil.GetShaderMessages(UnityEngine.Resources.Load<UnityEngine.Shader>("TerraGround"));
Assert(!shaderMessages.Any(m=>m.severity==UnityEditor.Rendering.ShaderCompilerMessageSeverity.Error),"ground shader has no errors");
foreach(var message in shaderMessages)results.Add("shader message: "+message.severity+" "+message.message);
landscape.Apply(site,"water-swale");Flush();
typeof(Terra.Gameplay.TerraMain).GetField("hoverId",flags).SetValue(main,site.id);typeof(Terra.Gameplay.TerraMain).GetMethod("FocusSite",flags).Invoke(main,null);
return results;
