using System;
using System.Collections.Generic;
using System.Linq;
namespace Terra.Contracts
{
    public static class PackValidator
    {
        static void Need(bool value,string message){if(!value)throw new ArgumentException(message);}
        static bool Finite(float v)=>!float.IsNaN(v)&&!float.IsInfinity(v);
        public static void Validate(Manifest m, WorldData w, Scenario s)
        {
            Need(m!=null&&m.schemaVersion==1,"Unsupported WorldPack schema. Expected 1.");
            Need(s!=null&&s.schemaVersion==1,"Unsupported scenario schema. Expected 1.");
            Need(!string.IsNullOrWhiteSpace(m.packId)&&s.packId==m.packId,"Scenario packId does not match.");
            Need(m.worldDataPath=="world.json"&&m.scenarioPath=="scenario.json","v1 requires world.json and scenario.json.");
            Need(m.contextBounds?.Length==4&&m.editableBounds?.Length==4,"Bounds require [xmin,zmin,xmax,zmax].");
            Need(m.contextBounds.All(Finite)&&m.editableBounds.All(Finite),"Bounds must be finite.");
            Need(m.contextBounds[0]<m.contextBounds[2]&&m.contextBounds[1]<m.contextBounds[3]&&m.editableBounds[0]<m.editableBounds[2]&&m.editableBounds[1]<m.editableBounds[3],"Bounds must have positive area.");
            Need(m.editableBounds[0]>=m.contextBounds[0]&&m.editableBounds[1]>=m.contextBounds[1]&&m.editableBounds[2]<=m.contextBounds[2]&&m.editableBounds[3]<=m.contextBounds[3],"Editable bounds must fit context.");
            Need(m.sources!=null&&m.sources.Length>0&&m.layers!=null,"Manifest sources/layers missing.");
            var sources=new HashSet<string>();foreach(var source in m.sources)Need(source!=null&&!string.IsNullOrEmpty(source.id)&&sources.Add(source.id)&&!string.IsNullOrEmpty(source.license),"Source IDs/license must be present and unique.");
            Need(m.geographyKind=="synthetic-development-fixture"||m.geographyKind=="real-geography","Unknown geography kind.");
            if(m.geographyKind=="real-geography")Need(m.boundsWgs84?.Length==4&&m.originWgs84?.Length==2&&!string.IsNullOrEmpty(m.sourceCrs)&&!string.IsNullOrEmpty(m.elevationDatum),"Real geography requires geographic bounds, origin, CRS and datum.");
            Need(m.rasterRows=="south-to-north"&&m.winding=="counterclockwise-xz","Unsupported axis convention.");
            Need(w?.terrain!=null&&w.sites!=null&&w.covers!=null&&w.features!=null,"Required world layers are missing.");
            var t=w.terrain;
            Need(t.columns>=2&&t.rows>=2&&t.columns<=257&&t.rows<=257&&Finite(t.cellSizeM)&&t.cellSizeM>0,"Invalid terrain dimensions.");
            Need(t.heights?.Length==t.columns*t.rows&&t.heights.All(Finite),"Terrain heights are missing or invalid.");
            Need(Math.Abs(t.xMin-m.contextBounds[0])<.01&&Math.Abs(t.zMin-m.contextBounds[1])<.01&&Math.Abs(t.xMin+(t.columns-1)*t.cellSizeM-m.contextBounds[2])<.01&&Math.Abs(t.zMin+(t.rows-1)*t.cellSizeM-m.contextBounds[3])<.01,"Terrain must span context bounds.");
            foreach(var cover in w.covers){Need(cover!=null&&sources.Contains(cover.sourceId),"Unknown cover source.");PolygonArea(cover.polygon,m.contextBounds);}
            foreach(var feature in w.features)
            {Need(feature!=null&&sources.Contains(feature.sourceId)&&feature.points!=null&&feature.points.Length>0&&Finite(feature.widthM)&&feature.widthM>=0,"Invalid feature/source.");foreach(var p in feature.points)Need(p!=null&&Finite(p.x)&&Finite(p.z)&&p.x>=m.contextBounds[0]&&p.x<=m.contextBounds[2]&&p.z>=m.contextBounds[1]&&p.z<=m.contextBounds[3],"Feature coordinate outside context.");}
            Need(s.storm!=null&&Finite(s.storm.rainfallMm)&&s.storm.rainfallMm>=0&&s.storm.initialAbstractionRatio==.2f&&s.objectives!=null,"Missing/invalid storm or objectives.");
            Need(w.sites.Length>0&&w.sites.Length<=32,"Expected 1–32 management sites.");
            Need(s.budget>=0&&s.treatments!=null&&s.treatments.Length>0,"Invalid scenario budget/treatments.");
            var ids=new HashSet<string>(); foreach(var tr in s.treatments){Need(!string.IsNullOrEmpty(tr.id)&&ids.Add(tr.id),"Duplicate/missing treatment ID."); Fractions(tr.cover);Need(Finite(tr.productiveFraction)&&tr.productiveFraction>=0&&tr.productiveFraction<=1,"Invalid productive fraction.");}
            var siteIds=new HashSet<string>();
            foreach(var site in w.sites)
            {
                Need(!string.IsNullOrEmpty(site.id)&&siteIds.Add(site.id),"Duplicate/missing site ID.");
                Need(sources.Contains(site.sourceId),"Unknown site source.");
                Need(site.allowedTreatments!=null&&site.offers!=null,"Missing site offers.");
                float area=PolygonArea(site.polygon,m.editableBounds);
                Need(Finite(site.analysisAreaM2)&&Math.Abs(area-site.analysisAreaM2)<.1f,"Site analysis area disagrees with polygon.");
                Need(Finite(site.baselineProduction)&&site.baselineProduction>=0,"Invalid baseline production.");Fractions(site.baselineCover);
                Need(!string.IsNullOrEmpty(site.contributingAreaId)&&!string.IsNullOrEmpty(site.modelParameterId),"Missing site model/connection ID.");
                Need(site.allowedTreatments.Distinct().Count()==site.allowedTreatments.Length,"Duplicate allowed treatment.");
                Need(site.offers.Length==site.allowedTreatments.Length,"Offer count does not match allowed treatments.");
                foreach(var id in site.allowedTreatments)Need(ids.Contains(id)&&site.offers.Count(x=>x.treatmentId==id)==1,"Unknown treatment or missing/duplicate offer.");
                foreach(var o in site.offers)Need(o.cost>=0&&Finite(o.production)&&o.production>=0&&Finite(o.retentionCapacityM3)&&o.retentionCapacityM3>=0,"Invalid site offer values.");
                Need(!site.protectedSite||(!string.IsNullOrEmpty(site.protectionReason)&&site.offers.Length==0),"Protected sites require a reason and no offers.");
            }
            for(int i=0;i<w.sites.Length;i++)for(int j=i+1;j<w.sites.Length;j++)Need(!Overlaps(w.sites[i].polygon,w.sites[j].polygon),"Management sites overlap.");
        }
        static bool Overlaps(Point[] a,Point[] b)
        {
            foreach(var ring in new[]{a,b})for(int i=0;i<ring.Length;i++)
            {
                var p=ring[i];var q=ring[(i+1)%ring.Length];float nx=-(q.z-p.z),nz=q.x-p.x;
                float amin=a.Min(v=>v.x*nx+v.z*nz),amax=a.Max(v=>v.x*nx+v.z*nz),bmin=b.Min(v=>v.x*nx+v.z*nz),bmax=b.Max(v=>v.x*nx+v.z*nz);
                if(amax<=bmin+.0001f||bmax<=amin+.0001f)return false;
            }
            return true;
        }
        public static float PolygonArea(Point[] p,float[] bounds)
        {
            Need(p!=null&&p.Length>=3&&p.Length<=64,"Polygon needs 3–64 points.");float area=0,sign=0;
            Need(p.All(a=>a!=null&&Finite(a.x)&&Finite(a.z)),"Polygon coordinates must be finite.");
            for(int i=0;i<p.Length;i++)
            {
                var a=p[i];var b=p[(i+1)%p.Length];var c=p[(i+2)%p.Length];
                Need(a!=null&&Finite(a.x)&&Finite(a.z),"Polygon coordinates must be finite.");
                Need(a.x>=bounds[0]&&a.x<=bounds[2]&&a.z>=bounds[1]&&a.z<=bounds[3],"Site is outside editable bounds.");
                area+=a.x*b.z-b.x*a.z;
                float cross=(b.x-a.x)*(c.z-b.z)-(b.z-a.z)*(c.x-b.x);
                if(Math.Abs(cross)>.0001f){if(sign==0)sign=cross;Need(sign*cross>0,"v1 polygons must be convex and simple.");}
            }
            Need(area>0,"Polygon must be counterclockwise and nonzero.");return area*.5f;
        }
        static void Fractions(CoverFraction[] values)
        {
            Need(values!=null&&values.Length>0,"Cover fractions missing.");var ids=new HashSet<string>();float sum=0;
            foreach(var v in values){Need(v!=null&&!string.IsNullOrEmpty(v.coverId)&&ids.Add(v.coverId)&&Finite(v.fraction)&&v.fraction>=0&&v.fraction<=1,"Invalid/duplicate cover fraction.");sum+=v.fraction;}
            Need(Math.Abs(sum-1)<.0001f,"Cover fractions must sum to one.");
        }
    }
}
