using System;
using System.Collections.Generic;
using Terra.Contracts;

namespace Terra.Simulation
{
    /// <summary>Educational event-volume model, not hydraulic routing or flood depth.</summary>
    public static class RunoffEvaluator
    {
        public const string ModelVersion = "terra-direct-runoff-v1";
        public const string FormulaSourceId = "usda-tr55-1986";

        sealed class Group
        {
            public double generated, capacity;
        }

        /// <summary>
        /// Throws ArgumentException for invalid input or values outside the float output range.
        /// Results and facts are sorted by ordinal site ID. The caller's DTOs are never modified.
        /// </summary>
        public static SimulationOutput Evaluate(SimulationInput input)
        {
            Validate(input);
            var ordered = (SimulationSiteInput[])input.sites.Clone();
            Array.Sort(ordered, (a, b) => StringComparer.Ordinal.Compare(a.siteId, b.siteId));
            var groups = new Dictionary<string, Group>(StringComparer.Ordinal);
            var generated = new double[ordered.Length];
            for (int i = 0; i < ordered.Length; i++)
            {
                var site = ordered[i];
                var volume = DirectRunoffDepthMm(input.storm.rainfallMm, site.curveNumber) * site.areaM2 / 1000d;
                generated[i] = volume;
                if (!groups.TryGetValue(site.contributingAreaId, out var group))
                    groups.Add(site.contributingAreaId, group = new Group());
                group.generated += volume;
                group.capacity += site.retentionCapacityM3;
            }

            var facts = new List<ExplanationFact>();
            var results = new SiteResult[ordered.Length];
            double totalGenerated = 0, totalRetained = 0, totalDischarged = 0;
            for (int i = 0; i < ordered.Length; i++)
            {
                var site = ordered[i];
                var group = groups[site.contributingAreaId];
                double groupRetained = Math.Min(group.generated, group.capacity);
                double retained = group.generated > 0 ? generated[i] * (groupRetained / group.generated) : 0;
                double discharged = generated[i] - retained;
                results[i] = new SiteResult {
                    siteId = site.siteId, generatedM3 = Output(generated[i]),
                    retainedM3 = Output(retained), dischargedM3 = Output(discharged)
                };
                totalGenerated += generated[i]; totalRetained += retained; totalDischarged += discharged;
                Add(facts, "runoff-generated", site.siteId, generated[i], "m3", FormulaSourceId);
                Add(facts, "runoff-retained", site.siteId, retained, "m3", input.storm.sourceId);
                Add(facts, "runoff-discharged", site.siteId, discharged, "m3", input.storm.sourceId);
                // Repeated per-site group context is not additive across sites.
                Add(facts, "connected-runoff", site.siteId, group.generated, "m3", input.storm.sourceId);
                if (site.retentionCapacityM3 > 0)
                {
                    // Capacity attribution is proportional across devices, separate from source-site retention.
                    double used = site.retentionCapacityM3 * (groupRetained / group.capacity);
                    Add(facts, "retention-capacity", site.siteId, site.retentionCapacityM3, "m3", input.storm.sourceId);
                    Add(facts, "retention-capacity-used", site.siteId, used, "m3", input.storm.sourceId);
                    Add(facts, "retention-capacity-unused", site.siteId, site.retentionCapacityM3 - used, "m3", input.storm.sourceId);
                }
            }
            Add(facts, "storm-rainfall", "", input.storm.rainfallMm, "mm", input.storm.sourceId);
            return new SimulationOutput {
                schemaVersion = 1, revision = input.revision,
                generatedM3 = Output(totalGenerated), retainedM3 = Output(totalRetained),
                dischargedM3 = Output(totalDischarged), sites = results, explanations = facts.ToArray()
            };
        }

        public static double DirectRunoffDepthMm(double rainfallMm, double curveNumber)
        {
            Need(Finite(rainfallMm) && rainfallMm >= 0, "rainfallMm must be finite and nonnegative.");
            Need(Finite(curveNumber) && curveNumber > 0 && curveNumber <= 100, "curveNumber must be finite in (0, 100].");
            double storage = 25400d / curveNumber - 254d;
            Need(Finite(storage), "curveNumber produces storage outside the finite double arithmetic range.");
            if (rainfallMm <= .2d * storage) return 0;
            double excess = rainfallMm - .2d * storage;
            // Algebraically Q=excess²/(excess+storage); avoid squaring large values.
            return excess / (1d + storage / excess);
        }

        static void Validate(SimulationInput input)
        {
            Need(input != null, "Simulation input is required.");
            Need(input.schemaVersion == 1, "Unsupported simulation schemaVersion; expected 1.");
            Need(input.revision >= 0, "revision must be nonnegative.");
            Need(input.storm != null, "storm is required.");
            var storm = input.storm;
            Need(Id(storm.id) && Id(storm.sourceId) && Id(storm.soilGroup) && Id(storm.antecedentCondition), "storm ID, source, soil and antecedent-condition metadata are required.");
            Need(Finite(storm.rainfallMm) && storm.rainfallMm >= 0, "storm.rainfallMm must be finite and nonnegative.");
            Need(storm.initialAbstractionRatio == .2f, "v1 supports only initialAbstractionRatio 0.2; changing it requires recalibrated CN assumptions.");
            Need(input.sites != null && input.sites.Length > 0, "sites must contain at least one analysis unit.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            for (int i = 0; i < input.sites.Length; i++)
            {
                var site = input.sites[i];
                string prefix = "sites[" + i + "]: ";
                Need(site != null, prefix + "site is required.");
                Need(Id(site.siteId) && ids.Add(site.siteId), prefix + "siteId must be present and unique.");
                Need(Id(site.contributingAreaId), prefix + "contributingAreaId is required; use an explicit isolated group for disconnected units.");
                Need(Finite(site.areaM2) && site.areaM2 >= 0, prefix + "areaM2 must be finite and nonnegative.");
                Need(Finite(site.curveNumber) && site.curveNumber > 0 && site.curveNumber <= 100, prefix + "curveNumber must be finite in (0, 100].");
                Need(Finite(site.retentionCapacityM3) && site.retentionCapacityM3 >= 0, prefix + "retentionCapacityM3 must be finite and nonnegative.");
            }
        }

        static void Add(List<ExplanationFact> facts, string code, string siteId, double value, string unit, string sourceId)
        {
            facts.Add(new ExplanationFact { code = code, siteId = siteId, treatmentId = "", value = Output(value), unit = unit, sourceId = sourceId });
        }
        static float Output(double value)
        {
            Need(Finite(value) && value >= 0 && value <= float.MaxValue, "Calculated result exceeds the finite nonnegative float contract range.");
            return (float)value;
        }
        internal static bool Finite(double value) => !double.IsNaN(value) && !double.IsInfinity(value);
        internal static bool Id(string value) => !string.IsNullOrWhiteSpace(value) && value == value.Trim();
        internal static void Need(bool condition, string message) { if (!condition) throw new ArgumentException(message); }
    }
}
