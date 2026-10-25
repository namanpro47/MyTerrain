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
                QueuedMiniature(label.transform,treatment);
            }
        }

        void QueuedMiniature(Transform label,string treatment)
        {
            var rule=buildRules.Find(treatment);
            var icon=buildCatalog?.Find(rule?.artId??treatment)?.icon;
            if(icon==null)return; // The named boundary remains useful without an art entry.
            var existing=label.Find("Queued project pin");
            if(existing!=null)
            {
                existing.Find("Project miniature").GetComponent<UnityEngine.UI.Image>().sprite=icon;
                return;
            }
            // Inherit the status label's camera-facing rotation and CSS-pixel scale.
            // No raycaster: this is an annotation, never another map input surface.
            var go=new GameObject("Queued project pin",typeof(RectTransform),typeof(Canvas));
            var rect=(RectTransform)go.transform;rect.SetParent(label,false);
            rect.sizeDelta=new Vector2(38,32);rect.localPosition=new Vector3(0,30,-.05f);
            var canvas=go.GetComponent<Canvas>();canvas.renderMode=RenderMode.WorldSpace;
            canvas.worldCamera=Camera.main;canvas.overrideSorting=true;canvas.sortingOrder=6;
            PinImage(rect,"Pin backing",Vector2.zero,new Vector2(38,32),new Color(.04f,.10f,.13f,.94f));
            var miniature=PinImage(rect,"Project miniature",new Vector2(-2,1),new Vector2(32,26),Color.white);
            miniature.sprite=icon;miniature.preserveAspect=true;
            PinImage(rect,"Plan check backing",new Vector2(13,-10),new Vector2(14,14),new Color(.04f,.10f,.13f,1));
            var shortStroke=PinImage(rect,"Plan check short",new Vector2(10,-10),new Vector2(6,2),new Color(.98f,.81f,.42f));
            shortStroke.rectTransform.localRotation=Quaternion.Euler(0,0,-45);
            var longStroke=PinImage(rect,"Plan check long",new Vector2(15,-8),new Vector2(10,2),new Color(.98f,.81f,.42f));
            longStroke.rectTransform.localRotation=Quaternion.Euler(0,0,45);
        }

        static UnityEngine.UI.Image PinImage(Transform parent,string name,Vector2 position,Vector2 size,Color color)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(UnityEngine.UI.Image));
            var rect=(RectTransform)go.transform;rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);
            rect.anchoredPosition=position;rect.sizeDelta=size;
            var image=go.GetComponent<UnityEngine.UI.Image>();image.color=color;image.raycastTarget=false;
            return image;
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
