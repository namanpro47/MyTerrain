using System;
using System.IO;
using System.Linq;
using Terra.Art;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
using Terra.UI;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public static class MainGameChapterChecks
{
    static int count;
    static bool buildWeb;
    public static void BuildVerifiedWeb(){buildWeb=true;RunBatch();}
    static void Check(bool value,string message){if(!value)throw new Exception(message);count++;}
    public static void RunBatch()
    {
        try
        {
            count=0;CheckExactSaveRestore();CheckSessionAndHud();
            Directory.CreateDirectory("Logs/AnnualTown");
            int sessionChecks=AnnualChapterChecks.Run();
            File.WriteAllText("Logs/AnnualTown/main-game-chapter-checks.json","{\"assertions\":"+count+",\"existingChapterAssertions\":"+sessionChecks+",\"liveProviderTested\":false}");
            Debug.Log("MAIN_GAME_CHAPTER_CHECKS_PASSED: "+count);
            if(buildWeb)TerraBuild.BuildWeb();
            EditorApplication.Exit(0);
        }
        catch(Exception error){Debug.LogException(error);EditorApplication.Exit(1);}
    }
    static string ExactSaveHash(object value)=>(string)typeof(AnnualTown).Assembly.GetType("Terra.Core.TownCopies").GetMethod("Hash").Invoke(null,new[]{value});
    static void CheckExactSaveRestore()
    {
        const string key="terra.checks.feedback.precision";
        string Fixture(string name)
        {
            using(var stream=File.OpenRead("docs/evidence/annual-feedback/restore-fixture/"+name+".json.gz"))
            using(var gzip=new System.IO.Compression.GZipStream(stream,System.IO.Compression.CompressionMode.Decompress))
            using(var reader=new StreamReader(gzip))return reader.ReadToEnd();
        }
        var go=new GameObject("Exact annual restore checks");
        var session=go.AddComponent<AnnualTownSession>();
        try
        {
            var definition=AnnualNumericJson.ReadTownDefinition(Fixture("save-roundtrip-definition"));
            string original=Fixture("save-roundtrip-original");
            var saved=AnnualNumericJson.ReadTownSave(original);
            var restored=AnnualTown.Restore(definition,saved);
            Check(ExactSaveHash(restored.History)==ExactSaveHash(saved.history),"Fractional v5 report history changed after exact restore.");
            Check(BitConverter.DoubleToInt64Bits(saved.history[0].outcomes.sites[1].vegetatedAreaM2)==4668317464170332160L,"Fractional save number lost a bit during JSON parsing.");
            session.Initialize(definition,"precision");session.ImportSave(original);session.saveKey=key;session.Save();
            string expected=ExactSaveHash(session.State);
            session.Initialize(definition,"gzip-restore",true);
            Check(ExactSaveHash(session.State)==expected,"Native compressed v5 restore changed accepted state.");
            PlayerPrefs.SetString(key,original);session.Initialize(definition,"plain-restore",true);
            Check(ExactSaveHash(session.State)==expected,"Native old plain v5 restore changed accepted state.");
            foreach(bool money in new[]{true,false})
            {
                var tampered=AnnualNumericJson.ReadTownSave(original);
                if(money)tampered.history[0].ledger.closingCashCents++;else tampered.history[0].outcomes.sites[1].vegetatedAreaM2+=0.01;
                bool rejected=false;try{AnnualTown.Restore(definition,tampered);}catch(ArgumentException){rejected=true;}
                Check(rejected,"Exact restore accepted a changed "+(money?"ledger":"outcome")+".");
            }
        }
        finally {PlayerPrefs.DeleteKey(key);UnityEngine.Object.DestroyImmediate(go);}
    }
    static void CheckSessionAndHud()
    {
        var world=JsonUtility.FromJson<WorldData>(File.ReadAllText("worlds/meadowbrook-v1/world.json"));
        var scenario=JsonUtility.FromJson<Scenario>(File.ReadAllText("worlds/meadowbrook-v1/scenario.json"));
        var hydro=JsonUtility.FromJson<RunoffConfiguration>(File.ReadAllText("worlds/meadowbrook-v1/hydrology.json"));
        AnnualSpatialOpportunities.AddWind(world,hydro);
        var catalog=AssetDatabase.LoadAssetAtPath<BuildActionCatalog>("Assets/Terra/Art/ExternalKit/BuildActions/BuildActionCatalog.asset");
        var rules=BuildActionRuntime.Expand(catalog,ref world,ref scenario);
        var definition=FullAnnualGameplay.Create(world,scenario,"meadowbrook-v1",rules,new LandscapeRunoff(world,scenario,hydro));
        var go=new GameObject("Main game chapter checks");
        var town=go.AddComponent<AnnualTownSession>();town.Initialize(definition,"main-ui-fixture");
        var chapters=go.AddComponent<AnnualChapterSession>();chapters.town=town;chapters.autoSave=false;
        bool acceptedNotification=false;
        chapters.StateChanged+=()=>{if(town.State.phase==TownPhase.Reviewing)acceptedNotification=chapters.Latest?.consequences.year==town.State.year;};
        var events=new GameObject("Chapter check events");var eventSystem=events.AddComponent<EventSystem>();
        // Batch checks run in edit mode, where EventSystem's runtime OnEnable is not invoked.
        typeof(EventSystem).GetMethod("OnEnable",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic).Invoke(eventSystem,null);
        var hud=new TerraHud(_=>{},()=>{},()=>{},()=>{},()=>{},()=>{});
        hud.ConfigureEditing(()=>{},()=>{},()=>{},()=>{});
        hud.ConfigureBuildActions(catalog,rules,world,_=>{});hud.ConfigureAnnual(definition,()=>{},()=>{},()=>{});
        try
        {
            count+=PlayFirstHudChecks.Run(hud,definition,world);
            count+=FieldNotesChecks.Run(hud,definition);
            count+=ActionFeedbackChecks.Run(hud);
            count+=AnnualFeedbackWaveChecks.Run(hud,definition,world);
            var playerDefinition=AnnualPlayerRules.Create(definition,"meadowbrook-v1","native-feedback");
            var rulesField=typeof(TerraHud).GetField("annualRules",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic);
            rulesField.SetValue(hud,playerDefinition);
            count+=AnnualFeedbackWaveChecks.Run(hud,playerDefinition,world);
            rulesField.SetValue(hud,definition);
            var offer=definition.actions.First(a=>a.actionId=="public-park");
            town.QueueAction(offer.siteId,offer.actionId,town.State.revision);
            long opening=town.State.cashCents;
            hud.AnnualConfirmation(town.Engine,AnnualGameplay.Forecast(town.Engine),()=>chapters.ResolveOffline(),true);
            Check(town.State.cashCents==opening&&town.State.history.Length==0,"Budget review spent money.");
            Click("End Year");
            Check(acceptedNotification,"Accepted notification arrived before its chapter was available.");
            Check(town.State.history.Length==1&&town.State.phase==TownPhase.Reviewing,"Year was not accepted exactly once.");
            var receipt=chapters.ForYear(1);
            receipt.chapter.title="Edited detached copy";
            Check(chapters.ForYear(1).chapter.title!="Edited detached copy","Historical chapter leaked mutable state.");
            string save=chapters.ExportSave();
            chapters.ImportSave(save);
            Check(chapters.ExportSave()==save,"Main catalog chapter/history did not restore exactly.");
            var facts=AnnualChapters.Facts(town.Engine);
            Check(facts.First(f=>f.id=="year").text.Contains("Year 1 of 10"),"Narrator did not receive the actual year.");
            Check(facts.First(f=>f.id=="budget").text.Contains("$")&&!facts.First(f=>f.id=="budget").text.Contains("USD cents"),"Narrator money was not formatted as dollars.");
            Action next=()=>{hud.CloseModal();chapters.Continue();};
            Action ledger=()=>hud.AnnualReport(town.Engine,AnnualGameplay.Baseline(definition),next,()=>hud.AnnualChapterReport(town.Engine,chapters.ForYear(1),next,()=>{}));
            hud.AnnualChapterReport(town.Engine,chapters.ForYear(1),next,ledger);
            Check(hud.Audit().Contains("local summary"),"Main report did not label the local summary.");
            Click("Budget & meters");Check(hud.Audit().Contains("Back to chapter"),"Ledger lost its chapter return path.");
            Click("Back to chapter");Click("Read chapter →");
            Click("Next card →");Click("Next card →");
            Check(town.State.year==1,"Reading story cards advanced time.");
            Click("Plan year 2 →");
            Check(town.State.year==2&&town.State.phase==TownPhase.Planning,"Last story card did not reopen planning.");
            town.BeginYear("ui-pending-year-2",town.State.revision);
            Directory.CreateDirectory("Logs/AnnualTown");
            File.WriteAllText("Logs/AnnualTown/main-game-chapter-request.json",JsonUtility.ToJson(new AnnualChapterRequest {definition=definition,snapshot=town.State}));
            var pending=chapters.ExportSave();
            chapters.ImportSave(pending);
            hud.AnnualChapterRecovery(2,"Connection interrupted",()=>{},()=>chapters.ResolveOffline(),()=>{hud.CloseModal();chapters.CancelResolution();});
            Check(hud.Audit().Contains("Use local summary")&&hud.Audit().Contains("Edit this year's plan"),"Recovery actions are missing.");
            Click("Edit this year's plan");
            Check(!hud.ModalOpen&&town.State.phase==TownPhase.Planning&&town.State.history.Length==1,"Editing a pending plan lost accepted history or left recovery open.");
            town.Initialize(definition,"new-main-ui-run");chapters.ResetForRun();
            Check(chapters.ForYear(1)==null&&chapters.Latest==null,"A new run retained a previous chapter.");
            chapters.FinishOffline();
            Check(town.State.phase==TownPhase.Completed&&chapters.ForYear(10)!=null,"Fast-forward lost its final offline chapter.");
            var completed=chapters.ExportSave();chapters.ImportSave(completed);
            Check(chapters.ExportSave()==completed,"Full completed chapter envelope did not restore.");
            string priorTownKey=town.saveKey,priorChapterKey=chapters.saveKey;
            town.saveKey="terra.checks.feedback.storage.town";chapters.saveKey="terra.checks.feedback.storage.chapter";
            try
            {
                town.Save();chapters.Save();
                Check(PlayerPrefs.GetString(town.saveKey).StartsWith("terra-gzip-v1:"),"Annual browser save is not compressed.");
                Check(PlayerPrefs.GetString(chapters.saveKey).Length<completed.Length/2,"Chapter save did not reduce browser quota pressure.");
                string exact=town.ExportSave();
                town.Initialize(definition,"restore-compressed",true);chapters.RestoreSaved();
                Check(town.ExportSave()==exact&&chapters.ExportSave()==completed,"Compressed native save lost accepted history or chapters.");
                PlayerPrefs.SetString(town.saveKey,exact);
                town.Initialize(definition,"restore-legacy",true);
                Check(town.ExportSave()==exact,"Legacy plain JSON save is no longer readable.");
            }
            finally{PlayerPrefs.DeleteKey(town.saveKey);PlayerPrefs.DeleteKey(chapters.saveKey);town.saveKey=priorTownKey;chapters.saveKey=priorChapterKey;}

            bool futureOpened=false;
            hud.AnnualChapterReport(town.Engine,chapters.ForYear(10),()=>futureOpened=true,()=>{});
            Click("Read chapter →");Click("Next card →");Click("Next card →");Click("See ten-year future");
            Check(futureOpened&&town.State.year==10,"Final chapter did not hand off to the future without a Year 11.");
            hud.CloseModal();
            var baseline=AnnualGameplay.Baseline(definition);
            hud.AnnualState(town.Engine,baseline,AnnualGameplay.Forecast(town.Engine).outcomes);
            foreach(bool compact in new[]{false,true})
            {
                typeof(TerraHud).GetMethod("LayoutAnnual",System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic)
                    .Invoke(hud,new object[]{compact?1024f:1440f,compact?576f:900f,compact});
                var audit=JsonUtility.FromJson<TerraHud.UiAudit>(hud.Audit());
                Check(!audit.controls.Any(c=>c.name.StartsWith("Build ")||c.name=="Button Undo  Z"||c.name=="Button Redo  Y"||c.name=="Button Clear all  X"),"Completed map exposes construction controls.");
                Check(audit.controls.Any(c=>c.name=="Button Valley view  R")&&audit.controls.Any(c=>c.name=="Button Field guide  G")&&audit.controls.Any(c=>c.name=="Button Controls  ?"),"Completed map hid an exploration control.");
            }
            town.Initialize(definition,"replayed-ui-run");hud.AnnualState(town.Engine,baseline,AnnualGameplay.Forecast(town.Engine).outcomes);
            var replayAudit=JsonUtility.FromJson<TerraHud.UiAudit>(hud.Audit());
            Check(replayAudit.controls.Any(c=>c.name.StartsWith("Build ")&&c.enabled),"Replay did not restore the build catalog.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(go);
            UnityEngine.Object.DestroyImmediate(events);
            var canvas=GameObject.Find("Terra HUD");if(canvas!=null)UnityEngine.Object.DestroyImmediate(canvas);
        }
    }
    static void Click(string caption)
    {
        var dialog=GameObject.Find("Field guide");
        var button=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None)
            .Single(b=>b.gameObject.activeInHierarchy&&(dialog==null||b.transform.IsChildOf(dialog.transform))&&b.GetComponentInChildren<TextMeshProUGUI>()?.text==caption);
        button.onClick.Invoke();
    }
}
