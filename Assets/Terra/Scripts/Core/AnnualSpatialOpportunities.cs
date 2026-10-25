using System;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    // Same AOI, additional fictional opportunity. Source cover clipped from canonical NLCD polygons.
    // This is neither a surveyed parcel nor legal/engineering permission to build a turbine.
    public static class AnnualSpatialOpportunities
    {
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
