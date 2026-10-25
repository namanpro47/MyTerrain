using System;
using System.IO;
using System.Linq;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
using UnityEditor;
using UnityEngine;

public static class AnnualChapterChecks
{
    static int count;
    static void Check(bool value, string message) { if (!value) throw new Exception(message); count++; }
    [MenuItem("Terra/Annual Town/Run Chapter Checks")]
    public static void RunMenu() { Debug.Log("Annual chapter checks: " + Run()); }
    public static void RunBatch()
    {
        try
        {
            int result = Run();
            Directory.CreateDirectory("Logs/AnnualTown");
            File.WriteAllText("Logs/AnnualTown/chapter-checks.json", "{\"unityChapterAssertions\":" + result + ",\"liveProviderTested\":false}");
            Debug.Log("Annual chapter checks passed: " + result);
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }
    public static int Run()
    {
        count = 0;
        var root = Path.GetFullPath(Path.Combine(Application.dataPath, "../worlds/meadowbrook-v1"));
        var world = JsonUtility.FromJson<WorldData>(File.ReadAllText(Path.Combine(root, "world.json")));
        var scenario = JsonUtility.FromJson<Scenario>(File.ReadAllText(Path.Combine(root, "scenario.json")));
        var definition = AnnualTownDemo.FromWorldPack(world, scenario, "meadowbrook-v1");
        var go = new GameObject("Chapter checks") { hideFlags = HideFlags.HideAndDontSave };
        string key = "terra.chapter.checks." + Guid.NewGuid().ToString("N");
        try
        {
            var town = go.AddComponent<AnnualTownSession>();
            town.Initialize(definition, "unity-chapter-fixture");
            var chapters = go.AddComponent<AnnualChapterSession>();
            chapters.town = town; chapters.saveKey = key;
            var offer = definition.actions.First(a => town.Preview(a.siteId, a.actionId) >= 0);
            town.QueueAction(offer.siteId, offer.actionId, town.State.revision);
            long cash = town.State.cashCents;
            town.BeginYear("frozen-fixture", town.State.revision);
            chapters.Save();
            string pending = chapters.ExportSave();
            Directory.CreateDirectory("Logs/AnnualTown");
            File.WriteAllText("Logs/AnnualTown/unity-chapter-request.json", JsonUtility.ToJson(new AnnualChapterRequest { definition = definition, snapshot = town.State }, true));
            chapters.ResolveOffline();
            Check(town.State.phase == TownPhase.Reviewing && town.State.history.Length == 1, "Offline chapter did not commit exactly one report.");
            Check(chapters.Latest.mode == "offline" && chapters.Latest.chapter.cards.Length == 3, "Offline disclosure/cards missing.");
            string accepted = chapters.ExportSave();
            chapters.ResolveOffline();
            Check(chapters.ExportSave() == accepted, "Repeated conclude changed accepted year.");
            var detached = chapters.Latest; detached.chapter.title = "Modified outside";
            Check(chapters.Latest.chapter.title != detached.chapter.title, "Latest receipt leaked mutable state.");
            chapters.Continue();
            Check(town.State.year == 2 && town.State.phase == TownPhase.Planning, "Continue did not open next chapter.");
            chapters.ImportSave(accepted);
            Check(chapters.ExportSave() == accepted, "Accepted chapter did not restore exactly through JsonUtility.");
            chapters.ImportSave(pending);
            Check(town.State.phase == TownPhase.Resolving && town.State.cashCents == cash && town.State.pendingRequest.requestId == "frozen-fixture", "Pending request/cash changed on restore.");
            chapters.ResolveOffline();
            Check(chapters.ExportSave() == accepted, "Restored pending year changed outcome.");
            chapters.FinishOffline();
            var final = town.State;
            Check(final.phase == TownPhase.Completed && final.history.Length == 10 && final.year == 10, "Finish skipped or added a year.");
            Check(chapters.Latest.consequences.year == 10 && chapters.Latest.chapter.title.Contains("ten-year"), "Final chapter missing.");
            string completed = chapters.ExportSave();
            chapters.FinishOffline();
            Check(chapters.ExportSave() == completed, "Repeated finish rerolled history.");
            chapters.ImportSave(completed);
            Check(chapters.ExportSave() == completed, "Ten-year chapter history did not restore exactly.");
            var tampered = JsonUtility.FromJson<AnnualChapterSave>(completed);
            tampered.chapters[0].consequences.inputHash = "tampered";
            bool rejected = false;
            try { chapters.ImportSave(JsonUtility.ToJson(tampered)); } catch (ArgumentException) { rejected = true; }
            Check(rejected && chapters.ExportSave() == completed, "Invalid save partially changed live history.");
            chapters.RestoreSaved();
            Check(chapters.ExportSave() == completed, "PlayerPrefs autosave restore failed.");
            chapters.ImportSave(pending);
            chapters.endpoint = "http://example.com/chapter";
            chapters.ResolveOnline();
            Check(!chapters.Busy && !string.IsNullOrEmpty(chapters.Error) && town.State.cashCents == cash, "Unsafe endpoint did not fail without spending.");
            chapters.CancelResolution();
            chapters.RestoreSaved();
            Check(town.State.phase == TownPhase.Planning && town.State.pendingRequest == null, "Cancelled resolution was not autosaved.");
            return count;
        }
        finally { PlayerPrefs.DeleteKey(key); UnityEngine.Object.DestroyImmediate(go); }
    }
}
