using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Terra.Art;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
using Terra.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public static class ProjectStatsChecks
{
    const BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    const string Output="docs/evidence/building-visual-stats";
    static int count;
    static T Field<T>(object o,string name)=>(T)o.GetType().GetField(name,Private).GetValue(o);
    static void Check(bool value,string reason){count++;if(!value)throw new Exception(reason);}
    public static void RunBatch()
    {
        try{Run();Debug.Log("PROJECT_STATS_CHECKS_PASSED: "+count);File.WriteAllText(Output+"/native-checks.json","{\"assertions\":"+count+",\"rendering\":\"native Unity uGUI offscreen HUD\"}");EditorApplication.Exit(0);}
        catch(Exception e){Debug.LogException(e);EditorApplication.Exit(1);}
    }
    public static void BuildWeb(){TerraBuild.BuildWeb();EditorApplication.Exit(0);}
    static void Run()
    {
        Directory.CreateDirectory(Output);
        var world=JsonUtility.FromJson<WorldData>(File.ReadAllText("worlds/meadowbrook-v1/world.json"));
        var scenario=JsonUtility.FromJson<Scenario>(File.ReadAllText("worlds/meadowbrook-v1/scenario.json"));
        var hydro=JsonUtility.FromJson<RunoffConfiguration>(File.ReadAllText("worlds/meadowbrook-v1/hydrology.json"));
        AnnualSpatialOpportunities.AddWind(world,hydro);
        var catalog=AssetDatabase.LoadAssetAtPath<BuildActionCatalog>("Assets/Terra/Art/ExternalKit/BuildActions/BuildActionCatalog.asset");
        var rules=BuildActionRuntime.Expand(catalog,ref world,ref scenario);
        var d=AnnualPlayerRules.Create(FullAnnualGameplay.Create(world,scenario,"meadowbrook-v1",rules,new LandscapeRunoff(world,scenario,hydro)),"meadowbrook-v1","ui-checks");
        var events=new GameObject("Project stats event system");var es=events.AddComponent<EventSystem>();
        typeof(EventSystem).GetMethod("OnEnable",Private).Invoke(es,null);
        var hud=new TerraHud(_=>{},()=>{},()=>{},()=>{},()=>{},()=>{});
        hud.ConfigureEditing(()=>{},()=>{},()=>{},()=>{});hud.ConfigureBuildActions(catalog,rules,world,_=>{});hud.ConfigureAnnual(d,()=>{},()=>{},()=>{});hud.Ready();
        var root=Field<Transform>(hud,"root");var canvas=root.GetComponent<Canvas>();
        var camera=new GameObject("Project stats capture camera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.16f,.22f,.20f);camera.orthographic=true;camera.nearClipPlane=.1f;camera.farClipPlane=100;camera.enabled=false;
        canvas.renderMode=RenderMode.WorldSpace;canvas.worldCamera=camera;root.position=Vector3.zero;camera.transform.position=new Vector3(0,0,-10);canvas.GetComponent<CanvasScaler>().enabled=false;
        var town=new AnnualTown(d,"ui-checks");var baseline=AnnualGameplay.Baseline(d);hud.AnnualState(town,baseline,baseline);
        string Text()=>string.Join("\n",root.GetComponentsInChildren<TextMeshProUGUI>().Where(t=>t.gameObject.activeInHierarchy).Select(t=>t.text));
        void Inspect(TownActionDefinition offer,bool selected=true)
        {
            hud.HidePlacementFeedback();hud.AnnualState(town,baseline,town.History.LastOrDefault()?.outcomes??baseline);
            hud.Selected(selected?offer.actionId:"");
            hud.AnnualPreview(world.sites.First(s=>s.id==offer.siteId),selected?offer:null,town.Snapshot(),"place",selected?null:"Accepted project",()=>{},false,()=>{});
        }
        void Layout(int width,int height)
        {
            if(camera.targetTexture!=null){var old=camera.targetTexture;camera.targetTexture=null;old.Release();UnityEngine.Object.DestroyImmediate(old);}
            var rt=new RenderTexture(width,height,24);rt.Create();camera.targetTexture=rt;
            ((RectTransform)root).sizeDelta=new Vector2(width,height);root.localScale=Vector3.one;camera.orthographicSize=height/2f;camera.aspect=(float)width/height;Canvas.ForceUpdateCanvases();
            typeof(TerraHud).GetMethod("LayoutAnnual",Private).Invoke(hud,new object[]{(float)width,(float)height,width<1120||height<680});
            Canvas.ForceUpdateCanvases();
        }
        void Validate()
        {
            var facts=Field<Transform>(hud,"projectFacts");
            Check(facts.gameObject.activeInHierarchy,"Visual facts hidden");
            foreach(var t in facts.GetComponentsInChildren<TextMeshProUGUI>().Concat(new[]{Field<TextMeshProUGUI>(hud,"previewTitle"),Field<TextMeshProUGUI>(hud,"previewKind")}))
            {
                if(!t.gameObject.activeInHierarchy)continue;t.ForceMeshUpdate();
                Check(t.preferredHeight<=t.rectTransform.rect.height+2,"Height overflow: "+t.text);
                Check(t.textBounds.size.x<=t.rectTransform.rect.width+3,"Width overflow: "+t.text+" ("+t.textBounds.size.x+"/"+t.rectTransform.rect.width+", font "+t.fontSize+")");
            }
            foreach(var row in facts.GetComponentsInChildren<RectTransform>().Where(t=>t.name.StartsWith("Project stat ")))
            {
                var value=row.GetComponentsInChildren<TextMeshProUGUI>().Last();
                Check(value.alignment==TextAlignmentOptions.Right,"Stat value not right aligned");
                var track=(RectTransform)row.Find("Amount track");
                var a=new Vector3[4];var b=new Vector3[4];track.GetWorldCorners(a);value.rectTransform.GetWorldCorners(b);
                Check(a[2].x<=b[0].x+1,"Value overlaps bar");
            }
            foreach(var t in Field<Transform>(hud,"townNeeds").GetComponentsInChildren<TextMeshProUGUI>())
            {t.ForceMeshUpdate();Check(t.textBounds.size.x<=t.rectTransform.rect.width+3&&t.textBounds.size.y<=t.rectTransform.rect.height+2,"Town need overflow: "+t.text);}
            var panel=Field<GameObject>(hud,"tooltip").GetComponent<RectTransform>();
            var corners=new Vector3[4];panel.GetWorldCorners(corners);
            var command=(RectTransform)Field<Transform>(hud,"commandBar");var commandCorners=new Vector3[4];command.GetWorldCorners(commandCorners);
            Check(corners[0].y>commandCorners[1].y,"Inspector overlaps bottom command bar: "+corners[0].y+" <= "+commandCorners[1].y+" canvas="+((RectTransform)root).rect.size);
        }
        void Capture(string name)
        {
            Canvas.ForceUpdateCanvases();
            var target=camera.targetTexture;
            if(GraphicsSettings.currentRenderPipeline!=null)RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});else camera.Render();
            var prior=RenderTexture.active;RenderTexture.active=target;var texture=new Texture2D(target.width,target.height,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,target.width,target.height),0,0);texture.Apply();RenderTexture.active=prior;
            File.WriteAllBytes(Output+"/"+name+".png",texture.EncodeToPNG());UnityEngine.Object.DestroyImmediate(texture);
            File.WriteAllText(Output+"/"+name+".txt",Text());
        }
        foreach(bool large in new[]{false,true})
        {
            if(large)typeof(TerraHud).GetMethod("ToggleText",Private).Invoke(hud,null);
            foreach(var size in new[]{new Vector2Int(1440,900),new Vector2Int(1280,720),new Vector2Int(1024,576)})
            foreach(var offer in d.actions.Where(a=>a.actionId!=AnnualPlayerRules.CancelAction).GroupBy(a=>a.actionId).Select(g=>g.First()))
            {
                Inspect(offer);Layout(size.x,size.y);Validate();
                Check(Text().Contains("SCORE IMPACT · Y"),"No score comparison for "+offer.actionId);
                Check(!Text().Contains(" output")&&!Text().Contains("Established"),"Unexplained simulation label in main overlay");
                if(offer.actionId=="data-center"||offer.actionId=="public-park")Capture(offer.actionId+"-"+size.x+(large?"-large":""));
            }
        }
        var data=d.actions.First(a=>a.actionId=="data-center");hud.Selected(data.actionId);
        hud.AnnualPreview(null,null,town.Snapshot(),"place","Choose a field",()=>{},false);Layout(1024,576);Validate();
        Check(Text().Contains("CHOOSE A FIELD")&&!Field<Button>(hud,"confirmEditButton").gameObject.activeSelf,"Catalog preview missing or offers a queue action");Capture("catalog-no-field");
        town.QueueAction(data.siteId,data.actionId,town.Revision);Inspect(data);Layout(1024,576);
        hud.PlacementFeedback("BLOCKED",data.title,data.siteId,"Already queued","Queued footprint","Already queued",null,null,Array.Empty<Vector3>(),null);
        Validate();Check(Text().Contains("QUEUED"),"Queued view lost its bars");
        var original=JsonUtility.ToJson(town.Snapshot());var replacement=d.actions.First(a=>a.siteId==data.siteId&&a.actionId=="public-park");Inspect(replacement);
        hud.AnnualPreview(world.sites.First(s=>s.id==data.siteId),replacement,town.Snapshot(),"place","Replacement blocked",()=>{},true);
        hud.PlacementFeedback("BLOCKED",replacement.title,data.siteId,"Choose another field","","Replacement blocked",null,null,Array.Empty<Vector3>(),null);
        Check(!Field<Transform>(hud,"projectFacts").gameObject.activeSelf&&Text().Contains("Replacement blocked"),"Blocked replacement exposed old-project bars");
        Check(original==JsonUtility.ToJson(town.Snapshot()),"Inspection changed saved state");
        AnnualTownDemo.Advance(town,"ui-y1",new AuthoredAnnualConsequenceProvider());Inspect(data,false);Layout(1024,576);Validate();Check(Text().Contains("1/2 yr"),"Construction does not show its first step");Capture("accepted-year1");
        town.ContinuePlanning(town.Revision);AnnualTownDemo.Advance(town,"ui-y2",new AuthoredAnnualConsequenceProvider());town.ContinuePlanning(town.Revision);
        Inspect(data,false);Layout(1280,720);Validate();Check(Text().Contains("2/2 yr")&&Text().Contains("0%")&&Text().Contains("Opens Y3"),"Built and establishment conflated");Capture("accepted-year3-planning");
        Check(Field<Button>(hud,"confirmEditButton").GetComponentInChildren<TextMeshProUGUI>().text=="Cancel project…","Cancellation control lost");
        AnnualTownDemo.Advance(town,"ui-y3",new AuthoredAnnualConsequenceProvider());Inspect(data,false);Layout(1440,900);Validate();Capture("accepted-operating");
        typeof(TerraHud).GetMethod("AnnualProjectDetails",Private).Invoke(hud,new object[]{data,false});
        Check(Field<bool>(hud,"fieldNotesOpen"),"Long assumptions lack a scrollable reading area");
        Check(Field<TextMeshProUGUI>(hud,"fieldNotesBody").text.Contains("not revenue"),"Missing plain-language meaning");
        hud.CloseModal();
        AnnualTownDemo.Finish(town,new AuthoredAnnualConsequenceProvider());Inspect(data,false);Layout(1024,576);Validate();Check(!Field<Button>(hud,"confirmEditButton").gameObject.activeSelf,"Year 10 exposes cancellation");
        camera.targetTexture.Release();UnityEngine.Object.DestroyImmediate(camera.targetTexture);UnityEngine.Object.DestroyImmediate(root.gameObject);UnityEngine.Object.DestroyImmediate(events);UnityEngine.Object.DestroyImmediate(camera.gameObject);
    }
}
