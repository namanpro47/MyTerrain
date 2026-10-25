using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terra.Contracts;
using Terra.Simulation;
using UnityEditor;
using UnityEngine;

// Isolated import/contract verification. Does not open or modify any scene.
public static class ParallelPackageChecks
{
    [Serializable] sealed class ContentIndex { public string[] entries, legacyEntries, questionIds; }
    [Serializable] sealed class AudioManifest { public AudioItem[] clips; }
    [Serializable] sealed class AudioItem { public string id, file; public int frames; }
    [Serializable] sealed class AudioReadback
    {
        public string id, guid, loadType, compressionFormat;
        public int samples, channels, frequency;
        public float durationSeconds;
        public bool forceToMono;
    }
    [Serializable] sealed class Report
    {
        public bool passed;
        public string engine, scope;
        public string[] checks;
        public int contentEntries, phase2Checks;
        public AudioReadback[] audio;
    }
    static readonly List<string> Passed = new List<string>();
    static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception(message);
        Passed.Add(message);
    }
    static T Read<T>(string path) => JsonUtility.FromJson<T>(File.ReadAllText(path));
    static bool Near(float a, float b) => Math.Abs(a - b) <= .001f;

    // One-time setup for new kit assets, through Unity's importer API (never hand-edited metadata).
    public static void ConfigureAudioImports()
    {
        const string kit = "Assets/Terra/Audio/ExternalKit/";
        foreach (var item in Read<AudioManifest>(kit + "manifest.json").clips)
        {
            var importer = (AudioImporter)AssetImporter.GetAtPath(kit + item.file);
            bool ambient = item.id == "ambient-wind";
            importer.forceToMono = false;
            importer.loadInBackground = false;
            var settings = importer.defaultSampleSettings;
            settings.loadType = ambient ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
            settings.compressionFormat = ambient ? AudioCompressionFormat.Vorbis : AudioCompressionFormat.PCM;
            settings.quality = 1f;
            settings.sampleRateSetting = AudioSampleRateSetting.PreserveSampleRate;
            settings.preloadAudioData = !ambient;
            importer.defaultSampleSettings = settings;
            // With mono input and forceToMono=false no normalization/downmix is applied.
            importer.SaveAndReimport();
        }
        AssetDatabase.ForceReserializeAssets(Read<AudioManifest>(kit + "manifest.json").clips.Select(x => kit + x.file), ForceReserializeAssetsOptions.ReserializeMetadata);
        Run();
    }

    public static void Run()
    {
        Passed.Clear();
        var input = Read<SimulationInput>("tests/fixtures/simulation/input.json");
        var expected = Read<SimulationOutput>("tests/fixtures/simulation/expected.json");
        var before = JsonUtility.ToJson(input);
        var result = RunoffEvaluator.Evaluate(input);
        Check(result.revision == expected.revision && result.schemaVersion == 1, "Simulation assembly executes with Unity-deserialized frozen input");
        Check(Near(result.generatedM3, expected.generatedM3) && Near(result.retainedM3, expected.retainedM3) && Near(result.dischargedM3, expected.dischargedM3), "Unity aggregate simulation values match independent frozen example");
        foreach (var site in expected.sites)
        {
            var actual = result.sites.Single(x => x.siteId == site.siteId);
            Check(Near(actual.generatedM3, site.generatedM3) && Near(actual.retainedM3, site.retainedM3) && Near(actual.dischargedM3, site.dischargedM3), "Unity per-unit generated/retained/discharged match: " + site.siteId);
        }
        Check(before == JsonUtility.ToJson(input), "Simulation leaves Unity input DTOs unchanged");
        Check(JsonUtility.ToJson(result) == JsonUtility.ToJson(RunoffEvaluator.Evaluate(input)), "Simulation repeat is deterministic in Unity");
        Check(JsonUtility.ToJson(JsonUtility.FromJson<SimulationOutput>(JsonUtility.ToJson(result))) == JsonUtility.ToJson(result), "Simulation results and explanation facts survive Unity JSON round trip");

        var index = Read<ContentIndex>("content/education/bindings.json");
        var entries = new Dictionary<string, ContentEntry>(StringComparer.Ordinal);
        foreach (var file in index.entries.Concat(index.legacyEntries))
        {
            var entry = Read<ContentEntry>("content/education/" + file);
            Check(entry != null && !string.IsNullOrWhiteSpace(entry.id) && entry.choices != null && entry.sourceIds != null && !entries.ContainsKey(entry.id), "Unity content DTO loads: " + file);
            entries.Add(entry.id, entry);
            var json = JsonUtility.ToJson(entry);
            Check(JsonUtility.ToJson(JsonUtility.FromJson<ContentEntry>(json)) == json, "Unity content round trip preserves choices/feedback: " + entry.id);
        }
        var scenario = Read<Scenario>("worlds/meadowbrook-v1/scenario.json");
        Check(entries.ContainsKey(scenario.introId) && entries.ContainsKey(scenario.learningId), "Canonical scenario content IDs resolve without changing the scenario");
        foreach (var id in index.questionIds)
            Check(entries[id].choices.Count(x => x.correct) == 1 && entries[id].choices.All(x => !string.IsNullOrWhiteSpace(x.explanation)), "Unity question keeps exactly one best answer and all feedback: " + id);

        const string kit = "Assets/Terra/Audio/ExternalKit/";
        var manifest = Read<AudioManifest>(kit + "manifest.json");
        var audio = new List<AudioReadback>();
        foreach (var item in manifest.clips)
        {
            var path = kit + item.file;
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            var importer = AssetImporter.GetAtPath(path) as AudioImporter;
            Check(clip != null && importer != null && clip.channels == 1 && clip.frequency == 22050 && clip.samples > 0, "Unity imports mono 22050-Hz audio asset: " + item.id);
            Check(Math.Abs(clip.length - item.frames / 22050d) < .1, "Imported clip duration agrees with source: " + item.id);
            Check(clip.LoadAudioData() && clip.loadState == AudioDataLoadState.Loaded, "Unity loads audio data: " + item.id);
            bool ambient = item.id == "ambient-wind";
            Check(clip.loadType == (ambient ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad), "Unity uses intended load type: " + item.id);
            Check(!importer.forceToMono && importer.defaultSampleSettings.sampleRateSetting == AudioSampleRateSetting.PreserveSampleRate && importer.defaultSampleSettings.preloadAudioData == !ambient, "Unity preserves mastered mono/rate and preload intent: " + item.id);
            if (!ambient)
            {
                var samples = new float[clip.samples * clip.channels];
                Check(clip.GetData(samples, 0) && samples.Any(x => Math.Abs(x) > .00001f), "Unity decoded short cue contains signal: " + item.id);
            }
            audio.Add(new AudioReadback { id = item.id, guid = AssetDatabase.AssetPathToGUID(path),
                samples = clip.samples, channels = clip.channels, frequency = clip.frequency, durationSeconds = clip.length,
                loadType = clip.loadType.ToString(), compressionFormat = importer.defaultSampleSettings.compressionFormat.ToString(), forceToMono = importer.forceToMono });
        }
        // This existing check runs data/state logic only, without touching TerraMain.
        var priorEvidence = File.ReadAllBytes("docs/evidence/phase2/contract-checks.json");
        Phase2Checks.Report regression;
        try { regression = JsonUtility.FromJson<Phase2Checks.Report>(Phase2Checks.Run()); }
        finally { File.WriteAllBytes("docs/evidence/phase2/contract-checks.json", priorEvidence); }
        Check(regression.count >= 139, "Existing 139 Phase 2 data/state checks remain valid alongside all packages");
        var report = new Report { passed = true, engine = Application.unityVersion,
            scope = "Isolated Unity import/serialization/assembly execution; no scene or live-game feature integration",
            checks = Passed.ToArray(), contentEntries = entries.Count, phase2Checks = regression.count, audio = audio.ToArray() };
        Directory.CreateDirectory("docs/evidence/parallel-integration");
        File.WriteAllText("docs/evidence/parallel-integration/unity-checks.json", JsonUtility.ToJson(report, true) + "\n");
        Debug.Log("PARALLEL_PACKAGES_PASS: " + Passed.Count + " package checks; " + regression.count + " existing state checks");
    }
}
