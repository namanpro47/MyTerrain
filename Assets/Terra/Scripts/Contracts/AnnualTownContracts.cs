using System;

namespace Terra.Contracts
{
    public enum TownPhase { Planning, Resolving, Reviewing, Completed }
    public enum TownAssetPhase { Building, Establishing, Operating, Suspended, Retired }

    // Money is integer USD cents. These contracts are independent of WorldPack/runoff v1.
    [Serializable] public sealed class AnnualTownDefinition
    {
        public int schemaVersion = 1, priceYear = 2026, years = 10;
        public string id, version, worldPackId, currency = "USD", assumptions;
        public long startingCashCents;
        public long[] appropriationsCents;
        public TownSiteDefinition[] sites;
        public TownActionDefinition[] actions;
        public TownYearDriver[] drivers;
        public TownScoreRubric rubric;
    }

    [Serializable] public sealed class TownSiteDefinition
    {
        public string id, name;
        public bool protectedSite;
        public double areaM2, baselineProductionUnits, baselineServiceUnits, baselineVegetatedFraction;
    }

    [Serializable] public sealed class TownActionDefinition
    {
        public string siteId, actionId, title, visualTreatmentId;
        public string sourceId, evidenceLevel, contributionType, ownerOperator, fundedUnit;
        public double fundedQuantity;
        public bool totalProjectCostKnown;
        public bool servicesFollowProduction = true;
        public long totalProjectCostCents, upfrontPublicCostCents, annualPublicPaymentCents;
        public long annualPublicOperatingCents, annualPublicReceiptsCents;
        public int paymentYears, operatingYears, constructionYears, establishmentYears = 1;
        public double productionUnits, serviceUnits, vegetatedFraction, retentionCapacityM3;
    }

    [Serializable] public sealed class TownScoreRubric
    {
        public string version, explanation;
        public double productionTargetUnits, serviceTargetUnits, retentionTargetM3;
        public double economicProductionWeight = .8, socialServiceWeight = .8, environmentalVegetationWeight = .8;
    }

    [Serializable] public sealed class TownYearDriver
    {
        public int year;
        public string id, description, evidenceLevel;
        public double productionFactor = 1;
        public long unexpectedPublicRepairCents;
    }

    [Serializable] public sealed class TownPlannedAction { public string siteId, actionId; }
    [Serializable] public sealed class TownPlanSnapshot { public TownPlannedAction[] actions; }
    [Serializable] public sealed class TownAsset
    {
        public string siteId, actionId;
        public int startedYear;
        public double condition = 1;
    }

    [Serializable] public sealed class TownYearRequest
    {
        public int schemaVersion = 1, year, revision;
        public string runId, requestId, definitionHash, inputHash;
        public TownPlannedAction[] actions;
        public TownAsset[] assets;
        public TownYearDriver driver;
    }

    // Provider output contains bounded contextual effects, never cash, time or invented sites.
    [Serializable] public sealed class TownSiteEffect
    {
        public string siteId, reason, sourceId;
        public double productionFactor = 1, conditionDelta;
        public bool disrupted;
    }
    [Serializable] public sealed class TownYearConsequences
    {
        public int schemaVersion = 1, year;
        public string runId, requestId, inputHash;
        public TownSiteEffect[] effects;
    }

    [Serializable] public sealed class TownYearLedger
    {
        public string currency;
        public int priceYear;
        public long openingCashCents, upfrontPaidCents, programDueCents, programPaidCents;
        public long operatingDueCents, operatingPaidCents, repairDueCents, repairPaidCents;
        public long openingArrearsCents, arrearsPaidCents, closingArrearsCents;
        public long appropriationCents, publicReceiptsCents, closingCashCents, futureCommitmentsCents;
    }

    [Serializable] public sealed class TownSiteYearResult
    {
        public string siteId, actionId, explanation;
        public TownAssetPhase phase;
        public double maturity, condition, productionUnits, serviceUnits, vegetatedAreaM2, retentionCapacityM3;
        public bool disrupted;
    }

    [Serializable] public sealed class TownYearOutcomes
    {
        public double economicScore, socialScore, environmentalScore;
        public double productionUnits, serviceUnits, vegetatedAreaM2, managedAreaM2, retentionCapacityM3;
        public int disruptedSites;
        public string rubricVersion, economicExplanation, socialExplanation, environmentalExplanation;
        public TownSiteYearResult[] sites;
    }

    [Serializable] public sealed class TownYearReport
    {
        public int year;
        public TownYearLedger ledger;
        public TownYearOutcomes outcomes;
        public TownYearRequest acceptedRequest;
        public TownYearConsequences acceptedConsequences;
    }

    [Serializable] public sealed class AnnualTownSnapshot
    {
        public int schemaVersion = 1, year, revision;
        public string runId, definitionHash;
        public TownPhase phase;
        public long cashCents, arrearsCents;
        public TownPlannedAction[] queue;
        public TownPlanSnapshot[] undo;
        public TownPlanSnapshot[] redo = Array.Empty<TownPlanSnapshot>();
        public TownAsset[] assets;
        public TownYearReport[] history;
        public TownYearRequest pendingRequest;
    }
}
