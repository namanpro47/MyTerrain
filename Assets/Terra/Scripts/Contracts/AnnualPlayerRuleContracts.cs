using System;

namespace Terra.Contracts
{
    // Optional, versioned extension. Absent on all previously accepted runs.
    [Serializable] public sealed class TownPlayerRules
    {
        public string version, regionId, scenarioSeed, disclosure;
        public TownHazardDefinition[] hazards;
    }
    [Serializable] public sealed class TownHazardDefinition
    {
        public string id, title, sourceUrl, evidence;
        public double annualChance, outputFactor, conditionLoss, affectedSiteChance;
    }
    [Serializable] public sealed class TownRegionalEvent
    {
        public string id, title, evidence;
        public double outputFactor, conditionLoss;
        public string[] affectedSiteIds;
    }
}
