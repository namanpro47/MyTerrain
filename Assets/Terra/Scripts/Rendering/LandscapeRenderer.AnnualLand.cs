using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;
using UnityEngine;

namespace Terra.Rendering
{
    public sealed partial class LandscapeRenderer
    {
        // Peter's landscape icons illustrate practices, while the live agreement covers the
        // complete managed polygon. Reuse the environment's own planting and terrain surfaces.
        void BuildAnnualLand(Site site,string id,Transform parent,bool preview)
        {
            var p=site.polygon;float l=p.Min(v=>v.x),r=p.Max(v=>v.x),b=p.Min(v=>v.z),t=p.Max(v=>v.z);
            if(id=="cultivate-standard")PlantCultivation(p,parent,false);
            else if(id=="cultivate-margin"||id=="conservation-rotation")
            {
                int bands=id=="conservation-rotation"?3:1;
                for(int i=0;i<bands;i++)
                {
                    float bottom=Mathf.Lerp(b,t,i/(float)bands),top=Mathf.Lerp(b,t,(i+1f)/bands);
                    float split=Mathf.Lerp(bottom,top,id=="conservation-rotation"?.3f:.25f);
                    var meadow=LandBand(p,bottom,split);var crops=LandBand(p,split,top);
                    if(meadow.Length>=3){PlantMeadow(p,parent,3.1f,true,(x,z)=>z<bottom||z>split);}
                    if(crops.Length>=3)PlantCultivation(crops,parent,false);
                }
            }
            else if(id=="water-swale")PlantSwale(site,parent);
            else PlantMeadow(p,parent,id=="cover-crop"?2.8f:2.9f,id!="cover-crop");
        }
        void BuildWindMeadow(Site site,Terra.Core.BuildActionLayout layout,Transform parent)
        {
            var ground=new GameObject("Wind agreement meadow").transform;ground.SetParent(parent,false);
            PlantMeadow(site.polygon,ground,4.5f,false,(x,z)=>Mathf.Abs(x-layout.x)<11&&Mathf.Abs(z-layout.z)<11);
        }
        void BuildRemainingSite(Site site,Terra.Core.BuildActionLayout layout,Transform parent)
        {
            // Source plants outside the occupied lot remain. The financial model preserves that
            // same unoccupied baseline; no vegetation is placed through the new structure.
            float l=layout.footprint.Min(p=>p.x)-1,r=layout.footprint.Max(p=>p.x)+1;
            float b=layout.footprint.Min(p=>p.z)-1,t=layout.footprint.Max(p=>p.z)+1;
            var pieces=new[]{LandBand(site.polygon,float.MinValue,b),LandBand(site.polygon,t,float.MaxValue),
                LandBand(LandBand(site.polygon,b,t),float.MinValue,l,true),LandBand(LandBand(site.polygon,b,t),r,float.MaxValue,true)};
            foreach(var polygon in pieces.Where(p=>p.Length>=3))
            {
                PlantCultivation(polygon,parent,true);
                if(site.baselineCover.Any(c=>c.coverId=="grass"&&c.fraction>.5f))PlantMeadow(polygon,parent,7,false);
            }
        }
        static Point[] LandBand(Point[] polygon,float bottom,float top,bool xAxis=false)
        {
            var points=polygon.ToList();
            foreach(bool lower in new[]{true,false})
            {
                if(points.Count==0)break;var output=new List<Point>();var previous=points[points.Count-1];
                float Distance(Point p){float axis=xAxis?p.x:p.z;return lower?axis-bottom:top-axis;}float pd=Distance(previous);
                foreach(var current in points)
                {
                    float cd=Distance(current);
                    if((cd>=0)!=(pd>=0)){float f=pd/(pd-cd);output.Add(new Point(Mathf.Lerp(previous.x,current.x,f),Mathf.Lerp(previous.z,current.z,f)));}
                    if(cd>=0)output.Add(current);previous=current;pd=cd;
                }
                points=output;
            }
            return points.ToArray();
        }
    }
}
