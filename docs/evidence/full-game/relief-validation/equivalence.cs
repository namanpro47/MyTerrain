var main=UnityEngine.Object.FindFirstObjectByType<Terra.Gameplay.TerraMain>();
var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.NonPublic;
var land=(Terra.Rendering.LandscapeRenderer)main.GetType().GetField("landscape",flags).GetValue(main);
var world=(Terra.Contracts.WorldData)main.GetType().GetField("world",flags).GetValue(main);
float Field(string n)=>(float)land.GetType().GetField(n,flags).GetValue(land);
float xmin=Field("xmin"),xmax=Field("xmax"),zmin=Field("zmin"),zmax=Field("zmax"),cell=Field("cell");
bool clusteredWoodland=(bool)land.GetType().GetField("clusteredWoodland",flags).GetValue(land);
float Height(float x,float z)=>land.Height(x,z);
UnityEngine.Vector3 Normal(float x,float z)=>new UnityEngine.Vector3(Height(x-1,z)-Height(x+1,z),2,Height(x,z-1)-Height(x,z+1)).normalized;
        void Reference(Terra.Contracts.Point[] points,float lift,(System.Collections.Generic.List<UnityEngine.Vector3> vertices,System.Collections.Generic.List<UnityEngine.Vector3> normals,System.Collections.Generic.List<int> triangles) g)
        {
            // Overlays use the same grid as the displayed terrain. Coarse source
            // triangles would bridge across the illustrative slopes and float above them.
            float dx=clusteredWoodland?(xmax-xmin)/UnityEngine.Mathf.Ceil((xmax-xmin)/1.5f):cell;
            float dz=clusteredWoodland?(zmax-zmin)/UnityEngine.Mathf.Ceil((zmax-zmin)/1.5f):cell;
            int nx=UnityEngine.Mathf.RoundToInt((xmax-xmin)/dx),nz=UnityEngine.Mathf.RoundToInt((zmax-zmin)/dz);
            int x0=UnityEngine.Mathf.Clamp(UnityEngine.Mathf.FloorToInt((points.Min(p=>p.x)-xmin)/dx),0,nx-1),x1=UnityEngine.Mathf.Clamp(UnityEngine.Mathf.FloorToInt((points.Max(p=>p.x)-xmin)/dx),0,nx-1);
            int z0=UnityEngine.Mathf.Clamp(UnityEngine.Mathf.FloorToInt((points.Min(p=>p.z)-zmin)/dz),0,nz-1),z1=UnityEngine.Mathf.Clamp(UnityEngine.Mathf.FloorToInt((points.Max(p=>p.z)-zmin)/dz),0,nz-1);
            var polygon=points.Select(p=>new UnityEngine.Vector2(p.x,p.z)).ToList();
            for(int z=z0;z<=z1;z++)for(int x=x0;x<=x1;x++)
            {
                float px=xmin+x*dx,pz=zmin+z*dz;var a=new UnityEngine.Vector2(px,pz);var b=new UnityEngine.Vector2(px+dx,pz);var c=new UnityEngine.Vector2(px,pz+dz);var d=new UnityEngine.Vector2(px+dx,pz+dz);
                Add(ClipTriangle(polygon,a,b,c));Add(ClipTriangle(polygon,b,d,c));
            }
            void Add(System.Collections.Generic.List<UnityEngine.Vector2> poly)
            {
                if(poly.Count<3)return;
                for(int j=1;j<poly.Count-1;j++)
                {
                    if(UnityEngine.Mathf.Abs(Cross(poly[j]-poly[0],poly[j+1]-poly[0]))<.00001f)continue;
                    int n=g.vertices.Count;
                    foreach(var p in new[]{poly[0],poly[j+1],poly[j]}){g.vertices.Add(new UnityEngine.Vector3(p.x,Height(p.x,p.y)+lift,p.y));g.normals.Add(Normal(p.x,p.y));}
                    g.triangles.AddRange(new[]{n,n+1,n+2});
                }
            }
        }
        float Cross(UnityEngine.Vector2 a,UnityEngine.Vector2 b)=>a.x*b.y-a.y*b.x;
        System.Collections.Generic.List<UnityEngine.Vector2> ClipTriangle(System.Collections.Generic.List<UnityEngine.Vector2> input,UnityEngine.Vector2 a,UnityEngine.Vector2 b,UnityEngine.Vector2 c)
        {
            var output=input;var triangle=new[]{a,b,c};
            for(int i=0;i<3&&output.Count>0;i++)
            {
                var start=triangle[i];var edge=triangle[(i+1)%3]-start;var next=new System.Collections.Generic.List<UnityEngine.Vector2>();var previous=output[output.Count-1];float pd=Cross(edge,previous-start);
                foreach(var current in output)
                {
                    float cd=Cross(edge,current-start);
                    if((cd>=0)!=(pd>=0))next.Add(UnityEngine.Vector2.LerpUnclamped(previous,current,pd/(pd-cd)));
                    if(cd>=0)next.Add(current);previous=current;pd=cd;
                }
                output=next;
            }
            return output;
        }

