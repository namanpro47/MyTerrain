using System;
using System.IO;
using System.Linq;
using Terra.Art;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class BuildActionIntegrationChecks
{
    static int count;
    static void Check(bool valid,string reason){if(!valid)throw new Exception(reason);count++;}
    static T Read<T>(string name)=>JsonUtility.FromJson<T>(File.ReadAllText("worlds/meadowbrook-v1/"+name+".json"));
    [MenuItem("Terra/Build Actions/Prepare and check")]
    public static void PrepareAndCheck()
    {
        count=0;Directory.CreateDirectory("docs/evidence/build-action-ui");
        var catalog=AssetDatabase.LoadAssetAtPath<BuildActionCatalog>("Assets/Terra/Art/ExternalKit/BuildActions/BuildActionCatalog.asset");
        var world=Read<WorldData>("world");var scenario=Read<Scenario>("scenario");
        var originalWorld=world;var originalScenario=scenario;string beforeWorld=JsonUtility.ToJson(world),beforeScenario=JsonUtility.ToJson(scenario);
        var rules=BuildActionRuntime.Expand(catalog,ref world,ref scenario);
        Check(rules.actions.Length==17&&scenario.treatments.Length==17,"17 actions in art, selector and runtime");
        Check(rules.actions.Select(a=>a.id).Distinct().Count()==17,"unique action IDs");
        Check(rules.actions.Select(a=>a.category).Distinct().Count()==5,"five categories");
        Check(JsonUtility.ToJson(originalWorld)==beforeWorld&&JsonUtility.ToJson(originalScenario)==beforeScenario,"canonical pack remains immutable");
        PackValidator.Validate(Read<Manifest>("manifest"),world,scenario);Check(true,"augmented runtime pack validates");
        var available=rules.actions.Select(action=>new Availability{id=action.id,category=action.category,sites=world.sites.Where(site=>rules.Rejection(site,action.id)==null).Select(site=>site.name).ToArray()}).ToArray();
        Check(available.Single(a=>a.id=="water-swale").sites.OrderBy(s=>s).SequenceEqual(new[]{"Brye-side field","Meadow edge"}),"swales stay on exactly two authored sites");
        Check(available.Single(a=>a.id=="wind").sites.Length==0,"utility wind remains full scale and cannot fit current parcels");
        foreach(var row in available.Where(a=>a.id!="wind"))Check(row.sites.Length>0,"at least one usable parcel: "+row.id);
        foreach(var site in world.sites)
        {
            Check(!site.protectedSite||site.offers.Length==0,"protected zone unchanged");
            foreach(var action in rules.actions)
            {
                var state=new WorldStateStore(world,scenario,new LandscapeRunoff(world,scenario,Read<RunoffConfiguration>("hydrology")),rules.Rejection);
                var command=new EditCommand{operation="add",siteId=site.id,treatmentId=action.id,expectedRevision=0};
                string rejection=rules.Rejection(site,action.id);var evaluation=state.Preview(command);
                Check(evaluation.valid==(rejection==null),"availability agrees with authoritative preview: "+site.id+" / "+action.id);
                if(!evaluation.valid){Check(!state.Commit(command).valid&&state.Revision==0,"blocked action changes no state");continue;}
                var layout=rules.Layout(site,action);
                Check(layout.footprint.All(p=>BuildActionRules.Contains(site.polygon,p.x,p.z)),"full footprint inside parcel");
                Check(action.landscape||layout.relief<=BuildActionRules.MaximumFoundationRelief,"bounded foundation");
                int cost=site.offers.Single(o=>o.treatmentId==action.id).cost;
                Check(state.Commit(command).valid&&state.Result.spentCredits==cost,"commit charges matching site offer");
                var runoff=state.Result;
                Check(runoff.runoffAvailable&&Math.Abs(runoff.generatedM3-runoff.retainedM3-runoff.dischargedM3)<.01f,"catalog action preserves runoff conservation");
                Check(state.Undo()&&state.Result.spentCredits==0&&state.TreatmentAt(site.id)==null,"undo restores budget and baseline");
            }
        }
        var central=world.sites.Single(s=>s.id=="south-02");var budgetState=new WorldStateStore(world,scenario,rules.Rejection);
        Check(budgetState.Commit(new EditCommand{operation="add",siteId=central.id,treatmentId="data-center",expectedRevision=0}).valid,"place data center");
        Check(!budgetState.Preview(new EditCommand{operation="add",siteId="south-03",treatmentId="data-center",expectedRevision=1}).valid,"overspend rejected");
        Check(budgetState.Commit(new EditCommand{operation="replace",siteId=central.id,treatmentId="low-density-housing",expectedRevision=1}).valid&&budgetState.Result.spentCredits==30,"replacement refunds previous cost");
        Check(budgetState.Undo()&&budgetState.TreatmentAt(central.id)=="data-center"&&budgetState.Result.spentCredits==36,"replacement undo restores previous model ID and cost");
        Check(!budgetState.Commit(new EditCommand{operation="replace",siteId=central.id,treatmentId="public-park",expectedRevision=1}).valid,"stale preview rejected");
        var scene=EditorSceneManager.OpenScene("Assets/Terra/Scenes/TerraMain.unity");
        var main=UnityEngine.Object.FindFirstObjectByType<TerraMain>();main.buildActionCatalog=catalog;EditorUtility.SetDirty(main);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Phase1Setup.PackageFixture();
        File.WriteAllText("docs/evidence/build-action-ui/rules.json",JsonUtility.ToJson(new Receipt{checksPassed=count,availability=available,maximumFoundationReliefMetres=3},true));
        File.WriteAllText("docs/evidence/build-action-ui/legacy-phase1.json",Phase1Checks.Run());
        File.WriteAllText("docs/evidence/build-action-ui/legacy-phase2.json",Phase2Checks.Run());
        Debug.Log("BUILD_ACTION_UI_CHECKS_PASS "+count);
    }
    public static void CheckAndBuild(){PrepareAndCheck();TerraBuild.BuildWeb();}
    [Serializable] sealed class Receipt{public int checksPassed;public float maximumFoundationReliefMetres;public Availability[] availability;}
    [Serializable] sealed class Availability{public string id,category;public string[] sites;}
}
