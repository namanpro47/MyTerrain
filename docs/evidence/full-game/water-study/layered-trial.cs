var shader=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Shader>("Assets/Editor/StreamStudy/LayeredStream.shader");
if(shader==null||UnityEditor.ShaderUtil.ShaderHasError(shader))throw new System.Exception("Trial water shader did not compile");
var renderers=UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>(UnityEngine.FindObjectsSortMode.None);
int changed=0;
foreach(var r in renderers){
 if(r.name!="Mapped stream centreline")continue;
 var original=r.sharedMaterial;
 var trial=new UnityEngine.Material(shader);
 trial.SetFloat("_NaturalBanks",1);
 trial.SetTexture("_RiverMap",original.GetTexture("_RiverMap"));
 trial.SetVector("_LandBounds",original.GetVector("_LandBounds"));
 trial.SetFloat("_MotionTime",original.GetFloat("_MotionTime"));
 trial.SetTexture("_BedMap",UnityEngine.Resources.Load<UnityEngine.Texture2D>("JulioGround/StonesColor"));
 r.sharedMaterial=trial;changed++;
}
return new{changed,shaderSupported=shader.isSupported};
