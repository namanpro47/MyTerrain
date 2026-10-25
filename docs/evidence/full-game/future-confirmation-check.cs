var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();var mainType=main.GetType();var original=(Terra.Gameplay.AnnualTownSession)mainType.GetField("annual",flags).GetValue(main);var hud=(Terra.UI.TerraHud)mainType.GetField("hud",flags).GetValue(main);var originalSaved=UnityEngine.JsonUtility.ToJson(original.State);
var fixture=new UnityEngine.GameObject("Future confirmation fixture").AddComponent<Terra.Gameplay.AnnualTownSession>();fixture.Initialize(original.Definition,"future-confirmation-fixture");fixture.QueueAction("south-01","restore-grass",fixture.State.revision);
var checks=new System.Collections.Generic.List<string>();var views=new System.Collections.Generic.List<object>();
void Assert(bool v,string message){if(!v)throw new System.Exception(message);checks.Add(message);}
void Verify(string label){
 var before=UnityEngine.JsonUtility.ToJson(fixture.State);var expected=Terra.Core.AnnualTown.Restore(fixture.Definition,fixture.State);bool reviewing=expected.Phase==Terra.Contracts.TownPhase.Reviewing;
 if(reviewing)expected.ContinuePlanning(expected.Revision);
 var report=Terra.Core.AnnualTownDemo.Advance(expected,expected.PendingRequest?.requestId??"independent-check-"+expected.Year,new Terra.Core.AuthoredAnnualConsequenceProvider());
 mainType.GetMethod("ReviewFuture",flags).Invoke(main,null);string text=((TMPro.TextMeshProUGUI)typeof(Terra.UI.TerraHud).GetField("modalText",flags).GetValue(hud)).text;
 Assert(text.Contains("expected Year "+report.year+" closing cash "+Terra.UI.TerraHud.Money(report.ledger.closingCashCents)),label+": confirmation matches independently advanced next report");
 Assert(text.Contains(reviewing?"Continue from accepted Year":"Submit the current reviewed plan"),label+": introduction matches phase");
 Assert(UnityEngine.JsonUtility.ToJson(fixture.State)==before,label+": opening confirmation leaves live state byte-identical");hud.CloseModal();
 Assert(UnityEngine.JsonUtility.ToJson(fixture.State)==before,label+": cancel leaves live state byte-identical");
 views.Add(new{label,nextYear=report.year,cash=report.ledger.closingCashCents,text});
}
try{
 mainType.GetField("annual",flags).SetValue(main,fixture);
 Verify("Planning Year1");
 fixture.BeginYear("fixture-year1",fixture.State.revision);Verify("Resolving Year1");
 fixture.CompleteYear(new Terra.Core.AuthoredAnnualConsequenceProvider().Resolve(fixture.State.pendingRequest));Verify("Reviewing Year1");
 while(fixture.State.year<9){fixture.ContinueYear(fixture.State.revision);fixture.EndYearOffline("fixture-year"+fixture.State.year);}
 Verify("Reviewing Year9");
 Assert(UnityEngine.JsonUtility.ToJson(original.State)==originalSaved,"original player run unchanged");
}finally{mainType.GetField("annual",flags).SetValue(main,original);hud.CloseModal();UnityEngine.Object.DestroyImmediate(fixture.gameObject);}
return new{checks=checks.Count,results=checks,views,frame=UnityEngine.Time.frameCount};
