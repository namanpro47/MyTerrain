var renderers=UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>(UnityEngine.FindObjectsSortMode.None).Where(r=>r.name=="Artist barn trial").ToArray();
var source=renderers[0].sharedMaterial;var lit=new UnityEngine.Material(UnityEngine.Shader.Find("Universal Render Pipeline/Simple Lit"));lit.SetTexture("_BaseMap",source.GetTexture("_BaseMap"));lit.SetColor("_BaseColor",UnityEngine.Color.white);lit.SetFloat("_Smoothness",0);lit.SetFloat("_SpecularHighlights",0);lit.SetFloat("_Cull",0);foreach(var r in renderers)r.sharedMaterial=lit;
return new {replaced=renderers.Length};
