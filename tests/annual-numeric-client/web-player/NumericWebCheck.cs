using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Linq;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
using UnityEngine;
public sealed class NumericWebCheck : MonoBehaviour
{
 [DllImport("__Internal")] static extern void TerraNumericCheckResult(string json);
 [Serializable] sealed class Result {
  public bool passed,reloaded,dense;public int checks,year,historyCount,sequence;public string runId,unity,saveHash,reportHash,mode,error;
 }
 async void Start()
 {
  var result=new Result {unity=Application.unityVersion};
  void Check(bool ok,string why){result.checks++;if(!ok)throw new Exception(why);}
  try {
   result.runId=new Uri(Application.absoluteURL).Fragment.TrimStart('#');
   if(string.IsNullOrEmpty(result.runId))throw new Exception("Use an explicit isolated test run in the URL fragment");
   result.dense=result.runId.StartsWith("dense-projection/",StringComparison.Ordinal);
   var definition=AnnualNumericJson.ReadDefinition(Resources.Load<TextAsset>(result.dense?"dense-definition":"definition").text);
   var store=AnnualNumericBrowserStore.Create(definition,result.runId);
   AnnualNumericSave fixture=null;
   if(result.dense){
    fixture=AnnualNumericJson.ReadSave(Resources.Load<TextAsset>("dense-save").text);
    Check(fixture.town.runId==result.runId,"Dense fixture uses its exact accepted run identity");
    var stored=await store.Read(default);
    result.reloaded=stored!=null;
    if(stored==null)await store.Write(null,fixture,default);
   }
   var session=gameObject.AddComponent<AnnualTownSession>();
   await session.InitializeNumericAsync(definition,result.runId,store);
   if(session.State.history.Length==0) {
    var secondStore=AnnualNumericBrowserStore.Create(definition,result.runId);
    var secondHost=new GameObject("NumericCheckPeer");var second=secondHost.AddComponent<AnnualTownSession>();
    await second.InitializeNumericAsync(definition,result.runId,secondStore);
    var park=definition.actions.First(a=>a.actionId=="public-park");
    await session.QueueActionAsync(park.siteId,park.actionId,session.State.revision);
    Check(session.State.queue.Length==1,"Saved planning queue");
    bool stale=false;try{await second.QueueActionAsync(park.siteId,park.actionId,second.State.revision);}catch(IOException){stale=true;}
    Check(stale&&second.NumericRecoveryRequired,"Second connection rejects stale save");
    await second.ReloadNumericAsync();Check(second.State.queue.Length==1,"Second connection recovers acknowledged state");
    Destroy(secondHost);Destroy(secondStore.gameObject);
    var report=await session.ResolveNumericYearAsync(result.runId+"/year-1",session.State.revision,"missing-config");
    Check(report.simulation.transitionHash==session.State.history[0].simulation.transitionHash,"Accepted report identity matches saved state");
   } else if(!result.dense)result.reloaded=true;
   int acceptedYear=result.dense?10:1;
   Check(session.State.history.Length==acceptedYear&&session.State.phase==(result.dense?TownPhase.Completed:TownPhase.Reviewing),"Accepted years survive storage/reload");
   if(result.dense){
    Check(session.NumericSaveState.saveHash==fixture.saveHash,"Dense full-envelope hash survives .NET to IL2CPP and IndexedDB");
    var accepted=session.State.history;
    for(int year=0;year<10;year++)Check(accepted[year].simulation.transitionHash==fixture.town.history[year].simulation.transitionHash,"Dense report identity year"+(year+1));
   }
   Check(session.NumericNarrationForYear(acceptedYear).status=="visible"&&session.NumericNarrationForYear(acceptedYear).receipt.mode=="local","First saved local mode survives storage/reload");
   string hash=session.NumericSaveState.saveHash;await session.ReloadNumericAsync();
   Check(session.NumericSaveState.saveHash==hash,"Reopen preserves complete envelope hash");
   bool blocked=false;try{session.Save();}catch(InvalidOperationException){blocked=true;}
   Check(blocked,"Legacy synchronous save remains guarded in WebGL");
   result.year=session.State.year;result.historyCount=session.State.history.Length;result.sequence=(int)session.NumericSaveState.sequence;
   result.saveHash=session.NumericSaveState.saveHash;result.reportHash=session.State.history[acceptedYear-1].simulation.transitionHash;
   result.mode=session.NumericNarrationForYear(acceptedYear).receipt.mode;result.passed=true;
  } catch(Exception error){result.error=error.ToString();}
  TerraNumericCheckResult(JsonUtility.ToJson(result));
 }
}
