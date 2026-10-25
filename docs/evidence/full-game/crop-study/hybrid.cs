var root=UnityEngine.GameObject.Find("Crop row volume study");if(root==null)throw new System.Exception("Run row-volumes first");
foreach(var filter in root.GetComponentsInChildren<UnityEngine.MeshFilter>()){var mesh=filter.sharedMesh;var v=mesh.vertices;for(int i=0;i<v.Length;i++)v[i].y-=.25f;mesh.vertices=v;mesh.RecalculateBounds();}
foreach(var nature in UnityEngine.Object.FindObjectsByType<Terra.Rendering.InstancedNature>(UnityEngine.FindObjectsSortMode.None))nature.enabled=true;
return new{frame=UnityEngine.Time.frameCount,annual=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>().Diagnostic};
