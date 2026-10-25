if(!UnityEngine.Application.isPlaying)throw new System.Exception("Play required");
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
if(main==null||string.IsNullOrEmpty(main.Diagnostic))throw new System.Exception("Initialized game required");
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var live=(Terra.Gameplay.AnnualTownSession)typeof(Terra.Gameplay.TerraMain).GetField("annual",flags).GetValue(main);
string original=UnityEngine.JsonUtility.ToJson(live.State);
var checks=new System.Collections.Generic.List<string>();void Check(bool yes,string label){if(!yes)throw new System.Exception(label);checks.Add(label);}
var go=new UnityEngine.GameObject("Chapter teardown regression");
var town=go.AddComponent<Terra.Gameplay.AnnualTownSession>();town.Initialize(live.Definition,"chapter-teardown-fixture");
var chapter=go.AddComponent<Terra.Gameplay.AnnualChapterSession>();chapter.autoSave=false;chapter.town=town;
var listener=new System.Net.Sockets.TcpListener(System.Net.IPAddress.Loopback,0);listener.Start();
chapter.endpoint="http://127.0.0.1:"+((System.Net.IPEndPoint)listener.LocalEndpoint).Port+"/chapter";
int events=0;chapter.StateChanged+=()=>events++;
try{
 chapter.ResolveOnline();Check(chapter.Busy,"Local in-flight request exists");Check(town.State.phase==Terra.Contracts.TownPhase.Resolving,"Year is frozen before transport cancellation");
 string frozen=UnityEngine.JsonUtility.ToJson(town.State);int beforeDisable=events;
 chapter.enabled=false;Check(!chapter.Busy&&!chapter.Finishing,"Disable releases transport and finishing state");Check(events==beforeDisable,"Disable does not publish into teardown subscribers");Check(UnityEngine.JsonUtility.ToJson(town.State)==frozen,"Disable preserves exact pending request and budget");
 chapter.enabled=true;chapter.ResolveOffline();Check(town.State.phase==Terra.Contracts.TownPhase.Reviewing&&town.State.history.Length==1,"Re-enabled session accepts same year offline exactly once");
 var after=UnityEngine.JsonUtility.ToJson(town.State);chapter.ResolveOffline();Check(UnityEngine.JsonUtility.ToJson(town.State)==after,"Repeated resolution cannot accept or charge the year twice");
 int beforeStop=events;chapter.StopRequest();Check(events==beforeStop+1,"Explicit player stop still publishes");
 chapter.Continue();chapter.ResolveOnline();Check(chapter.Busy&&town.State.year==2,"Next-year transport starts normally");
 string next=UnityEngine.JsonUtility.ToJson(town.State);int beforeDeactivate=events;
 go.SetActive(false);Check(!chapter.Busy&&events==beforeDeactivate,"GameObject deactivation silently releases transport");Check(UnityEngine.JsonUtility.ToJson(town.State)==next,"Deactivation preserves next-year pending request");
 Check(UnityEngine.JsonUtility.ToJson(live.State)==original,"Live game and ledger remain unchanged");
 int beforeDestroy=events;UnityEngine.Object.DestroyImmediate(go);Check(events==beforeDestroy,"Destroying disabled session publishes no callback");
 return new{count=checks.Count,checks,events,frame=UnityEngine.Time.frameCount,state=main.Diagnostic};
}finally{if(go!=null)UnityEngine.Object.DestroyImmediate(go);listener.Stop();}
