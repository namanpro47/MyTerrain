var folder="Assets/Editor/CozyCropTrial";
System.IO.Directory.CreateDirectory(folder);
System.IO.File.Copy("docs/evidence/full-game/cozy-crop-study/source/wheat.fbx",folder+"/wheat.fbx",true);
System.IO.File.Copy("docs/evidence/full-game/cozy-crop-study/source/palette-0.png",folder+"/palette.png",true);
UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);
var model=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(folder+"/wheat.fbx");
return model.GetComponentsInChildren<UnityEngine.MeshFilter>().Select(f=>new {name=f.name,verts=f.sharedMesh.vertexCount,bounds=f.sharedMesh.bounds.ToString(),position=f.transform.position.ToString(),scale=f.transform.lossyScale.ToString(),rotation=f.transform.rotation.eulerAngles.ToString(),material=f.GetComponent<UnityEngine.MeshRenderer>().sharedMaterial.name}).ToArray();
