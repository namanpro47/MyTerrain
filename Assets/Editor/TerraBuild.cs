using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// A repeatable browser build for the local prototype and future CI.
public static class TerraBuild
{
    [MenuItem("Terra/Build Web")]
    public static void BuildWeb()
    {
        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new BuildFailedException("Add an enabled scene to File > Build Profiles first.");

        // The fallback lets a plain local/static web server serve the build.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
        PlayerSettings.WebGL.decompressionFallback = true;
        // Avoid mixing cached data/framework/wasm across static-site deployments.
        PlayerSettings.WebGL.nameFilesAsHashes = true;
        bool phase1 = scenes.Any(scene => scene.EndsWith("TerraMain.unity"));
        PlayerSettings.WebGL.template = phase1 ? "PROJECT:Terra" : "PROJECT:Phase0";
        if (phase1) Phase1Setup.PackageFixture();

        var output = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/Web"));
        var staging = Path.GetFullPath(Path.Combine(Application.dataPath, "../Builds/Staging/Web"));
        if (Directory.Exists(staging)) Directory.Delete(staging, true);
        var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
        {
            scenes = scenes,
            locationPathName = staging,
            target = BuildTarget.WebGL,
            options = BuildOptions.None
        });

        if (report.summary.result != BuildResult.Succeeded)
            throw new BuildFailedException($"Web build failed: {report.summary.result}");

        File.Copy(Path.Combine(Application.dataPath, "../scripts/vercel.json"), Path.Combine(staging, "vercel.json"), true);
        var previous = output + ".previous";
        if (Directory.Exists(previous)) Directory.Delete(previous, true);
        if (Directory.Exists(output)) Directory.Move(output, previous);
        Directory.Move(staging, output);
        Debug.Log($"Terra Web build succeeded: {output} ({report.summary.totalSize} bytes)");
    }
}
