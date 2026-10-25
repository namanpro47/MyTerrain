using System;
using System.IO;
using System.Linq;
using Terra.Audio;
using Terra.Core;
using UnityEditor;
using UnityEngine;

public static class TerraAudioChecks
{
    [Serializable] sealed class Receipt { public int checks, clips, bytes; public string[] families; public string unity, result; }
    public static string Run()
    {
        int checks = 0; void Need(bool value, string message) { checks++; if (!value) throw new Exception("Audio check: " + message); }
        var families = FullAnnualGameplay.Supported.Select(TerraAudio.PlacementFamily).ToArray();
        Need(families.All(f => f != null), "Every playable action has an intentional sound");
        Need(families.Distinct().Count() == 17, "17 distinct material gestures");
        Need(TerraAudio.PlacementFamily("unknown-action") == null, "Unknown actions use neutral feedback");
        foreach (var family in families)
        {
            var variants = Enumerable.Range(0,3).Select(i => Resources.Load<AudioClip>("Audio/place-" + family + "-" + i)).ToArray();
            Need(variants.All(c => c != null), "Missing variant: " + family);
            Need(variants.Select(AssetDatabase.GetAssetPath).Distinct().Count() == 3, "Repeated take: " + family);
        }
        string[] cues = { "ui-click", "ui-focus", "ui-open", "ui-close", "ui-confirm", "tool-select", "cancel", "undo", "redo", "blocked", "move", "remove", "clear", "camera", "atlas-select", "travel", "arrival", "year-submit", "year-report", "year-attention", "year-continue", "new-run", "decade-complete" };
        foreach (var id in cues) Need(Resources.Load<AudioClip>("Audio/"+id) != null,"Missing event: "+id);
        var paths = AssetDatabase.FindAssets("t:AudioClip",new[]{"Assets/Terra/Resources/Audio"}).Select(AssetDatabase.GUIDToAssetPath).ToArray();
        foreach (var path in paths)
        {
            var clip=AssetDatabase.LoadAssetAtPath<AudioClip>(path);
            var importer=(AudioImporter)AssetImporter.GetAtPath(path);
            bool bed=clip.name.StartsWith("music-")||clip.name.StartsWith("ambient-");
            Need(clip.length > .02f && clip.channels >= 1 && clip.channels <= 2,"Usable audio: "+path);
            Need(importer.defaultSampleSettings.loadType==(bed?AudioClipLoadType.CompressedInMemory:AudioClipLoadType.DecompressOnLoad),"Import memory policy: "+path);
            Need(!importer.defaultSampleSettings.preloadAudioData == bed,"Long tracks load after gesture: "+path);
            Need(!importer.forceToMono || clip.channels==1,"Stereo music retained");
        }
        Need(Resources.Load<AudioClip>("Audio/music-mystical").length > 90,"Full first piece");
        Need(Resources.Load<AudioClip>("Audio/music-vaporware").length > 120,"Full second piece");
        Need(File.Exists("Assets/Terra/Resources/Audio/LICENSE.txt"),"License shipped");
        Need(File.Exists("Assets/Terra/Resources/Audio/manifest.json"),"Provenance shipped");
        var receipt = new Receipt { checks=checks,clips=paths.Length,bytes=paths.Sum(p=>(int)new FileInfo(p).Length),families=families,unity=Application.unityVersion,result="passed" };
        string json=JsonUtility.ToJson(receipt,true);Directory.CreateDirectory("docs/evidence/audio");File.WriteAllText("docs/evidence/audio/unity-import.json",json);Debug.Log("TERRA_AUDIO_CHECKS: "+json);return json;
    }
    public static void ValidateAndBuild() { Run(); TerraBuild.BuildWeb(); }
}
