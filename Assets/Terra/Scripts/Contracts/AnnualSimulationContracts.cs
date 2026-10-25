using System;

namespace Terra.Contracts
{
    // Numeric v4 additions are optional on legacy DTOs; never reinterpret old accepted years.
    [Serializable] public sealed class TownSimulationDefinition
    {
        public string version, disclosure, assumptions, sensitivity;
        public double sensitivityFactor;
        public TownWeatherSchedule weather;
        public TownSimulationDriver[] drivers;
        public TownCropDefinition[] crops;
        public TownPracticeDefinition[] practices;
        public TownExposureDefinition[] sites;
        public TownPublicResponsibility[] publicResponsibilities;
        public double dailyStorageRecoveryFraction, privateAnnualRecovery, drySpellGraceDays;
        public double rainDamageThresholdMm, localRainDamagePerMm, upstreamRainThresholdMm, upstreamDamagePerMm;
        public double antecedentAmplificationPerMm, maximumEventDamage, minimumWeatherFactor, maximumWeatherFactor;
        public string eventRuleId, recoveryRuleId;
    }
    [Serializable] public sealed class TownPracticeDefinition
    {
        public string actionId, ruleId;
        public string[] cropSequence;
        public double croppedFraction, matureDroughtMitigation, matureRainVulnerability, establishmentCostFraction;
    }
    [Serializable] public sealed class TownExposureDefinition
    {
        public string siteId, connectionGroupId, baselineCropId, ruleId;
        public double exposure, baselineCroppedFraction;
    }
    [Serializable] public sealed class TownPublicResponsibility
    {
        public string actionId, responsibility, ruleId;
        public long repairCentsPerConditionUnit;
    }
    [Serializable] public sealed class TownSimulationState
    {
        public TownSimulationSiteState[] sites;
        public TownRepairLiability[] repairs;
    }
    [Serializable] public sealed class TownSimulationSiteState
    {
        public string siteId, cropId;
        public int rotationPhase;
        public double condition = 1, outstandingDamage, recoveredLastYear, storedWaterM3;
    }
    [Serializable] public sealed class TownRepairLiability
    {
        public string id, siteId, actionId, eventId, responsibility, ruleId;
        public int createdYear;
        public long originalCents, unpaidCents;
        public double originalConditionLoss, unrecoveredCondition;
    }
    [Serializable] public sealed class TownSimulationRequest
    {
        public string rulesVersion;
        public TownSimulationDriver driver;
        public TownSimulationState openingState;
    }
    [Serializable] public sealed class TownSimulationReport
    {
        public string version, transitionId, transitionHash;
        public TownSimulationDriver driver;
        public TownSimulationState closingState;
        public TownCropResult[] crops;
        public TownEventImpact[] impacts;
        public TownPublicPayment[] payments;
        public double economicDelta, socialDelta, environmentalDelta;
        public double energyDeficit, waterDeficit, wastewaterDeficit, housingDeficit, jobsDeficit;
    }
    [Serializable] public sealed class TownCropResult
    {
        public string siteId, cropId, practiceId, units, ruleId;
        public int harvestDay;
        public double productiveAreaM2, benchmarkPerHa, weatherFactor, managementFactor, harvestCondition, outputUnits;
        public double openingCondition, closingConditionBeforeRecovery, nextYearCondition;
    }
    [Serializable] public sealed class TownEventImpact
    {
        public string siteId, eventId, ruleId, connectionGroupId;
        public int dayOfYear;
        public bool beforeHarvest, publiclyResponsible;
        public double localHazard, upstreamHazard, connectedStorageUsedM3, conditionLoss;
    }
    [Serializable] public sealed class TownPublicPayment
    {
        public string id, siteId, actionId, kind, liabilityId;
        public long dueCents, paidCents;
        public double nextYearRecovery;
    }
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
