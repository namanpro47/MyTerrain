var preset=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Material>("Assets/Editor/StreamStudy/Uber/Template Materials/UWa-Template-Clear.mat");
if(preset==null||preset.shader==null)throw new System.Exception("Native preset is missing");
if(UnityEditor.ShaderUtil.ShaderHasError(preset.shader))return UnityEditor.ShaderUtil.GetShaderMessages(preset.shader);
var material=new UnityEngine.Material(preset);material.name="Native Clear water study";
// No planar-reflection component or rendering feature is installed for this study.
material.DisableKeyword("_ENABLEPLANERREFLECTION");material.SetFloat("_ENABLEPLANERREFLECTION",0);
var camera=UnityEngine.Camera.main;
var data=camera.GetComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
if(data==null)data=camera.gameObject.AddComponent<UnityEngine.Rendering.Universal.UniversalAdditionalCameraData>();
data.requiresDepthTexture=true;data.requiresColorTexture=true;
int changed=0;
foreach(var renderer in UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>(UnityEngine.FindObjectsSortMode.None))
 if(renderer.name=="Mapped stream centreline"){renderer.sharedMaterial=material;changed++;}
return new{changed,shader=preset.shader.name,supported=preset.shader.isSupported,keywords=material.shaderKeywords,depth=data.requiresDepthTexture,opaque=data.requiresColorTexture};
