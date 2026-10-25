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
var session=(Terra.Gameplay.AnnualTownSession)typeof(Terra.Gameplay.TerraMain).GetField("annual",flags).GetValue(main);
var saved=UnityEngine.JsonUtility.ToJson(session.State);
var display=typeof(Terra.Gameplay.TerraMain).GetMethod("DisplayAnnual",flags);
var rendered=(System.Collections.Generic.Dictionary<string,string>)typeof(Terra.Gameplay.TerraMain).GetField("displayedAnnual",flags).GetValue(main);
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

var surface=(UnityEngine.Texture2D)type.GetField("treatmentSurface",flags).GetValue(landscape);
var baselinePixels=surface.GetPixels32();
landscape.Apply(site,"water-swale");CheckCut("queued swale before remove preview");
landscape.PreviewBaseline(site);Baseline("remove preview restores ground");
Assert(surface.GetPixels32().SequenceEqual(baselinePixels),"remove preview restores surface");
landscape.ClearGhost();CheckCut("cancel removal restores queued swale");
landscape.Apply(site,null);Baseline("remove queued swale restores baseline");
var buildingSite=world.sites.First(s=>session.Definition.actions.Any(a=>a.siteId==s.id&&a.actionId=="high-density-housing"));
landscape.Ghost(buildingSite,true,"high-density-housing");Flush();
Assert(surface.GetPixels32().SequenceEqual(baselinePixels),"building preview preserves outside source cover");
landscape.ClearGhost();landscape.Apply(buildingSite,"high-density-housing");Flush();
Assert(surface.GetPixels32().SequenceEqual(baselinePixels),"building placement preserves outside source cover");
landscape.Apply(buildingSite,null);Flush();
landscape.Apply(site,"conservation-rotation");Flush();
float sx=site.polygon.Average(p=>p.x),bottom=site.polygon.Min(p=>p.z),top=site.polygon.Max(p=>p.z);
var terrain=world.terrain;
for(int band=0;band<3;band++)
{
    float zMeadow=UnityEngine.Mathf.Lerp(bottom,top,(band+.15f)/3),zCrop=UnityEngine.Mathf.Lerp(bottom,top,(band+.65f)/3);
    float u=(sx-terrain.xMin)/((terrain.columns-1)*terrain.cellSizeM);
    float V(float z)=>(z-terrain.zMin)/((terrain.rows-1)*terrain.cellSizeM);
    Assert(surface.GetPixelBilinear(u,V(zMeadow)).r<.25f&&surface.GetPixelBilinear(u,V(zCrop)).r>.75f,"rotation surface matches planted band "+(band+1));
}
var holders=(System.Collections.Generic.Dictionary<string,UnityEngine.Transform>)type.GetField("proposals",flags).GetValue(landscape);
var natureFlags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
int planted=0;
foreach(var nature in holders[site.id].GetComponents<Terra.Rendering.InstancedNature>())
    foreach(var batch in (System.Collections.IEnumerable)typeof(Terra.Rendering.InstancedNature).GetField("batches",natureFlags).GetValue(nature))
        if((bool)batch.GetType().GetField("groundcover").GetValue(batch))planted+=((UnityEngine.Matrix4x4[])batch.GetType().GetField("transforms").GetValue(batch)).Length;
Assert(planted>0,"narrow rotation strips contain living groundcover: "+planted+" instances");
landscape.Apply(site,null);Flush();
var town=new Terra.Core.AnnualTown(session.Definition,"terrain-integration-fixture");
void Display(){display.Invoke(main,new object[]{town.Snapshot()});Flush();}
town.QueueAction(site.id,"water-swale",town.Revision);Display();CheckCut("annual queued swale");
town.UndoPlan(town.Revision);Display();Baseline("annual undo restores ground");
town.RedoPlan(town.Revision);Display();CheckCut("annual redo restores swale");
Terra.Core.AnnualTownDemo.Finish(town,new Terra.Core.AuthoredAnnualConsequenceProvider());Display();CheckCut("annual operating Year 10 swale");
var acceptedPixels=surface.GetPixels32();var acceptedCash=town.CashCents;
town=Terra.Core.AnnualTown.Restore(session.Definition,UnityEngine.JsonUtility.FromJson<Terra.Contracts.AnnualTownSnapshot>(UnityEngine.JsonUtility.ToJson(town.Snapshot())));
rendered.Clear();Display();CheckCut("accepted save replay swale");
Assert(town.CashCents==acceptedCash&&surface.GetPixels32().SequenceEqual(acceptedPixels),"accepted reload preserves ledger and surface");
display.Invoke(main,new object[]{new Terra.Core.AnnualTown(session.Definition,"baseline-comparison").Snapshot()});Baseline("read-only baseline comparison");
Display();CheckCut("return from comparison restores accepted ground");
Assert(UnityEngine.JsonUtility.ToJson(session.State)==saved,"rendering leaves live session and accepted history unchanged");
rendered.Clear();display.Invoke(main,new object[]{session.State});Baseline("restore actual scene after isolated fixture");
return results;
