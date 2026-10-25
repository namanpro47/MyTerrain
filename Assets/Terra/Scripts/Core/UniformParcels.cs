using System;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    [Serializable] public sealed class UniformParcelPack
    {
        public int schemaVersion, gridSizeM;
        public string packId, version, worldSha256, title, story;
        public Site[] sites;
        public RunoffContext[] context;
        public RunoffConnection[] connections;
        public StartingBuilding[] initialBuildings;
    }

    public static class UniformParcels
    {
        public const string SourceId = "scenario-open-grid-v1";
        public static bool Applied(WorldData world) => world.sites.Any(s => s.sourceId == SourceId);

        // The original pack remains intact. This is an explicitly versioned scenario overlay.
        public static void Apply(WorldData world, RunoffConfiguration runoff, UniformParcelPack pack, string packId)
        {
            void Need(bool ok,string reason) { if(!ok)throw new ArgumentException(reason); }
            Need(!Applied(world),"Open-land parcels have already been applied.");
            Need(pack != null && pack.schemaVersion == 1 && pack.packId == packId && pack.version == "4.0.0"
                && pack.gridSizeM == 60 && pack.worldSha256 == runoff.worldSha256,"Parcel overlay does not match this source landscape.");
            Need(pack.sites != null && pack.sites.Length > 20 && pack.sites.Select(s=>s.id).Distinct().Count()==pack.sites.Length,
                "Parcel IDs must be unique and the complete overlay must be present.");
            Need(pack.context != null && pack.connections != null && pack.initialBuildings != null,"Incomplete parcel overlay.");
            foreach(var site in pack.sites)
            {
                Need(site.analysisAreaM2 > 0 && site.baselineCover != null && Math.Abs(site.baselineCover.Sum(c=>(double)c.fraction)-1)<.00001,
                    "Every parcel needs an exact baseline cover partition.");
                Need(pack.connections.Count(c=>c.siteId==site.id)==1,"Every parcel needs one explicit runoff group.");
            }
            Need(Math.Abs(pack.context.Sum(c=>(double)c.areaM2)+pack.sites.Sum(s=>(double)s.analysisAreaM2)-runoff.analysisAreaM2)<.2,
                "Open-land parcels and context must conserve the source analysis area.");
            foreach(var old in world.sites.Where(s=>s.protectedSite))
                Need(pack.sites.Any(s=>s.id==old.id && s.protectedSite && s.analysisAreaM2==old.analysisAreaM2
                    && s.polygon.Length==old.polygon.Length && s.polygon.Zip(old.polygon,(a,b)=>a.x==b.x&&a.z==b.z).All(x=>x)),
                    "Existing protected geography must be retained.");
            Need(pack.initialBuildings.Length==7 && pack.initialBuildings.Select(b=>b.siteId).Distinct().Count()==7
                && pack.initialBuildings.Count(b=>b.actionId=="low-density-housing")==4,"Opening community must occupy seven distinct slots, including four homes.");
            foreach(var initial in pack.initialBuildings)
                Need(pack.sites.Any(s=>s.id==initial.siteId && s.sourceId==SourceId && s.protectedSite && s.offers.Length==0),
                    "Starting community slots cannot be offered as new construction.");
            world.sites=TownCopies.Copy(pack.sites);
            runoff.context=TownCopies.Copy(pack.context);
            runoff.connections=TownCopies.Copy(pack.connections);
            runoff.partitionSha256=TownCopies.Hash(pack);
            runoff.connectionNote="Authored isolated grid groups; each parcel handles its own same-storm runoff. No inferred drainage connections.";
        }
    }
}
