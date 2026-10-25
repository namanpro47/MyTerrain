using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;
using UnityEngine;
using UnityEngine.Rendering;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        readonly Dictionary<string,Transform> queuedBoundaries=new Dictionary<string,Transform>();
        readonly List<LineRenderer> placementLines=new List<LineRenderer>();
        Material previewBoundary,queuedBoundary,blockedBoundary;

        public Vector3[] PlacementWorldPoints(Site site,string treatment)
        {
            var rule=buildRules?.Find(treatment);
            var layout=rule==null||rule.landscape?null:buildRules.Layout(site,rule);
            var ring=layout?.footprint??site.polygon;
            return ring.Select(p=>new Vector3(p.x,layout?.Valid==true?layout.groundY+.5f:Height(p.x,p.z)+.5f,p.z)).ToArray();
        }

        Material ProposalMaterial(bool valid,bool queued)
        {
            var material=!valid?blockedBoundary:queued?queuedBoundary:previewBoundary;
            if(material!=null)return material;
            material=new Material(Resources.Load<Shader>("TerraProposalBoundary"));
            material.SetColor("_BaseColor",!valid?new Color(1,.53f,.34f):queued?new Color(.98f,.81f,.42f):new Color(.73f,1,.94f));
            if(!valid)blockedBoundary=material;else if(queued)queuedBoundary=material;else previewBoundary=material;
            return material;
        }

        void BuildPlacementBoundary(Site site,string treatment,Transform parent,bool valid,bool queued)
        {
            var points=PlacementWorldPoints(site,treatment);
            var go=new GameObject(queued?"Queued footprint":"Preview footprint");go.transform.SetParent(parent,false);
            var line=go.AddComponent<LineRenderer>();line.useWorldSpace=true;line.loop=true;line.positionCount=points.Length;line.SetPositions(points);
            line.sharedMaterial=ProposalMaterial(valid,queued);line.textureMode=LineTextureMode.Tile;line.shadowCastingMode=ShadowCastingMode.Off;
            line.startWidth=line.endWidth=3.2f*boundaryScale;placementLines.Add(line);
            if(!valid&&points.Length>=4)
            {
                for(int i=0;i<2;i++)
                {
                    var cross=new GameObject("Blocked cross");cross.transform.SetParent(parent,false);
                    var stroke=cross.AddComponent<LineRenderer>();stroke.useWorldSpace=true;stroke.positionCount=2;
                    stroke.SetPositions(new[]{points[i],points[i+2]});stroke.sharedMaterial=ProposalMaterial(false,false);
                    stroke.textureMode=LineTextureMode.Tile;stroke.shadowCastingMode=ShadowCastingMode.Off;
                    stroke.startWidth=stroke.endWidth=3.2f*boundaryScale;placementLines.Add(stroke);
                }
            }
        }

        void TintPlacementGhost(Transform group)
        {
            var tint=new Color(.74f,.90f,.89f);
            foreach(var renderer in group.GetComponentsInChildren<Renderer>())
            {
                if(renderer is LineRenderer){renderer.enabled=false;continue;} // Replace the old solid footprint.
                var block=new MaterialPropertyBlock();renderer.GetPropertyBlock(block);block.SetColor("_BaseColor",tint);block.SetColor("_Color",tint);renderer.SetPropertyBlock(block);
            }
            foreach(var nature in group.GetComponentsInChildren<InstancedNature>())nature.SetTint(tint,.88f);
        }

        public void MarkQueued(Site site,string treatment)
        {
            if(queuedBoundaries.TryGetValue(site.id,out var previous)){DestroyGroup(previous);queuedBoundaries.Remove(site.id);}
            if(treatment==null)return;
            var holder=new GameObject("Queued site "+site.id).transform;holder.SetParent(root,false);queuedBoundaries[site.id]=holder;
            BuildPlacementBoundary(site,treatment,holder,true,true);
            if(annualLabels.TryGetValue(site.id,out var label)&&label!=null)
            {
                label.text="Queued · "+(buildRules.Find(treatment)?.title??treatment);
                label.fontSize=120;label.rectTransform.sizeDelta=new Vector2(1400,180);
            }
        }

        void ScalePlacementBoundaries()
        {
            placementLines.RemoveAll(line=>line==null);
            foreach(var line in placementLines)line.startWidth=line.endWidth=3.2f*boundaryScale;
            foreach(var material in new[]{previewBoundary,queuedBoundary,blockedBoundary})
                if(material!=null)material.SetFloat("_DashLength",Mathf.Max(.2f,boundaryScale*12));
        }
        void DisposePlacementMaterials()
        {
            foreach(var material in new[]{previewBoundary,queuedBoundary,blockedBoundary})if(material!=null)Object.Destroy(material);
        }
    }
}
