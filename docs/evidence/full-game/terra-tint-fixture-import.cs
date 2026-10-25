System.IO.Directory.CreateDirectory("Assets/Editor/TintTypeFixture");
System.IO.File.WriteAllText("Assets/Editor/TintTypeFixture/Texture.shader", "Shader \"Hidden/Terra/TintTypeFixture\" { Properties { _BaseColor (\"Texture, not tint\", 2D) = \"white\" {} _FixtureSentinel (\"Sentinel\", Float) = 0 } SubShader { Pass {} } }");
UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);
var shader=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.Shader>("Assets/Editor/TintTypeFixture/Texture.shader");return new {shader=shader.name,errors=UnityEditor.ShaderUtil.GetShaderMessages(shader).Select(m=>m.message)};
