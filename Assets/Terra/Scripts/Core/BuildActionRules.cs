using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    // The full catalog is an authored placement experiment, not a calibrated economic model.
    public sealed class BuildActionRule
    {
        public string id, artId, title, category, description;
        public int cost;
        public float width, depth;
        public bool landscape;
    }

    public sealed class BuildActionLayout
    {
        public float x, z, yaw, groundY, relief;
        public Point[] footprint;
        public string rejection;
        public bool Valid => rejection == null;
    }

    public sealed class BuildActionRules
    {
        public static readonly string[] Categories = { "Farming", "Residential", "Community", "Utilities", "Nature" };
        public readonly BuildActionRule[] actions;
        readonly WorldData world;
        readonly HashSet<string> swaleSites;
        readonly Dictionary<string, BuildActionLayout> layouts = new Dictionary<string, BuildActionLayout>();
        public const float MaximumFoundationRelief = 3;

        public BuildActionRules(WorldData world, BuildActionRule[] actions)
        {
            this.world = world; this.actions = actions;
            swaleSites = new HashSet<string>(world.sites.Where(s => s.allowedTreatments.Contains("water-swale")).Select(s => s.id));
        }
        public BuildActionRule Find(string id) => actions.FirstOrDefault(a => a.id == id);
        public string Rejection(Site site, string id)
        {
            if (site.protectedSite) return site.name + " is a protected scenario zone. No construction or land changes are allowed.";
            var action = Find(id); if (action == null) return "Choose an action from the build catalog.";
            if (id == "water-swale" && !swaleSites.Contains(site.id))
                return "Swales are enabled only at Meadow edge and Brye-side field in this scenario. This is an authored connection rule, not a terrain calculation.";
            return Layout(site, action).rejection;
        }
        public BuildActionLayout Layout(Site site, BuildActionRule action)
        {
            string key = site.id + ":" + action.id;
            if (layouts.TryGetValue(key, out var cached)) return cached;
            float minX=site.polygon.Min(p=>p.x),maxX=site.polygon.Max(p=>p.x),minZ=site.polygon.Min(p=>p.z),maxZ=site.polygon.Max(p=>p.z);
            BuildActionLayout best=null;
            // Axis-aligned and quarter-turn options keep fronts legible; never rescale a structure.
            foreach(float yaw in new[]{180f,90f})
            {
                float w=yaw==180?action.width:action.depth,d=yaw==180?action.depth:action.width;
                foreach(float u in new[]{.5f,.25f,.75f})foreach(float v in new[]{.5f,.25f,.75f})
                {
                    float x=minX+w/2+(maxX-minX-w)*u,z=minZ+d/2+(maxZ-minZ-d)*v;
                    var ring=new[]{new Point(x-w/2,z-d/2),new Point(x+w/2,z-d/2),new Point(x+w/2,z+d/2),new Point(x-w/2,z+d/2)};
                    if(ring.Any(p=>!Contains(site.polygon,p.x,p.z)))continue;
                    float low=float.MaxValue,high=float.MinValue;
                    // Include terrain-grid corners and both rectangle edges; the surface is piecewise linear.
                    var xs=Samples(x-w/2,x+w/2,world.terrain.xMin,world.terrain.cellSizeM);
                    var zs=Samples(z-d/2,z+d/2,world.terrain.zMin,world.terrain.cellSizeM);
                    foreach(float px in xs)foreach(float pz in zs){float h=Height(px,pz);low=Math.Min(low,h);high=Math.Max(high,h);}
                    // A grid cell is split on x+z=constant. Include where those diagonals
                    // intersect footprint edges, not just grid corners (which can miss a peak).
                    var terrain=world.terrain;float step=terrain.cellSizeM,origin=terrain.xMin+terrain.zMin;
                    float first=origin+(float)Math.Ceiling((x-w/2+z-d/2-origin)/step)*step;
                    for(float diagonal=first;diagonal<x+w/2+z+d/2;diagonal+=step)
                    {
                        foreach(float px in new[]{x-w/2,x+w/2}){float pz=diagonal-px;if(pz<z-d/2||pz>z+d/2)continue;float h=Height(px,pz);low=Math.Min(low,h);high=Math.Max(high,h);}
                        foreach(float pz in new[]{z-d/2,z+d/2}){float px=diagonal-pz;if(px<x-w/2||px>x+w/2)continue;float h=Height(px,pz);low=Math.Min(low,h);high=Math.Max(high,h);}
                    }
                    var layout=new BuildActionLayout{x=x,z=z,yaw=yaw,footprint=ring,groundY=high+.04f,relief=high-low};
                    // Terrain-following samples prefer the first central fit; only rigid pads need the flattest candidate.
                    if(best==null||(!action.landscape&&layout.relief<best.relief))best=layout;
                }
            }
            if(best==null)best=new BuildActionLayout{rejection=$"Needs a {action.width:0} × {action.depth:0} m footprint. This parcel is too small; models keep their real scale."};
            else if(!action.landscape&&best.relief>MaximumFoundationRelief)
                best.rejection=$"The best fitting footprint crosses {best.relief:0.0} m of elevation. This prototype allows at most {MaximumFoundationRelief:0} m of foundation fill. Choose a flatter field.";
            layouts[key]=best;return best;
        }
        static float[] Samples(float min,float max,float origin,float step)
        {
            var values=new List<float>{min,max};for(float n=origin+(float)Math.Ceiling((min-origin)/step)*step;n<max;n+=step)if(n>min)values.Add(n);
            return values.ToArray();
        }
        public static bool Contains(Point[] polygon,float x,float z)
        {
            // WorldPack v1 validates convex, counterclockwise site polygons. For convex rings,
            // containment of every footprint corner also contains each complete footprint edge.
            for(int i=0;i<polygon.Length;i++){var a=polygon[i];var b=polygon[(i+1)%polygon.Length];if((b.x-a.x)*(z-a.z)-(b.z-a.z)*(x-a.x)<-.001f)return false;}
            return true;
        }
        public float Height(float x,float z)
        {
            var t=world.terrain;float fx=Math.Max(0,Math.Min(t.columns-1,(x-t.xMin)/t.cellSizeM)),fz=Math.Max(0,Math.Min(t.rows-1,(z-t.zMin)/t.cellSizeM));
            int ix=Math.Min((int)fx,t.columns-2),iz=Math.Min((int)fz,t.rows-2),n=iz*t.columns+ix;float u=fx-ix,v=fz-iz;
            float a=t.heights[n],b=t.heights[n+1],c=t.heights[n+t.columns],d=t.heights[n+t.columns+1];
            return u+v<=1?a+(b-a)*u+(c-a)*v:d+(c-d)*(1-u)+(b-d)*(1-v);
        }
    }
}
