using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        // Illustrative cultivation, not observed crop species or surveyed agricultural practice.
        void PlantCropBeds(Point[] polygon,Transform parent,bool baseline,bool context=false)
        {
            var plant=Resources.Load<GameObject>("CultivatedBeds/LeafCrop");
            var soil=Resources.Load<Material>("CultivatedBeds/Soil");
            if(plant==null||soil==null)throw new System.InvalidOperationException("Missing cultivated-bed resources.");
            float l=polygon.Min(p=>p.x),r=polygon.Max(p=>p.x),b=polygon.Min(p=>p.z),t=polygon.Max(p=>p.z);
            var insideCache=new Dictionary<Vector2,bool>();
            var groundCache=new Dictionary<Vector2,float>();
            bool Within(float x,float z)
            {
                var key=new Vector2(x,z);
                if(!insideCache.TryGetValue(key,out bool value))
                {
                    value=(context?Inside(x,z):Contains(polygon,x,z))&&(!baseline||VisualCropAt(x,z))
                        &&(!context||(!NearLine(x,z,8)&&!world.sites.Any(site=>Contains(site.polygon,x,z))));
                    insideCache.Add(key,value);
                }
                return value;
            }
            float Ground(float x,float z)
            {
                var key=new Vector2(x,z);
                if(!groundCache.TryGetValue(key,out float value)){value=Height(x,z);groundCache.Add(key,value);}
                return value;
            }
            float Edge(float x,float z)
            {
                float distance=float.MaxValue;var p=new Vector2(x,z);
                for(int i=0;i<polygon.Length;i++)
                {
                    var a=new Vector2(polygon[i].x,polygon[i].z);var q=polygon[(i+1)%polygon.Length];var d=new Vector2(q.x,q.z)-a;
                    float along=Mathf.Clamp01(Vector2.Dot(p-a,d)/Mathf.Max(.00001f,d.sqrMagnitude));
                    distance=Mathf.Min(distance,Vector2.Distance(p,a+d*along));
                }
                return distance;
            }
            float BedHeight(float x,float z,float across)=>.62f*Mathf.Pow(Mathf.Max(0,1-across*across/1.96f),.7f)
                *(context?1:Mathf.SmoothStep(0,1,Mathf.Clamp01(Edge(x,z)/4)));
            var geometry=new Geometry();var plants=new List<Matrix4x4>();
            float[] cross={-1.4f,-.9f,0,.9f,1.4f};
            var bounds=plant.GetComponent<LODGroup>().GetLODs()[0].renderers[0].GetComponent<MeshFilter>().sharedMesh.bounds;
            for(float u=Mathf.Floor((l-20)/4)*4;u<r+20;u+=4)
            for(float z=Mathf.Ceil(b/1.25f)*1.25f;z+1.25f<=t;z+=1.25f)
            {
                for(int k=0;k<cross.Length-1;k++)
                {
                    var points=new Vector3[4];bool valid=true;
                    for(int j=0;j<4;j++)
                    {
                        float pz=z+(j==1||j==2?1.25f:0),across=cross[k+(j>=2?1:0)],px=u+FieldOffset(pz)+across;
                        if(!Within(px,pz)){valid=false;break;}
                        points[j]=new Vector3(px,Ground(px,pz)+BedHeight(px,pz,across)+.015f,pz);
                    }
                    if(!valid)continue;
                    float centreX=(points[0].x+points[1].x+points[2].x+points[3].x)*.25f;
                    if(context&&(centreX<l||centreX>=r))continue;
                    int first=geometry.vertices.Count;geometry.vertices.AddRange(points);
                    var normal=Vector3.Cross(points[1]-points[0],points[3]-points[0]).normalized;
                    for(int j=0;j<4;j++){geometry.normals.Add(normal);geometry.uv.Add(new Vector2(points[j].x,points[j].z)/9);}
                    geometry.triangles.AddRange(new[]{first,first+1,first+2,first,first+2,first+3});
                }
                if(Mathf.RoundToInt(z/1.25f)%2!=0)continue;
                float x=u+FieldOffset(z);
                if(!Within(x,z)||(context&&(x<l||x>=r))||(!context&&Edge(x,z)<2.5f))continue;
                float height=1.25f+.2f*Mathf.PerlinNoise(x*.05f+52,z*.05f+37);
                float yaw=Mathf.Repeat(x*137.508f+z*79.13f,360);
                var matrix=PlantMatrix(plant,x,z,height,yaw)*Matrix4x4.Scale(new Vector3(2.1f/height,1,2.1f/height));
                matrix.m13+=BedHeight(x,z,0);
                bool fits=true;
                for(int i=0;i<8;i++)
                {
                    var corner=matrix.MultiplyPoint3x4(new Vector3((i&1)==0?bounds.min.x:bounds.max.x,
                        (i&2)==0?bounds.min.y:bounds.max.y,(i&4)==0?bounds.min.z:bounds.max.z));
                    if(!Within(corner.x,corner.z)){fits=false;break;}
                }
                if(fits)plants.Add(matrix);
            }
            MeshObject("Cultivated soil beds",geometry,soil,parent);
            if(plants.Count>0)
            {
                var nature=parent.gameObject.AddComponent<InstancedNature>();
                // Keep LOD/culling bounds local instead of spanning an entire crop row.
                // The shared collector can still combine nearby groups after selection.
                foreach(var cell in plants.GroupBy(m=>new Vector2Int(Mathf.FloorToInt(m.m03/32),Mathf.FloorToInt(m.m23/32))))
                    nature.Add(plant,cell.ToList(),Color.white);
            }
        }
    }
}
