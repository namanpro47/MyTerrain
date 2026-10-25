using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        const int TreatmentResolution=512;
        readonly Dictionary<string,string> surfaceTreatments=new Dictionary<string,string>();
        readonly Color32[] surfacePixels=new Color32[TreatmentResolution*TreatmentResolution];
        Texture2D treatmentSurface;
        string ghostTreatment;
        bool surfaceDirty;

        void CreateTreatmentSurface(Material material)
        {
            treatmentSurface=new Texture2D(TreatmentResolution,TreatmentResolution,TextureFormat.RGBA32,true,true)
            {name="Illustrative treatment surface",wrapMode=TextureWrapMode.Clamp,filterMode=FilterMode.Trilinear};
            treatmentSurface.SetPixels32(surfacePixels);treatmentSurface.Apply();
            material.SetTexture("_TreatmentMap",treatmentSurface);
        }

        // One upload after input/state changes, including a whole-history undo, rather than per site.
        // This changes only presentation. CoverAt, source polygons and the gameplay ledger are untouched.
        void FlushTreatmentSurface()
        {
            if(!surfaceDirty||treatmentSurface==null)return;
            Array.Clear(surfacePixels,0,surfacePixels.Length);
            foreach(var site in world.sites)
            {
                string treatment;
                if(site.id==ghostSiteId&&ghostTreatment!=null)treatment=ghostTreatment;
                else if(!surfaceTreatments.TryGetValue(site.id,out treatment))continue;
                PaintTreatmentSurface(site,treatment);
            }
            treatmentSurface.SetPixels32(surfacePixels);treatmentSurface.Apply();FlushSwaleGround();surfaceDirty=false;
        }

        void PaintTreatmentSurface(Site site,string treatment)
        {
            // Buildings retain source cover outside their bounded occupied footprint.
            // Keep the ghost ID (even for a building) so it suppresses a prior land preview.
            if(string.IsNullOrEmpty(treatment)||(buildRules!=null&&buildRules.Find(treatment)?.landscape!=true))return;
            var polygon=site.polygon;
            float left=polygon.Min(p=>p.x),right=polygon.Max(p=>p.x),bottom=polygon.Min(p=>p.z),top=polygon.Max(p=>p.z);
            float dx=(xmax-xmin)/TreatmentResolution,dz=(zmax-zmin)/TreatmentResolution;
            int x0=Mathf.Clamp(Mathf.FloorToInt((left-xmin)/dx),0,TreatmentResolution-1),x1=Mathf.Clamp(Mathf.CeilToInt((right-xmin)/dx),0,TreatmentResolution-1);
            int z0=Mathf.Clamp(Mathf.FloorToInt((bottom-zmin)/dz),0,TreatmentResolution-1),z1=Mathf.Clamp(Mathf.CeilToInt((top-zmin)/dz),0,TreatmentResolution-1);
            bool rotation=treatment=="conservation-rotation";
            bool cultivated=treatment.StartsWith("cultivate")||rotation,margin=treatment=="cultivate-margin";
            float marginTop=Mathf.Lerp(bottom,top,.25f);
            var swale=treatment=="water-swale"?SwaleFor(site):null;
            for(int z=z0;z<=z1;z++)for(int x=x0;x<=x1;x++)
            {
                float px=xmin+(x+.5f)*dx,pz=zmin+(z+.5f)*dz;
                if(!Contains(polygon,px,pz))continue;
                float edge=float.MaxValue;
                for(int i=0;i<polygon.Length;i++)
                {
                    var a=new Vector2(polygon[i].x,polygon[i].z);var b=new Vector2(polygon[(i+1)%polygon.Length].x,polygon[(i+1)%polygon.Length].z);
                    var segment=b-a;float t=segment.sqrMagnitude>0?Mathf.Clamp01(Vector2.Dot(new Vector2(px,pz)-a,segment)/segment.sqrMagnitude):0;
                    edge=Mathf.Min(edge,Vector2.Distance(new Vector2(px,pz),a+t*segment));
                }
                // Blend inside the footprint only; the selectable boundary remains exact and visible.
                // Texture filtering adds a sub-texel visual transition, never analytical area.
                float coverage=site.sourceId=="scenario-connected-fields-v2"?1:Mathf.SmoothStep(0,1,Mathf.Clamp01(edge/3f));
                float cropWeight=cultivated?1:0;
                if(margin)cropWeight=Mathf.SmoothStep(0,1,Mathf.InverseLerp(marginTop-1,marginTop+1,pz));
                if(rotation)
                {
                    // Match the three clipped planting bands in BuildAnnualLand.
                    float bandHeight=(top-bottom)/3f;
                    int band=Mathf.Min(2,Mathf.FloorToInt((pz-bottom)/bandHeight));
                    float split=bottom+(band+.3f)*bandHeight;
                    cropWeight=Mathf.SmoothStep(0,1,Mathf.InverseLerp(split-1,split+1,pz));
                }
                surfacePixels[z*TreatmentResolution+x]=new Color(cropWeight,1-cropWeight,swale==null?0:SwaleCut(swale,px,pz)/SwaleDepth,coverage);
            }
        }
    }
}
