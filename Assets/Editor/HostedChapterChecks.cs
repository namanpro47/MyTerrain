using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Terra.Contracts;
using Terra.Core;
using Terra.Gameplay;
using UnityEditor;
using UnityEngine;

public static class HostedChapterChecks
{
    static int count;
    static bool buildWeb;
    public static void BuildVerifiedWeb() { buildWeb = true; RunBatch(); }
    static void Check(bool value, string message) { if (!value) throw new Exception(message); count++; }
    public static void RunBatch()
    {
        try
        {
            count = 0;
            Check(HostedChapterBuild.EndpointForBuild("http://127.0.0.1:8787/chapter", BuildTarget.WebGL) == HostedChapterBuild.ProductionEndpoint, "WebGL loopback was not replaced.");
            Check(HostedChapterBuild.EndpointForBuild("http://127.0.0.1:8787/chapter", BuildTarget.StandaloneWindows64) == "http://127.0.0.1:8787/chapter", "Native developer endpoint changed.");
            Check(HostedChapterBuild.EndpointForBuild("https://custom.example/chapter", BuildTarget.WebGL) == "https://custom.example/chapter", "Explicit hosted override changed.");
            var world = JsonUtility.FromJson<WorldData>(File.ReadAllText("worlds/meadowbrook-v1/world.json"));
            var scenario = JsonUtility.FromJson<Scenario>(File.ReadAllText("worlds/meadowbrook-v1/scenario.json"));
            var definition = AnnualTownDemo.FromWorldPack(world, scenario, "meadowbrook-v1");
            var go = new GameObject("Hosted chapter fallback checks");
            try
            {
                var town = go.AddComponent<AnnualTownSession>();
                town.Initialize(definition, "hosted-fallback-checks");
                var offer = definition.actions.First(a => town.Preview(a.siteId, a.actionId) >= 0);
                town.QueueAction(offer.siteId, offer.actionId, town.State.revision);
                town.BeginYear("hosted-fallback-year-1", town.State.revision);
                var input = new AnnualChapterRequest { definition = town.Definition, snapshot = town.State };
                var chapters = go.AddComponent<AnnualChapterSession>(); chapters.town = town; chapters.autoSave = false;
                var flags = BindingFlags.Instance | BindingFlags.NonPublic;
                var frozen = typeof(AnnualChapterSession).GetField("frozen", flags);
                var fail = typeof(AnnualChapterSession).GetMethod("Fail", flags);
                var expected = AnnualChapters.Preview(input).Snapshot();
                frozen.SetValue(chapters, input);
                fail.Invoke(chapters, new object[] { "Synthetic provider quota reached" });
                Check(town.State.phase == TownPhase.Reviewing && town.State.history.Length == 1, "Fallback must accept exactly one year.");
                Check(town.State.cashCents == expected.cashCents, "Fallback changed the reconciled cash.");
                Check(JsonUtility.ToJson(town.State.history[0]) == JsonUtility.ToJson(expected.history[0]), "Fallback changed engine outcomes.");
                Check(chapters.Latest.mode == "offline" && chapters.Latest.chapter.introduction.StartsWith("AI unavailable."), "Fallback disclosure missing.");
                Check(!chapters.Busy && chapters.Error == null, "Fallback left a busy/error state.");
                var saved = chapters.ExportSave();
                chapters.ResolveOffline();
                Check(chapters.ExportSave() == saved, "Repeated resolution changed the accepted year.");
                chapters.ImportSave(saved);
                Check(chapters.ExportSave() == saved, "Fallback receipt did not survive save/reload exactly.");
                chapters.ImportSave(JsonUtility.ToJson(new AnnualChapterSave { town = input.snapshot, chapters = Array.Empty<AnnualChapterReceipt>() }));
                chapters.fallbackWhenUnavailable = false; frozen.SetValue(chapters, input);
                fail.Invoke(chapters, new object[] { "Synthetic transport failure" });
                Check(town.State.phase == TownPhase.Resolving && town.State.history.Length == 0, "Disabled fallback advanced the year.");
                Check(!string.IsNullOrEmpty(chapters.Error), "Disabled fallback lost recovery information.");
                chapters.fallbackWhenUnavailable = true;
                frozen.SetValue(chapters, input); fail.Invoke(chapters, new object[] { "Synthetic timeout" });
                Check(town.State.history.Length == 1 && town.State.cashCents == expected.cashCents, "Retry rerolled the pending year.");
            }
            finally { UnityEngine.Object.DestroyImmediate(go); }
            int existing = AnnualChapterChecks.Run();
            Directory.CreateDirectory("Logs/AnnualTown");
            File.WriteAllText("Logs/AnnualTown/hosted-chapter-checks.json", "{\"hostedAssertions\":" + count + ",\"existingAssertions\":" + existing + ",\"browserTested\":false}");
            Debug.Log("HOSTED_CHAPTER_CHECKS_PASSED: " + count + "; existing " + existing);
            if (buildWeb) TerraBuild.BuildWeb();
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }
}
