// Disposable runtime HUD test. Does not publish/save or mutate the live annual engine.
var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var hud=(Terra.UI.TerraHud)main.GetType().GetField("hud",flags).GetValue(main);
var session=(Terra.Gameplay.AnnualTownSession)main.GetType().GetField("annual",flags).GetValue(main);
var ui=typeof(Terra.UI.TerraHud);
var label=(TMPro.TextMeshProUGUI)ui.GetField("actionSummary",flags).GetValue(hud);
var layout=ui.GetMethod("LayoutAnnual",flags);
var selected=(string)ui.GetField("activeTool",flags).GetValue(hud);
bool originalLarge=hud.LargeText;
string saved=session.ExportSave();
var definition=session.Definition;
var baseline=Terra.Core.AnnualGameplay.Baseline(definition);
var provider=new Terra.Core.AuthoredAnnualConsequenceProvider();
var town=new Terra.Core.AnnualTown(definition,"disposable-guidance-hud-check");
int checks=0;var failures=new System.Collections.Generic.List<string>();
var messages=new System.Collections.Generic.List<string>();
void Check(bool ok,string why){checks++;if(!ok)failures.Add(why);}
void Render(Terra.Core.AnnualTown test,string fragment)
{
    string before=UnityEngine.JsonUtility.ToJson(test.Snapshot());
    hud.Selected(null);hud.BuildAvailability(null);
    hud.AnnualState(test,baseline,baseline);
    Check(label.text.Contains(fragment),"HUD did not bind expected guidance: "+fragment+" / "+label.text);
    Check(UnityEngine.JsonUtility.ToJson(test.Snapshot())==before,"HUD mutated test engine");
    messages.Add(label.text);
}
try
{
    hud.CloseModal();
    Render(town,"inspect");
    var park=definition.actions.First(a=>a.actionId=="public-park");
    town.QueueAction(park.siteId,park.actionId,town.Revision);Render(town,"not paid");
    town.BeginResolution("guidance-hud-year1",town.Revision);Render(town,"pending");
    Terra.Core.AnnualTownDemo.Advance(town,"guidance-hud-year1",provider);Render(town,"what changed");
    town.ContinuePlanning(town.Revision);Render(town,"Housing need");
    hud.Selected("public-park");
    Check(label.text.Contains("upkeep")&&!label.text.Contains("Housing need"),"Selected project lost its own upkeep summary");
    hud.Selected(null);Check(label.text.Contains("Housing need"),"Cancel did not restore contextual guidance");
    Terra.Core.AnnualTownDemo.Finish(town,provider);Render(town,"complete");
    // Every authored fallback and every real catalog title must fit the same existing row.
    var source=System.IO.File.ReadAllText("Assets/Terra/Scripts/Core/AnnualPlanningGuidance.cs");
    foreach(System.Text.RegularExpressions.Match match in System.Text.RegularExpressions.Regex.Matches(source,"Say\\(\"([^\"]+)\"\\)"))messages.Add(match.Groups[1].Value);
    var rules=(Terra.Core.BuildActionRules)ui.GetField("actionRules",flags).GetValue(hud);
    var shortTitle=ui.GetMethod("ShortTitle",System.Reflection.BindingFlags.Static|System.Reflection.BindingFlags.NonPublic);
    foreach(var action in rules.actions)messages.Add("Output below target: consider "+shortTitle.Invoke(null,new object[]{action})+". Inspect a field, or End Year to wait.");
    foreach(bool large in new[]{false,true})
    {
        if(hud.LargeText!=large)ui.GetMethod("ToggleText",flags).Invoke(hud,null);
        foreach(var size in new[]{new UnityEngine.Vector2(1024,576),new UnityEngine.Vector2(1280,720),new UnityEngine.Vector2(1440,900),new UnityEngine.Vector2(1920,1080)})
        {
            layout.Invoke(hud,new object[]{size.x,size.y,size.x<1120||size.y<680});
            Check(label.fontSize>=(large?15.68f:14f)-.001f,"Hint did not retain readable/larger text size at "+size);
            foreach(string message in messages.Distinct())
            {
                label.text=message;label.ForceMeshUpdate(true,true);
                Check(label.preferredWidth>0&&label.preferredHeight>0&&label.preferredWidth<=label.rectTransform.rect.width+.1f&&label.preferredHeight<=label.rectTransform.rect.height+.1f,
                    "Hint overflow: "+message+" at "+size+" larger="+large+" needs "+label.preferredWidth+"x"+label.preferredHeight);
            }
        }
    }
    Check(session.ExportSave()==saved,"Disposable HUD test changed live saved state");
}
finally
{
    if(hud.LargeText!=originalLarge)ui.GetMethod("ToggleText",flags).Invoke(hud,null);
    hud.Selected(selected);
    hud.AnnualState(session.Engine,baseline,baseline);
    layout.Invoke(hud,new object[]{(float)UnityEngine.Screen.width,(float)UnityEngine.Screen.height,UnityEngine.Screen.width<1120||UnityEngine.Screen.height<680});
}
return new{checks,failures,messages=messages.Distinct().ToArray(),liveStateUnchanged=session.ExportSave()==saved};
