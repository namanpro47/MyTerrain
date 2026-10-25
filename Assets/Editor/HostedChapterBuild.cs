using System;
using Terra.Gameplay;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;
using UnityEngine.SceneManagement;

// Configure the temporary build scene, not the editable scene or local developer endpoint.
public sealed class HostedChapterBuild : IProcessSceneWithReport
{
    public const string ProductionEndpoint = "https://terra-chapters.vercel.app/chapter";
    public int callbackOrder => 100;
    public static string EndpointForBuild(string configured, BuildTarget target)
        => target == BuildTarget.WebGL && Uri.TryCreate(configured, UriKind.Absolute, out var uri) && uri.IsLoopback
            ? ProductionEndpoint : configured;

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        if (report == null || report.summary.platform != BuildTarget.WebGL) return;
        foreach (var root in scene.GetRootGameObjects())
            foreach (var main in root.GetComponentsInChildren<TerraMain>(true))
            {
                main.annualChapterEndpoint = EndpointForBuild(main.annualChapterEndpoint, report.summary.platform);
                if (!Uri.TryCreate(main.annualChapterEndpoint, UriKind.Absolute, out var endpoint) || endpoint.Scheme != "https")
                    throw new BuildFailedException("A public WebGL player requires an HTTPS chapter endpoint.");
                Debug.Log("TERRA_HOSTED_CHAPTER_ENDPOINT: " + main.annualChapterEndpoint);
            }
    }
}
