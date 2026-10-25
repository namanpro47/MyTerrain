using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
public static class NumericWebBuild
{
 public static void Run()
 {
  Terra.EditorChecks.AnnualNumericClientChecks.Run();
  var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
  new GameObject("NumericWebCheck").AddComponent<NumericWebCheck>();
  new GameObject("Camera").AddComponent<Camera>().backgroundColor=Color.gray;
  EditorSceneManager.SaveScene(scene,"Assets/NumericCheck.unity");
  PlayerSettings.WebGL.compressionFormat=WebGLCompressionFormat.Disabled;
  PlayerSettings.WebGL.template="PROJECT:NumericCheck";
  var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions {scenes=new[]{"Assets/NumericCheck.unity"},
   locationPathName=Path.GetFullPath("Build"),target=BuildTarget.WebGL,options=BuildOptions.None});
  if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Numeric WebGL verification build failed: "+report.summary.result);
 }
}
