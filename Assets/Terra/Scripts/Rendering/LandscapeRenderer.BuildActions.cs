using System.Collections.Generic;
using Terra.Art;
using Terra.Core;
using Terra.Contracts;
using UnityEngine;
using Object=UnityEngine.Object;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        BuildActionCatalog buildCatalog;
        BuildActionRules buildRules;
        Material projectGrassMaterial,projectPathMaterial;
        public void ConfigureBuildActions(BuildActionCatalog catalog,BuildActionRules rules){buildCatalog=catalog;buildRules=rules;}

        // Rules cache analytical layouts. Derive a separate presentation object so
        // placing or previewing a model can never rewrite suitability or source relief.
        public BuildActionLayout VisualLayout(BuildActionLayout source)
        {
            if(!clusteredWoodland||source==null||!source.Valid)return source;
            float left=source.foundation[0].x,right=source.foundation[2].x,bottom=source.foundation[0].z,top=source.foundation[2].z;
            float dx=displayDx,dz=displayDz;
            var xs=new List<float>{left,right};var zs=new List<float>{bottom,top};
            for(float x=xmin+Mathf.Ceil((left-xmin)/dx)*dx;x<right;x+=dx)if(x>left)xs.Add(x);
            for(float z=zmin+Mathf.Ceil((bottom-zmin)/dz)*dz;z<top;z+=dz)if(z>bottom)zs.Add(z);
            float high=float.MinValue,low=float.MaxValue;
            void Sample(float x,float z){float h=Height(x,z);high=Mathf.Max(high,h);low=Mathf.Min(low,h);}
            foreach(float x in xs)foreach(float z in zs)Sample(x,z);
            // Include intersections of displayed mesh diagonals with foundation edges.
            foreach(float x in new[]{left,right})
            {
                float gx=(x-xmin)/dx,u=gx-Mathf.Floor(gx);
                for(int iz=Mathf.FloorToInt((bottom-zmin)/dz);iz<=Mathf.FloorToInt((top-zmin)/dz);iz++)
                {float z=zmin+(iz+1-u)*dz;if(z>=bottom&&z<=top)Sample(x,z);}
            }
            foreach(float z in new[]{bottom,top})
            {
                float gz=(z-zmin)/dz,v=gz-Mathf.Floor(gz);
                for(int ix=Mathf.FloorToInt((left-xmin)/dx);ix<=Mathf.FloorToInt((right-xmin)/dx);ix++)
                {float x=xmin+(ix+1-v)*dx;if(x>=left&&x<=right)Sample(x,z);}
            }
            return new BuildActionLayout{x=source.x,z=source.z,yaw=source.yaw,groundY=high+.04f,relief=high-low,
                footprint=source.footprint,foundation=source.foundation,rejection=source.rejection};
        }

        Material ProjectGround(bool path)
        {
            var material=path?projectPathMaterial:projectGrassMaterial;
            if(material!=null)return material;
            material=new Material(Resources.Load<Shader>("TerraProjectGround")){name=path?"Project earth paths":"Project meadow ground"};
            material.SetFloat("_GroundKind",path?1:0);
            material.SetTexture("_MeadowPaint",Resources.Load<Texture2D>("JulioGround/MeadowColor"));
            material.SetTexture("_MeadowRelief",Resources.Load<Texture2D>("JulioGround/MeadowNormal"));
            material.SetTexture("_EarthPaint",Resources.Load<Texture2D>("JulioGround/EarthColor"));
            material.SetTexture("_EarthRelief",Resources.Load<Texture2D>("JulioGround/EarthNormal"));
            if(path)projectPathMaterial=material;else projectGrassMaterial=material;
            return material;
        }

        void AdaptProjectGround(GameObject instance)
        {
            // Catalog ground parts share the landscape's licensed paint family.
            // Assign instance renderers only; metric prefab meshes and source materials stay intact.
            foreach(var renderer in instance.GetComponentsInChildren<MeshRenderer>())
                if(renderer.name=="grass"||renderer.name=="path")renderer.sharedMaterial=ProjectGround(renderer.name=="path");
        }

        bool BuildCatalogAction(Site site,string id,Transform parent,bool preview,float vegetationMaturity=1)
        {
            var action=buildRules?.Find(id);if(action==null)return false;
            if(action.landscape){BuildAnnualLand(site,id,parent,preview,vegetationMaturity);return true;}
            var entry=buildCatalog.Find(action.artId);var layout=VisualLayout(buildRules.Layout(site,action));
            if(entry?.prefab==null||!layout.Valid)return false;
            if(id=="wind"){if(!preview)BuildWindMeadow(site,layout,parent);}
            else if(site.protectedSite||!baselines.ContainsKey(site.id))BuildRemainingSite(site,layout,parent);
            // Rigid projects reuse the site's source vegetation outside the occupied lot.
            // Replanting that whole field on every hover was the main selection stall.
            var instance=Object.Instantiate(entry.prefab,parent);instance.name=entry.title+(preview?" preview":"");
            instance.transform.localPosition=new Vector3(layout.x,action.landscape?0:layout.groundY,layout.z);
            instance.transform.localRotation=Quaternion.Euler(0,layout.yaw,0);instance.transform.localScale=Vector3.one;
            AdaptProjectGround(instance);
            if(id!="wind")GradeProjectEdges(instance,layout);
            if(id=="wind")
            {
                var oldPath=instance.transform.Find("path");if(oldPath!=null)oldPath.gameObject.SetActive(false);
                var pad=new Geometry();var corners=layout.foundation;
                Wall(pad,new Vector3(corners[0].x,layout.groundY+.07f,corners[0].z),new Vector3(corners[3].x,layout.groundY+.07f,corners[3].z),new Vector3(corners[2].x,layout.groundY+.07f,corners[2].z),new Vector3(corners[1].x,layout.groundY+.07f,corners[1].z));
                MeshObject("Wind pad",pad,ProjectGround(true),parent);
            }
            {
                // A level foundation with terrain-following sides fills the bounded gap. Existing
                // analytical terrain stays intact; the rules reject >3 m source relief and protected land.
                var geometry=new Geometry();var ring=layout.foundation;
                for(int edge=0;edge<4;edge++)
                {
                    var a=ring[edge];var b=ring[(edge+1)%4];int steps=Mathf.CeilToInt(Vector2.Distance(new Vector2(a.x,a.z),new Vector2(b.x,b.z))/2);
                    for(int j=0;j<steps;j++)
                    {
                        var p=Vector2.Lerp(new Vector2(a.x,a.z),new Vector2(b.x,b.z),j/(float)steps);var q=Vector2.Lerp(new Vector2(a.x,a.z),new Vector2(b.x,b.z),(j+1)/(float)steps);int n=geometry.vertices.Count;
                        geometry.vertices.AddRange(new[]{new Vector3(p.x,Height(p.x,p.y)-.04f,p.y),new Vector3(q.x,Height(q.x,q.y)-.04f,q.y),new Vector3(q.x,layout.groundY+.06f,q.y),new Vector3(p.x,layout.groundY+.06f,p.y)});
                        var normal=Vector3.Cross(Vector3.up,geometry.vertices[n+1]-geometry.vertices[n]).normalized;for(int k=0;k<4;k++)geometry.normals.Add(normal);
                        geometry.triangles.AddRange(new[]{n,n+2,n+1,n,n+3,n+2});
                    }
                }
                MeshObject("Level foundation",geometry,Mat(new Color(.52f,.48f,.38f)),parent);
                if(id!="wind")parent.Find("Level foundation").gameObject.SetActive(false); // Kept for construction.
            }
            if(preview)
            {
                var go=new GameObject("Actual occupied footprint");go.transform.SetParent(parent,false);var line=go.AddComponent<LineRenderer>();line.sharedMaterial=BoundaryMat(new Color(1,.84f,.36f));line.useWorldSpace=true;line.loop=true;line.positionCount=4;line.startWidth=line.endWidth=.5f;
                for(int i=0;i<4;i++){var p=layout.footprint[i];line.SetPosition(i,new Vector3(p.x,action.landscape?Height(p.x,p.z)+.4f:layout.groundY+.3f,p.z));}
            }
            return true;
        }
    }
}
