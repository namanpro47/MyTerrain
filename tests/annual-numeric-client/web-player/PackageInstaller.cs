using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
public static class NumericPackageInstaller
{
 static AddAndRemoveRequest request;static double deadline;
 public static void Install(){request=Client.AddAndRemove(new[]{"com.unity.nuget.newtonsoft-json@3.2.2","com.unity.modules.jsonserialize@1.0.0"},new string[0]);deadline=EditorApplication.timeSinceStartup+600;EditorApplication.update+=Poll;}
 static void Poll(){if(!request.IsCompleted){if(EditorApplication.timeSinceStartup>deadline){Debug.LogError("Numeric package installation timed out");EditorApplication.Exit(2);}return;}
  EditorApplication.update-=Poll;if(request.Status!=StatusCode.Success){Debug.LogError(request.Error?.message);EditorApplication.Exit(1);return;}
  Debug.Log("Numeric test packages: "+string.Join(", ",request.Result.Select(p=>p.name+"@"+p.version)));EditorApplication.Exit(0);}
}
