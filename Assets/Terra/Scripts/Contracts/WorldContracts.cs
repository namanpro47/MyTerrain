using System;

namespace Terra.Contracts
{
    [Serializable] public sealed class Point { public float x, z; public Point() {} public Point(float x, float z) { this.x=x; this.z=z; } }
    [Serializable] public sealed class Source { public string id, title, url, date, license, role; }
    [Serializable] public sealed class Layer { public string id, path, availability; }
    [Serializable] public sealed class Manifest
    {
        public int schemaVersion; public string packId, fixtureVersion, geographyKind, selectedLocationId, sourceCrs, elevationDatum;
        public double[] boundsWgs84, originWgs84; public float originElevationM; public float[] editableBounds, contextBounds;
        public string worldDataPath, scenarioPath, rasterRows, winding; public Layer[] layers; public Source[] sources;
    }
    [Serializable] public sealed class TerrainGrid { public int columns, rows; public float cellSizeM, xMin, zMin; public float[] heights; }
    [Serializable] public sealed class CoverFraction { public string coverId; public float fraction; }
    [Serializable] public sealed class CoverPatch { public string id, coverId, sourceId; public Point[] polygon; }
    [Serializable] public sealed class Feature { public string id, kind, sourceId; public bool decorative; public float widthM; public Point[] points; }
    [Serializable] public sealed class Site
    {
        public string id, name, sourceId, contributingAreaId, modelParameterId; public Point[] polygon; public float analysisAreaM2, baselineProduction;
        public bool protectedSite; public string protectionReason; public CoverFraction[] baselineCover; public string[] allowedTreatments; public SiteOffer[] offers;
    }
    [Serializable] public sealed class SiteOffer { public string treatmentId; public int cost; public float production, retentionCapacityM3; }
    [Serializable] public sealed class WorldData { public TerrainGrid terrain; public CoverPatch[] covers; public Feature[] features; public Site[] sites; }
    [Serializable] public sealed class Treatment
    {
        public string id, family, title, description, visualRecipeId, modelParameterId;
        public float productiveFraction; public CoverFraction[] cover;
    }
    [Serializable] public sealed class Storm { public string id, soilGroup, antecedentCondition, sourceId; public float rainfallMm, initialAbstractionRatio; }
    [Serializable] public sealed class Objectives { public float productionMin, dischargeMaxM3; public bool runoffEnabled; }
    [Serializable] public sealed class Scenario
    {
        public int schemaVersion, budget; public string id, packId, introId, learningId, assumptionsId; public Treatment[] treatments; public Storm storm; public Objectives objectives;
    }
    [Serializable] public sealed class EditCommand { public string operation, siteId, destinationSiteId, treatmentId; public int expectedRevision; }
    [Serializable] public sealed class Intervention { public string siteId, treatmentId; }
    [Serializable] public sealed class ExplanationFact { public string code, siteId, treatmentId; public float value; public string unit, sourceId; }
    [Serializable] public sealed class CoverArea { public string coverId; public float areaM2; }
    [Serializable] public sealed class SiteResult { public string siteId; public float production, productionDelta, generatedM3, retainedM3, dischargedM3; }
    [Serializable] public sealed class WorldResult
    {
        public int revision, spentCredits, remainingCredits; public float production, baselineProduction, netProduction;
        public bool runoffAvailable; public float generatedM3, retainedM3, dischargedM3;
        public CoverArea[] coverAreas; public SiteResult[] sites; public ExplanationFact[] explanations;
    }
    [Serializable] public sealed class Evaluation { public bool valid; public string reason; public WorldResult result; public Intervention[] interventions; }
    [Serializable] public sealed class SimulationSiteInput { public string siteId, contributingAreaId; public float areaM2, curveNumber, retentionCapacityM3; }
    [Serializable] public sealed class SimulationInput { public int schemaVersion, revision; public Storm storm; public SimulationSiteInput[] sites; }
    [Serializable] public sealed class SimulationOutput { public int schemaVersion, revision; public float generatedM3, retainedM3, dischargedM3; public SiteResult[] sites; public ExplanationFact[] explanations; }
    [Serializable] public sealed class ContentChoice { public string id, text, explanation; public bool correct; }
    [Serializable] public sealed class ContentEntry { public string id, title, body; public ContentChoice[] choices; public string[] sourceIds; }
}
