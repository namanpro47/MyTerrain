using System;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    // Same AOI, additional fictional opportunity. Source cover clipped from canonical NLCD polygons.
    // This is neither a surveyed parcel nor legal/engineering permission to build a turbine.
    public static class AnnualSpatialOpportunities
    {
        public static bool Connected(WorldData world)=>world.sites.Any(s=>s.sourceId=="scenario-connected-fields-v2");

        // Versioned scenario override; observed terrain/cover and the old pack stay intact.
        // Take each expanded area out of context exactly once, by its actual source cover.
        public static void ConnectFields(WorldData world,RunoffConfiguration runoff,Site[] replacements)
        {
            if(Connected(world))throw new ArgumentException("Connected fields already applied.");
            if(replacements==null||replacements.Length!=8||replacements.Select(s=>s.id).Distinct().Count()!=8)
                throw new ArgumentException("Eight unique connected fields are required.");
            var delta=new System.Collections.Generic.Dictionary<string,double>();
            foreach(var next in replacements)
            {
                var old=world.sites.Single(s=>s.id==next.id);
                if(old.protectedSite||!next.id.StartsWith("south-")||next.sourceId!="scenario-connected-fields-v2")
                    throw new ArgumentException("Only the authored southern fields can connect.");
                foreach(var c in old.baselineCover)delta[c.coverId]=(delta.TryGetValue(c.coverId,out var n)?n:0)-old.analysisAreaM2*c.fraction;
                foreach(var c in next.baselineCover)delta[c.coverId]=(delta.TryGetValue(c.coverId,out var n)?n:0)+next.analysisAreaM2*c.fraction;
            }
            foreach(var pair in delta)
                if(runoff.context.Where(c=>c.coverId==pair.Key).Sum(c=>c.areaM2)<pair.Value)
                    throw new ArgumentException("Connected fields exceed source context cover.");
            foreach(var pair in delta)runoff.context.First(c=>c.coverId==pair.Key).areaM2-=(float)pair.Value;
            foreach(var next in replacements)world.sites[Array.FindIndex(world.sites,s=>s.id==next.id)]=next;
        }
        public static void AddWind(WorldData world, RunoffConfiguration runoff)
        {
            const string id="annual-wind-13";
            if(world.sites.Any(s=>s.id==id))throw new ArgumentException("Annual opportunities already applied.");
            const float grass=222.1696208f, cultivated=22500-grass;
            var site=new Site {id=id,name="Eastern wind opportunity",sourceId="scenario-assumptions-v1",contributingAreaId="site-only-"+id,modelParameterId="cn-b-cover-assumed-v1",
                polygon=new[]{new Point(375,-495),new Point(525,-495),new Point(525,-345),new Point(375,-345)},analysisAreaM2=22500,
                baselineProduction=3,baselineCover=new[]{new CoverFraction{coverId="cultivated",fraction=cultivated/22500},new CoverFraction{coverId="grass",fraction=grass/22500}},
                allowedTreatments=Array.Empty<string>(),offers=Array.Empty<SiteOffer>(),protectionReason="Fictional wind-only option with assumed consent/access; full rotor clearance stays inside this envelope."};
            world.sites=world.sites.Concat(new[]{site}).ToArray();
            // The original catchment remains exactly the same size: this site replaces context area.
            runoff.context.First(c=>c.coverId=="grass").areaM2-=grass;
            runoff.context.First(c=>c.coverId=="cultivated").areaM2-=cultivated;
            runoff.connections=runoff.connections.Concat(new[]{new RunoffConnection{siteId=id,groupId=site.contributingAreaId}}).ToArray();
        }
    }
}
