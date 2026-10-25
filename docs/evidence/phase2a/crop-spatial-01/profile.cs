var camera=UnityEngine.Camera.main;var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic|System.Reflection.BindingFlags.Public;
var rows=new System.Collections.Generic.List<object>();var planes=UnityEngine.GeometryUtility.CalculateFrustumPlanes(camera);float factor=camera.pixelHeight/2f,span=UnityEngine.Mathf.Tan(camera.fieldOfView*.5f*UnityEngine.Mathf.Deg2Rad);
foreach(var n in UnityEngine.Object.FindObjectsByType<Terra.Rendering.InstancedNature>())foreach(var b in (System.Collections.IEnumerable)n.GetType().GetField("batches",flags).GetValue(n)){
object Get(string name)=>b.GetType().GetField(name,flags).GetValue(b);
var mesh=(UnityEngine.Mesh)Get("mesh");if(!mesh.name.StartsWith("LeafCrop"))continue;
var bounds=(UnityEngine.Bounds)Get("bounds");var shadow=bounds;shadow.Expand((float)Get("maxHeight")*8+bounds.size.y*2+16);bool visible=UnityEngine.GeometryUtility.TestPlanesAABB(planes,shadow);
float depth=UnityEngine.Mathf.Max(1,UnityEngine.Vector3.Dot(bounds.center-camera.transform.position,camera.transform.forward)-bounds.extents.magnitude);float pixels=(float)Get("detailExtent")*factor/(depth*span);
var medium=(UnityEngine.Mesh)Get("medium");var distant=(UnityEngine.Mesh)Get("distant");int lod=pixels<(float)Get("mediumDetailPixels")?2:pixels<(float)Get("highDetailPixels")?1:0;var selected=lod==2?distant:lod==1?medium:mesh;
var transforms=(UnityEngine.Matrix4x4[])Get("transforms");int sub=(int)Get("submesh");rows.Add(new{owner=n.name,count=transforms.Length,visible,boundsX=bounds.size.x,boundsZ=bounds.size.z,pixels,lod,triangles=(long)selected.GetIndexCount(sub)/3*transforms.Length});
}
var soils=UnityEngine.Object.FindObjectsByType<UnityEngine.MeshRenderer>().Where(r=>r.name=="Cultivated soil beds").ToArray();
return new{frame=UnityEngine.Time.frameCount,rows,soilSurfaces=soils.Length,soilVertices=soils.Sum(r=>r.GetComponent<UnityEngine.MeshFilter>().sharedMesh.vertexCount),shaderErrors=UnityEditor.ShaderUtil.GetShaderMessages(UnityEngine.Resources.Load<UnityEngine.Shader>("TerraGround")),stats=new{UnityEditor.UnityStats.triangles}};