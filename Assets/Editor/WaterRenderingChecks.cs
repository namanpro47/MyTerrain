using System;
using System.IO;
using System.Linq;
using System.Reflection;
using Terra.Contracts;
using Terra.Gameplay;
using Terra.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public static class WaterRenderingChecks
{
    public static string Run()
    {
        if(!EditorApplication.isPlaying)throw new InvalidOperationException("Run in the playing Wisconsin main scene after loading.");
        int checks=0;
        void Check(bool value,string why){if(!value)throw new Exception(why);checks++;}
        var main=UnityEngine.Object.FindFirstObjectByType<TerraMain>();
        var flags=BindingFlags.Instance|BindingFlags.NonPublic;
        var worldField=typeof(TerraMain).GetField("world",flags);
        var cameraField=typeof(TerraMain).GetField("view",flags);
        var world=(WorldData)worldField.GetValue(main);
        var camera=(Camera)cameraField.GetValue(main);
        Check(world.features.Any(f=>f.kind=="water"),"Wisconsin water fixture required.");
        var landscape=(LandscapeRenderer)typeof(TerraMain).GetField("landscape",flags).GetValue(main);
        var materials=UnityEngine.Object.FindObjectsByType<MeshRenderer>(FindObjectsSortMode.None)
            .Where(r=>r.name=="Mapped stream centreline").Select(r=>r.sharedMaterial).Distinct().ToArray();
        Check(materials.Length==1,"Stream surfaces must share a single owned runtime material.");
        var material=materials.Single();
        Check(material.shader.name=="UberStylizedWater"&&!ShaderUtil.ShaderHasError(material.shader),"Native water shader is unavailable or failed compilation.");
        Check(material.HasFloat("_MotionTime"),"Reduced-motion clock is absent.");
        Check(material.IsKeywordEnabled("_ENABLENORMAL")&&material.IsKeywordEnabled("_ENABLEREFRACTION"),"Authored surface layers are missing.");
        Check(!material.IsKeywordEnabled("_ENABLEPLANERREFLECTION"),"Unsupported planar reflection is enabled.");
        var cameraData=camera.GetComponent<UniversalAdditionalCameraData>();
        Check(cameraData!=null&&cameraData.requiresDepthTexture&&cameraData.requiresColorTexture,"Stream camera must provide the depth and color inputs.");
        var pipeline=(UniversalRenderPipelineAsset)UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline;
        Check(!pipeline.supportsCameraDepthTexture&&!pipeline.supportsCameraOpaqueTexture,"The shared mobile quality asset was modified.");
        float priorClock=material.GetFloat("_MotionTime");
        landscape.Tick(12.5f,false);Check(material.GetFloat("_MotionTime")==12.5f,"Full motion does not advance the water clock.");
        landscape.Tick(18f,true);Check(material.GetFloat("_MotionTime")==0,"Reduced motion must freeze all water layers.");
        landscape.Tick(priorClock,false);
        string priorDiagnostic=main.Diagnostic;
        var temporaryCamera=new GameObject("Water camera isolation check");
        try
        {
            var isolated=temporaryCamera.AddComponent<Camera>();isolated.enabled=false;
            cameraField.SetValue(main,isolated);
            foreach(var id in new[]{"point-reyes-v1","suusamyr-v1"})
            {
                worldField.SetValue(main,JsonUtility.FromJson<WorldData>(File.ReadAllText("worlds/"+id+"/world.json")));
                typeof(TerraMain).GetMethod("ConfigureWaterRendering",flags).Invoke(main,null);
                Check(isolated.GetComponent<UniversalAdditionalCameraData>()==null,id+" must not allocate water rendering passes.");
            }
        }
        finally
        {
            cameraField.SetValue(main,camera);worldField.SetValue(main,world);
            UnityEngine.Object.DestroyImmediate(temporaryCamera);
        }
        Check(main.Diagnostic==priorDiagnostic,"Rendering checks changed the annual ledger.");
        return "Water rendering: "+checks+" assertions passed; authored surface, camera isolation, reduced motion, unchanged annual state.";
    }
}
