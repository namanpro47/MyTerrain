var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var main=UnityEngine.Object.FindAnyObjectByType<Terra.Gameplay.TerraMain>();
var land=typeof(Terra.Gameplay.TerraMain).GetField("landscape",flags).GetValue(main);
var t=land.GetType(); object Field(string n)=>t.GetField(n,flags).GetValue(land);
var world=(Terra.Contracts.WorldData)typeof(Terra.Gameplay.TerraMain).GetField("world",flags).GetValue(main);
var site=world.sites.First(s=>s.id=="south-07");
var shape=t.GetMethod("SwaleFor",flags).Invoke(land,new object[]{site});
var route=(Terra.Contracts.Point[])shape.GetType().GetField("route").GetValue(shape);
var bounds=(UnityEngine.Bounds)shape.GetType().GetField("bounds").GetValue(shape);
float half=(float)shape.GetType().GetField("halfWidth").GetValue(shape);
var pixels=(UnityEngine.Color32[])Field("surfacePixels");
var texture=(UnityEngine.Texture2D)Field("treatmentSurface");
if(!System.IO.File.Exists("/tmp/terra-swale-original-pixels.bin"))System.IO.File.WriteAllBytes("/tmp/terra-swale-original-pixels.bin",pixels.SelectMany(p=>new byte[]{p.r,p.g,p.b,p.a}).ToArray());
var original=System.IO.File.ReadAllBytes("/tmp/terra-swale-original-pixels.bin");
for(int i=0;i<pixels.Length;i++){pixels[i]=new UnityEngine.Color32(original[i*4],original[i*4+1],(byte)UnityEngine.Mathf.RoundToInt(255*UnityEngine.Mathf.Pow(original[i*4+2]/255f,3)),original[i*4+3]);}
texture.SetPixels32(pixels);texture.Apply();
var go=UnityEngine.GameObject.Find("Swale shoulder study");if(go!=null)UnityEngine.Object.DestroyImmediate(go);go=new UnityEngine.GameObject("Swale shoulder study");
var low=UnityEngine.Resources.Load<UnityEngine.GameObject>("Nature/Grass_Common_Short");
var rush=UnityEngine.Resources.Load<UnityEngine.GameObject>("Nature/Grass_Common_Tall");
var lows=new System.Collections.Generic.List<UnityEngine.Matrix4x4>();var tall=new System.Collections.Generic.List<UnityEngine.Matrix4x4>();
var rng=new System.Random(319);
float J(float a,float b)=>a+(b-a)*(float)rng.NextDouble();
float Dist(float x,float z){float best=float.MaxValue;var p=new UnityEngine.Vector2(x,z);for(int i=1;i<route.Length;i++){var a=new UnityEngine.Vector2(route[i-1].x,route[i-1].z);var b=new UnityEngine.Vector2(route[i].x,route[i].z);var d=b-a;float u=UnityEngine.Mathf.Clamp01(UnityEngine.Vector2.Dot(p-a,d)/d.sqrMagnitude);best=UnityEngine.Mathf.Min(best,UnityEngine.Vector2.Distance(p,a+u*d));}return best;}
var matrix=t.GetMethod("PlantMatrix",flags);var cutMethod=t.GetMethod("SwaleCut",flags);
for(float z=bounds.min.z+4;z<bounds.max.z-4;z+=1.5f)for(float x=bounds.min.x+4;x<bounds.max.x-4;x+=1.5f){float px=x+J(-.4f,.4f),pz=z+J(-.4f,.4f),d=Dist(px,pz);if(d<2.2f||d>half*.85f)continue;float cut=(float)cutMethod.Invoke(land,new object[]{shape,px,pz});if(cut<=0)continue;float cohort=UnityEngine.Mathf.PerlinNoise(px*.075f+100,pz*.075f+100);if(cohort<.36f)continue;var m=(UnityEngine.Matrix4x4)matrix.Invoke(land,new object[]{low,px,pz,J(1.2f,1.6f),J(0,360)});m=m*UnityEngine.Matrix4x4.Scale(new UnityEngine.Vector3(2.2f,1,2.2f));m.m13-=cut;lows.Add(m);if(cohort>.55f&&rng.NextDouble()<.25){m=(UnityEngine.Matrix4x4)matrix.Invoke(land,new object[]{rush,px,pz,J(1.1f,1.6f),J(0,360)});m.m13-=cut;tall.Add(m);}}
var plants=go.AddComponent<Terra.Rendering.InstancedNature>();plants.Add(low,lows,UnityEngine.Color.white,true);plants.Add(rush,tall,UnityEngine.Color.white);
return new {low=lows.Count,tall=tall.Count,halfWidth=half,frame=UnityEngine.Time.frameCount,temporary=true};