var geometryType=land.GetType().GetNestedType("Geometry",System.Reflection.BindingFlags.NonPublic);
var surface=land.GetType().GetMethod("Surface",flags);
var shapes=world.sites.Select(s=>(s.id,s.polygon)).ToList();
shapes.Add(("rotated",new[]{new Terra.Contracts.Point(40,40),new Terra.Contracts.Point(160,65),new Terra.Contracts.Point(140,145),new Terra.Contracts.Point(20,120)}));
shapes.Add(("reverse-winding",shapes.Last().Item2.Reverse().ToArray()));
int checks=0,triangles=0;float maxPositionError=0,maxNormalError=0;var results=new System.Collections.Generic.List<object>();
var displayedLand=land;
foreach(bool fineGrid in new[]{true,false})
{
 land=fineGrid?displayedLand:new Terra.Rendering.LandscapeRenderer(world,null,null,null,null,null,null,false);
 clusteredWoodland=fineGrid;
 foreach(var shape in shapes)
 {
 var reference=(new System.Collections.Generic.List<UnityEngine.Vector3>(),new System.Collections.Generic.List<UnityEngine.Vector3>(),new System.Collections.Generic.List<int>());
 var watch=System.Diagnostics.Stopwatch.StartNew();Reference(shape.Item2,.22f,reference);watch.Stop();double oldMs=watch.Elapsed.TotalMilliseconds;
 var g=System.Activator.CreateInstance(geometryType,true);
 watch.Restart();surface.Invoke(land,new object[]{shape.Item2,.22f,g});watch.Stop();double newMs=watch.Elapsed.TotalMilliseconds;
 var vertices=(System.Collections.Generic.List<UnityEngine.Vector3>)geometryType.GetField("vertices").GetValue(g);var normals=(System.Collections.Generic.List<UnityEngine.Vector3>)geometryType.GetField("normals").GetValue(g);var indices=(System.Collections.Generic.List<int>)geometryType.GetField("triangles").GetValue(g);
 double discardedOld=0,discardedNew=0;
 void RemoveSlivers(System.Collections.Generic.List<UnityEngine.Vector3> v,System.Collections.Generic.List<int> ids,ref double area)
 {
  for(int t=ids.Count-3;t>=0;t-=3)
  {var a=v[ids[t]];var b=v[ids[t+1]];var c=v[ids[t+2]];double size=System.Math.Abs((double)(b.x-a.x)*(c.z-a.z)-(double)(b.z-a.z)*(c.x-a.x))*.5;
   if(size<.001){area+=size;ids.RemoveRange(t,3);}}
 }
 int rawOld=reference.Item3.Count/3,rawNew=indices.Count/3;
 RemoveSlivers(reference.Item1,reference.Item3,ref discardedOld);RemoveSlivers(vertices,indices,ref discardedNew);
 if(discardedOld>.01||discardedNew>.01)throw new System.Exception(shape.Item1+": excessive discarded area "+discardedOld+"/"+discardedNew);
 if(indices.Count!=reference.Item3.Count)throw new System.Exception(shape.Item1+": triangle count changed "+indices.Count+"/"+reference.Item3.Count);checks++;
 for(int t=0;t<indices.Count;t+=3)
 {
  int offset=-1;float best=float.MaxValue;
  for(int shift=0;shift<3;shift++)
  {float error=0;for(int j=0;j<3;j++)error=UnityEngine.Mathf.Max(error,UnityEngine.Vector3.Distance(reference.Item1[reference.Item3[t+j]],vertices[indices[t+(j+shift)%3]]));if(error<best){best=error;offset=shift;}}
  if(best>.001f)throw new System.Exception(shape.Item1+": triangle geometry/winding "+t+" error "+best);
  maxPositionError=UnityEngine.Mathf.Max(maxPositionError,best);
  for(int j=0;j<3;j++){float error=UnityEngine.Vector3.Distance(reference.Item2[reference.Item3[t+j]],normals[indices[t+(j+offset)%3]]);maxNormalError=UnityEngine.Mathf.Max(maxNormalError,error);if(error>.001f)throw new System.Exception("Normal error "+error);}
  checks+=2;triangles++;
 }
 results.Add(new{fineGrid,shape=shape.Item1,rawOld,rawNew,discardedOld,discardedNew,triangles=indices.Count/3,oldVertices=reference.Item1.Count,newVertices=vertices.Count,oldMs,newMs});
}
}
return new{checks,triangles,maxPositionError,maxNormalError,results,frame=UnityEngine.Time.frameCount};
