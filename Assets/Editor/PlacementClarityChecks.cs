using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Terra.Art;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
using Terra.Rendering;
using Terra.UI;
using TMPro;
using UnityEditor;
using UnityEngine;

// Structural/transaction checks complement the required real Chrome walkthrough.
public static class PlacementClarityChecks
{
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static int checks;
    static void Check(bool valid,string reason){checks++;if(!valid)throw new Exception("Placement clarity: "+reason);}
    static T Field<T>(object value,string name)=>(T)value.GetType().GetField(name,Private).GetValue(value);
    static void Invoke(object value,string name,params object[] args)=>value.GetType().GetMethod(name,Private).Invoke(value,args);
    static T Read<T>(string name)=>JsonUtility.FromJson<T>(File.ReadAllText("worlds/meadowbrook-v1/"+name+".json"));
    static Rect WorldBounds(RectTransform rect)
    {
        var corners=new Vector3[4];rect.GetWorldCorners(corners);
        return Rect.MinMaxRect(corners.Min(p=>p.x),corners.Min(p=>p.y),corners.Max(p=>p.x),corners.Max(p=>p.y));
    }

    public static void RunBatch()
    {
        checks=0;
        var world=Read<WorldData>("world");var scenario=Read<Scenario>("scenario");var hydro=Read<RunoffConfiguration>("hydrology");
        AnnualSpatialOpportunities.ConnectFields(world,hydro,JsonUtility.FromJson<WorldData>(Resources.Load<TextAsset>("ConnectedFields").text).sites);
        AnnualSpatialOpportunities.AddWind(world,hydro);
        var catalog=AssetDatabase.LoadAssetAtPath<BuildActionCatalog>("Assets/Terra/Art/ExternalKit/BuildActions/BuildActionCatalog.asset");
        var rules=BuildActionRuntime.Expand(catalog,ref world,ref scenario);
        var definition=FullAnnualGameplay.Create(world,scenario,"placement-checks",rules,new LandscapeRunoff(world,scenario,hydro));
        var town=new AnnualTown(definition,"placement-clarity");var baseline=AnnualGameplay.Baseline(definition);
        var events=new GameObject("Placement check events").AddComponent<UnityEngine.EventSystems.EventSystem>();
        // Edit-mode batch runners do not call EventSystem.OnEnable automatically.
        typeof(UnityEngine.EventSystems.EventSystem).GetMethod("OnEnable",Private).Invoke(events,null);
        var hud=new TerraHud(_=>{},()=>{},()=>{},()=>{},()=>{},()=>{});
        hud.ConfigureEditing(()=>{},()=>{},()=>{},()=>{});hud.ConfigureBuildActions(catalog,rules,world,_=>{});
        hud.ConfigureAnnual(definition,()=>{},()=>{},()=>{});hud.ConfigureFuture(()=>{});hud.Ready();
        hud.AnnualState(town,baseline,baseline);hud.PlacementAcceptedState(town,baseline);
        string before=JsonUtility.ToJson(town.Snapshot());
        foreach(bool large in new[]{false,true})
        {
            if(large)Invoke(hud,"ToggleText");
            foreach(var size in new[]{new Vector2(1024,576),new Vector2(1280,720),new Vector2(1440,900),new Vector2(1920,1080)})
            {
                Invoke(hud,"LayoutAnnual",size.x,size.y,size.x<1120||size.y<680);
                hud.HideAnnualPreview();var dock=(RectTransform)Field<Transform>(hud,"actionDock");
                Vector2 position=dock.anchoredPosition,dimensions=dock.sizeDelta;Vector3 scale=dock.localScale;
                foreach(var offer in definition.actions.GroupBy(a=>a.actionId).Select(group=>group.First()))
                {
                    var site=world.sites.First(s=>s.id==offer.siteId);hud.Selected(offer.actionId);
                    Canvas.ForceUpdateCanvases();
                    var cards=Field<System.Collections.Generic.Dictionary<UnityEngine.UI.Button,string>>(hud,"actionButtons");
                    var card=cards.First(pair=>pair.Value==offer.actionId).Key;
                    Check(card.gameObject.activeInHierarchy,"Selected tool is hidden on another page: "+offer.actionId);
                    var instruction=WorldBounds(Field<TextMeshProUGUI>(hud,"actionSummary").rectTransform);
                    foreach(var visibleCard in cards.Keys.Where(b=>b.gameObject.activeInHierarchy))
                    {
                        var bounds=WorldBounds((RectTransform)visibleCard.transform);
                        Check(!bounds.Overlaps(instruction)&&bounds.yMin>instruction.yMax,
                            "Project card covers dock instruction at "+size+" (large text="+large+"): "+cards[visibleCard]);
                    }
                    foreach(var text in card.GetComponentsInChildren<TextMeshProUGUI>())
                    {
                        text.ForceMeshUpdate();
                        Check(text.preferredHeight<=text.rectTransform.rect.height+2,"Clipped tool label at "+size+": "+text.text);
                    }
                    foreach(string reason in new string[]{null,"Ground too uneven. Choose another marked field."})
                    {
                        hud.AnnualPreview(site,offer,town.Snapshot(),"place",reason,()=>{},false);
                        var rule=rules.Find(offer.actionId);
                        string space=rule.landscape?"Treats this whole field":$"{rule.width:0} × {rule.depth:0} m footprint · reserves this field's project slot";
                        hud.HidePlacementFeedback();hud.Toast("Focused site");
                        Check(Field<GameObject>(hud,"toastPanel").activeSelf,"Ordinary feedback is hidden outside placement");
                        hud.PlacementFeedback(reason==null?"PREVIEW":"BLOCKED",offer.title,site.name,
                            reason??"Reserve $70,000 · Click to queue",space,reason,7000000,993000000,Array.Empty<Vector3>(),null);
                        Check(!Field<GameObject>(hud,"toastPanel").activeSelf,"Existing toast covers candidate guidance");
                        hud.Toast("Choose an empty destination");
                        Check(!Field<GameObject>(hud,"toastPanel").activeSelf,"New toast covers candidate guidance");
                        Check(position==dock.anchoredPosition&&dimensions==dock.sizeDelta&&scale==dock.localScale,"Hover moved the dock for "+offer.actionId+" at "+size);
                        Check(!Field<Transform>(hud,"placementCapsule").GetComponent<CanvasGroup>().blocksRaycasts,"Feedback intercepts placement clicks");
                        Canvas.ForceUpdateCanvases();
                        foreach(var label in Field<Transform>(hud,"projectFacts").GetComponentsInChildren<TextMeshProUGUI>())
                        {
                            if(!label.gameObject.activeInHierarchy)continue;label.ForceMeshUpdate();
                            Check(label.preferredHeight<=label.rectTransform.rect.height+2,"Clipped "+label.name+" for "+offer.actionId+" at "+size+": "+label.text);
                        }
                    }
                    hud.HideAnnualPreview();Check(position==dock.anchoredPosition,"Leaving a field moved the dock");
                }
            }
        }
        Check(before==JsonUtility.ToJson(town.Snapshot()),"Inspection changed the annual ledger");
        var inputObject=new GameObject("Placement intent checks");
        var main=inputObject.AddComponent<TerraMain>();var session=inputObject.AddComponent<AnnualTownSession>();session.Initialize(definition,"placement-input");
        void Set(string name,object value)=>typeof(TerraMain).GetField(name,Private).SetValue(main,value);
        bool Consume()=>(bool)typeof(TerraMain).GetMethod("ConsumePlacementIntent",Private).Invoke(main,null);
        var inputOffer=definition.actions.First(a=>a.actionId=="public-park");
        Set("ready",true);Set("annual",session);Set("treatmentId",inputOffer.actionId);Set("hoverId",inputOffer.siteId);
        Invoke(main,"CapturePlacementIntent",inputOffer.siteId);Set("hoverId","another-field");
        Check(!Consume(),"A pointer press silently retargeted another field");
        Set("hoverId",inputOffer.siteId);Invoke(main,"CapturePlacementIntent",inputOffer.siteId);
        session.QueueAction(inputOffer.siteId,inputOffer.actionId,session.State.revision);
        Check(!Consume(),"An old pointer press accepted a changed plan revision");
        Invoke(main,"CapturePlacementIntent",inputOffer.siteId);Set("treatmentId","cultivate-standard");
        Check(!Consume(),"An old pointer press switched tools");
        Invoke(main,"CapturePlacementIntent",inputOffer.siteId);Check(Consume(),"An unchanged target was rejected");
        Invoke(main,"CapturePlacementIntent",inputOffer.siteId);Set("pressed",true);
        main.LoseFocus("");
        Check(!Field<bool>(main,"pressed")&&!Field<bool>(main,"placementIntentPending"),"Focus loss retained a pending placement press");
        UnityEngine.Object.DestroyImmediate(inputObject);
        Check(!Field<TextMeshProUGUI[]>(hud,"annualScores").Any(t=>t.text.Contains("Year-end")),"Persistent meters show prospective values as accepted");
        var shader=Resources.Load<Shader>("TerraProposalBoundary");Check(shader!=null&&!ShaderUtil.ShaderHasError(shader),"Proposal boundary shader unavailable or invalid");
        var owner=new GameObject("Placement geometry checks");var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
        var renderer=new LandscapeRenderer(world,material,owner.transform,null,null,null,null);renderer.ConfigureBuildActions(catalog,rules);
        foreach(var id in new[]{"public-park","cultivate-standard","water-swale"})
        {
            var offer=definition.actions.First(a=>a.actionId==id);var site=world.sites.First(s=>s.id==offer.siteId);
            var points=renderer.PlacementWorldPoints(site,id);var rule=rules.Find(id);
            var expected=rule.landscape?site.polygon:rules.Layout(site,rule).footprint;
            Check(points.Length==expected.Length&&points.Select((p,i)=>Math.Abs(p.x-expected[i].x)<.001f&&Math.Abs(p.z-expected[i].z)<.001f).All(x=>x),id+" preview does not match its actual spatial domain");
        }
        renderer.Dispose();UnityEngine.Object.DestroyImmediate(owner);UnityEngine.Object.DestroyImmediate(material);
        hud.HideAnnualPreview();hud.HidePlacementFeedback();
        if(hud.LargeText)Invoke(hud,"ToggleText");
        checks+=PlayFirstHudChecks.Run(hud,definition,world);
        Directory.CreateDirectory("docs/evidence/placement-clarity");
        File.WriteAllText("docs/evidence/placement-clarity/structural-checks.json",JsonUtility.ToJson(new Receipt{checks=checks,passed=true,scope="HUD state/layout, ledger immutability, actual placement geometry and shader; not browser or novice acceptance"},true));
        Debug.Log("PLACEMENT_CLARITY_CHECKS_PASSED: "+checks);
    }
    [Serializable] sealed class Receipt{public bool passed;public int checks;public string scope;}
}
