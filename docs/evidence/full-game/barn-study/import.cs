var folder="Assets/Editor/BarnStudy";
System.IO.Directory.CreateDirectory(folder);
foreach(var path in System.IO.Directory.GetFiles("docs/evidence/full-game/barn-study/source").Where(p=>p.EndsWith(".fbx")||p.EndsWith(".png")))System.IO.File.Copy(path,folder+"/"+System.IO.Path.GetFileName(path),true);
UnityEditor.AssetDatabase.Refresh(UnityEditor.ImportAssetOptions.ForceSynchronousImport);
return new[]{"barnlvl1"}.Select(name=>{var model=UnityEditor.AssetDatabase.LoadAssetAtPath<UnityEngine.GameObject>(folder+"/"+name+".fbx");return new {name,meshes=model.GetComponentsInChildren<UnityEngine.MeshFilter>().Select(f=>new{name=f.name,vertices=f.sharedMesh.vertexCount,bounds=f.GetComponent<UnityEngine.MeshRenderer>().bounds.ToString(),position=f.transform.position.ToString(),rotation=f.transform.rotation.eulerAngles.ToString(),scale=f.transform.lossyScale.ToString()}).ToArray()};}).ToArray();
