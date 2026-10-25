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
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.WebGL)
            throw new BuildFailedException("Switch the active build target to WebGL before exporting; platform-specific assemblies must be imported first.");
        var scenes = EditorBuildSettings.scenes
            .Where(scene => scene.enabled)
            .Select(scene => scene.path)
            .ToArray();

        if (scenes.Length == 0)
            throw new BuildFailedException("Add an enabled scene to File > Build Profiles first.");

        // Lossless Brotli reduces arrival bytes without changing textures or game code.
        // Keep the fallback so the same export works on a plain local/static server.
        PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Brotli;
        PlayerSettings.WebGL.decompressionFallback = true;
        // Avoid mixing cached data/framework/wasm across static-site deployments.
        PlayerSettings.WebGL.nameFilesAsHashes = true;
        // Release exports optimize the player, not the editor's turnaround time.
        UnityEditor.WebGL.UserBuildSettings.codeOptimization = UnityEditor.WebGL.WasmCodeOptimization.RuntimeSpeedLTO;
        // Chrome's repeated ten-year runs reserve ~212 MiB. Avoid growing from
        // the 32 MiB default during arrival; geometric growth remains available.
        PlayerSettings.WebGL.initialMemorySize = 256;
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
        // Package only server source; credentials are supplied by the hosting environment.
        var api = Path.Combine(staging, "api"); Directory.CreateDirectory(api);
        File.Copy(Path.Combine(Application.dataPath, "../services/narration/api/narration.js"), Path.Combine(api, "narration.js"), true);
        var previous = output + ".previous";
        if (Directory.Exists(previous)) Directory.Delete(previous, true);
        if (Directory.Exists(output)) Directory.Move(output, previous);
        Directory.Move(staging, output);
        Debug.Log($"Terra Web build succeeded: {output} ({report.summary.totalSize} bytes)");
    }
}
