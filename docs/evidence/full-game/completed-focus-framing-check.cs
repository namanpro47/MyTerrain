var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var type=main.GetType();
object Field(string name)=>type.GetField(name,flags).GetValue(main);
void Set(string name,object value)=>type.GetField(name,flags).SetValue(main,value);
var world=(Terra.Contracts.WorldData)Field("world");
var rules=(Terra.Core.BuildActionRules)Field("buildActionRules");
var session=(Terra.Gameplay.AnnualTownSession)Field("annual");
var hud=(Terra.UI.TerraHud)Field("hud");
var endingFixture=new UnityEngine.GameObject("Ending framing fixture");
var endingSession=endingFixture.AddComponent<Terra.Gameplay.AnnualTownSession>();
endingSession.Initialize(session.Definition,"ending-framing");endingSession.FinishOffline();
var baseline=Terra.Core.AnnualGameplay.Baseline(session.Definition);
hud.AnnualState(endingSession.Engine,baseline,Terra.Core.AnnualGameplay.Forecast(endingSession.Engine).outcomes);
// Screen.width in an Editor eval reflects the invoking editor window. Lay out against
// the actual Game-view canvas, then validate the camera's real viewport projection.
var canvas=UnityEngine.Object.FindObjectsByType<UnityEngine.Canvas>(UnityEngine.FindObjectsSortMode.None).First(c=>c.name=="Terra HUD");
float width=canvas.pixelRect.width,height=canvas.pixelRect.height;
var uiFlags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var dock=(UnityEngine.Transform)typeof(Terra.UI.TerraHud).GetField("actionDock",uiFlags).GetValue(hud);
dock.localScale=UnityEngine.Vector3.one*UnityEngine.Mathf.Min(1,(width-48)/760);
typeof(Terra.UI.TerraHud).GetMethod("LayoutAnnual",uiFlags).Invoke(hud,new object[]{width,height,width<1120||height<680});
var landscape=(Terra.Rendering.LandscapeRenderer)Field("landscape");
var camera=(UnityEngine.Camera)Field("view");
var safe=hud.LandscapeFocusViewport();
if(safe.width<=0||safe.height<=0)throw new System.Exception("Test requires a supported game viewport: "+UnityEngine.Screen.width+"x"+UnityEngine.Screen.height);
var rows=new System.Collections.Generic.List<object>();
foreach(bool perspective in new[]{false,true})foreach(var offer in session.Definition.actions)
{
    var site=world.sites.First(s=>s.id==offer.siteId);var rule=rules.Find(offer.actionId);
    Set("hoverId",site.id);Set("treatmentId",offer.actionId);Set("perspective",perspective);
    type.GetMethod("FocusSite",flags).Invoke(main,null);
    foreach(var pair in new[]{("shownYaw","yaw"),("shownPitch","pitch"),("shownZoom","zoom"),("shownTarget","target")})Set(pair.Item1,Field(pair.Item2));
    type.GetMethod("RenderView",flags).Invoke(main,null);
    var points=site.polygon.Select(p=>new UnityEngine.Vector3(p.x,landscape.Height(p.x,p.z),p.z)).ToList();
    var art=main.buildActionCatalog.Find(offer.actionId);var layout=rules.Layout(site,rule);
    if(!rule.landscape&&layout.Valid)
    {
        // Independently inspect the actual source renderer bounds rather than its catalog summary.
        var origin=new UnityEngine.Vector3(layout.x,layout.groundY,layout.z);var rotation=UnityEngine.Quaternion.Euler(0,layout.yaw,0);
        foreach(var renderer in art.prefab.GetComponentsInChildren<UnityEngine.Renderer>(true))
        {
            var b=renderer.bounds;
            foreach(int x in new[]{-1,1})foreach(int y in new[]{-1,1})foreach(int z in new[]{-1,1})
                points.Add(origin+rotation*(b.center+UnityEngine.Vector3.Scale(b.extents,new UnityEngine.Vector3(x,y,z))));
        }
    }
    foreach(var point in points)
    {
        var p=camera.WorldToViewportPoint(point);
        if(p.z<=0||p.x<safe.xMin-.001||p.x>safe.xMax+.001||p.y<safe.yMin-.001||p.y>safe.yMax+.001)
            throw new System.Exception(offer.actionId+" at "+site.id+" perspective="+perspective+" outside clear viewport: "+p+" safe "+safe);
    }
    rows.Add(new{offer.actionId,offer.siteId,perspective,zoom=(float)Field("zoom"),checkedPoints=points.Count});
}
Set("hoverId",null);Set("treatmentId","");type.GetMethod("ResetView",flags).Invoke(main,null);
hud.AnnualState(session.Engine,baseline,Terra.Core.AnnualGameplay.Forecast(session.Engine).outcomes);
UnityEngine.Object.DestroyImmediate(endingFixture);
return new{width,height,safe=safe.ToString(),cases=rows.Count,rows};
