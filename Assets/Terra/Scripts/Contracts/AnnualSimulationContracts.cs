using System;

namespace Terra.Contracts
{
    // Numeric v4 additions are optional on legacy DTOs; never reinterpret old accepted years.
    [Serializable] public sealed class TownWeatherSchedule
    {
        public string packId, packSha256, dailySha256, samplerVersion, sharedScenarioSeed;
        public int startYear;
        public string[] recordIds;
    }
    [Serializable] public sealed class TownCropDefinition
    {
        public string id, units, evidenceLevel, ruleId;
        public int sowMonth, sowDay, harvestMonth, harvestDay;
        public double outputPerHa, referenceRainMm, droughtLossPerDay, heatLossPerDay, excessRainLossPerMm;
    }
    [Serializable] public sealed class TownCropWeather
    {
        public string cropId, ruleId;
        public int firstDay, harvestDay, precipTenthsMm, longestDrySpellDays, hotDays;
    }
    [Serializable] public sealed class TownWeatherEvent
    {
        public string recordId, date;
        public int dayOfYear, month, precipTenthsMm, antecedent7DaysTenthsMm, tminTenthsC, tmaxTenthsC;
        public bool inAnalysisWindow;
    }
    [Serializable] public sealed class TownSimulationDriver
    {
        public int year, historicalYear;
        public string recordId, packSha256, derivationVersion;
        public TownCropWeather[] crops;
        public TownWeatherEvent[] events;
        // Separate, explicit assumptions: this precipitation/temperature pack has no irradiance/wind.
        public double solarFactor = 1, windFactor = 1;
        public string generationRuleId;
    }

    // Input-only NOAA preparation DTOs. Exact source bytes are checked before deserialization.
    [Serializable] public sealed class TownWeatherPack
    {
        public int schemaVersion;
        public string packId, derivationVersion;
        public TownWeatherWindow[] eligibleWindows;
        public TownWeatherRecord[] records;
    }
    [Serializable] public sealed class TownWeatherWindow { public int startYear; public string[] recordIds; }
    [Serializable] public sealed class TownWeatherRecord
    {
        public string recordId, stationId;
        public int year, days;
        public TownWeatherEvent[] heavyPrecipitationEvents;
    }
    [Serializable] public sealed class TownDailyWeather
    {
        public int schemaVersion;
        public string packId, stationId;
        public TownDailyYear[] years;
    }
    [Serializable] public sealed class TownDailyYear { public int year; public TownWeatherDay[] days; }
    [Serializable] public sealed class TownWeatherDay
    {
        public int dayOfYear, precipTenthsMm, tminTenthsC, tmaxTenthsC;
        public string precipFlags, tminFlags, tmaxFlags;
    }
}
