using System;
using System.Diagnostics;
using System.IO;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine;

// Every normal build includes the prepared catalog; no separate manual staging dependency.
public sealed class CommunityBuild : IPreprocessBuildWithReport
{
    public int callbackOrder=>0;
    public void OnPreprocessBuild(BuildReport report){Stage();}
    public static void Stage()
    {
        string root=Path.GetFullPath(Path.Combine(Application.dataPath,".."));
        var start=new ProcessStartInfo("/usr/bin/env","python3 scripts/prepare-communities.py")
        {WorkingDirectory=root,UseShellExecute=false,RedirectStandardOutput=true,RedirectStandardError=true,CreateNoWindow=true};
        using(var process=Process.Start(start))
        {
            var output=process.StandardOutput.ReadToEnd();var error=process.StandardError.ReadToEnd();process.WaitForExit();
            if(process.ExitCode!=0)throw new BuildFailedException("Community staging failed: "+error);
            UnityEngine.Debug.Log(output);
        }
        UnityEditor.AssetDatabase.Refresh();
    }
}
