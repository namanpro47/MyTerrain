using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using Terra.Contracts;
using Terra.Simulation;

static class Program
{
    static readonly JsonSerializerOptions Json = new JsonSerializerOptions { IncludeFields = true, WriteIndented = true };
    static readonly List<string> Passed = new List<string>();
    static void Check(string name, Action check) { check(); Passed.Add(name); }
    static void True(bool value, string message = "assertion failed") { if (!value) throw new Exception(message); }
    static void Near(double actual, double expected, double tolerance = .001) => True(Math.Abs(actual - expected) <= tolerance, $"{actual} != {expected} ± {tolerance}");
    static void Reject(Action action, string diagnostic)
    {
        try { action(); } catch (ArgumentException e) { True(e.Message.Contains(diagnostic), e.Message); return; }
        throw new Exception("Expected rejection: " + diagnostic);
    }
    static SimulationSiteInput Site(string id = "a", string group = "east", float area = 1000, float cn = 100, float capacity = 0) =>
        new SimulationSiteInput { siteId = id, contributingAreaId = group, areaM2 = area, curveNumber = cn, retentionCapacityM3 = capacity };
    static SimulationInput Input(params SimulationSiteInput[] sites) => new SimulationInput {
        schemaVersion = 1, revision = 7, sites = sites,
        storm = new Storm { id = "storm-test", rainfallMm = 10, initialAbstractionRatio = .2f,
            soilGroup = "B", antecedentCondition = "average; educational assumption", sourceId = "scenario-assumptions-v1" }
    };
    static SimulationOutput Run(params SimulationSiteInput[] sites) => RunoffEvaluator.Evaluate(Input(sites));
    static string Serialize<T>(T value) => JsonSerializer.Serialize(value, Json);
    static SimulationInput Clone(SimulationInput input) => JsonSerializer.Deserialize<SimulationInput>(Serialize(input), Json);
    static void Balance(SimulationOutput output)
    {
        double tolerance = Math.Max(.001, output.generatedM3 * 2e-7);
        Near(output.generatedM3, output.retainedM3 + (double)output.dischargedM3, tolerance);
        Near(output.generatedM3, output.sites.Sum(s => (double)s.generatedM3), tolerance);
        Near(output.retainedM3, output.sites.Sum(s => (double)s.retainedM3), tolerance);
        Near(output.dischargedM3, output.sites.Sum(s => (double)s.dischargedM3), tolerance);
        foreach (var s in output.sites) {
            True(s.generatedM3 >= 0 && s.retainedM3 >= 0 && s.dischargedM3 >= 0);
            Near(s.generatedM3, s.retainedM3 + (double)s.dischargedM3, Math.Max(.001, s.generatedM3 * 2e-7));
            True(s.production == 0 && s.productionDelta == 0, "Hydrology must not fabricate production");
        }
    }
    static int Main(string[] args)
    {
        try {
            string root = args.Length == 0 ? Directory.GetCurrentDirectory() : Path.GetFullPath(args[0]);
            Check("Frozen fixture: all aggregate and per-site hydrology fields", () => {
                var input = JsonSerializer.Deserialize<SimulationInput>(File.ReadAllText(Path.Combine(root, "tests/fixtures/simulation/input.json")), Json);
                var expected = JsonSerializer.Deserialize<SimulationOutput>(File.ReadAllText(Path.Combine(root, "tests/fixtures/simulation/expected.json")), Json);
                var result = RunoffEvaluator.Evaluate(input);
                Near(result.generatedM3, expected.generatedM3); Near(result.retainedM3, expected.retainedM3); Near(result.dischargedM3, expected.dischargedM3);
                True(result.revision == expected.revision && result.schemaVersion == expected.schemaVersion);
                foreach (var s in expected.sites) {
                    var actual = result.sites.Single(a => a.siteId == s.siteId);
                    Near(actual.generatedM3, s.generatedM3); Near(actual.retainedM3, s.retainedM3); Near(actual.dischargedM3, s.dischargedM3);
                }
                Balance(result);
            });
            Check("Independent unit case: 10 mm × 1000 m2 at CN100 = 10 m3, retain 3, discharge 7", () => {
                var r = Run(Site(capacity: 3)); Near(r.generatedM3, 10); Near(r.retainedM3, 3); Near(r.dischargedM3, 7); Balance(r);
            });
            Check("Independent USDA published table 2-1, 2 inches/CN80 = 0.56 inches (rounded)", () => {
                Near(RunoffEvaluator.DirectRunoffDepthMm(50.8, 80) / 25.4, .56, .005);
            });
            Check("Independent fractional hand case: CN50, P127 mm, S254, Q=17.5846153846154 mm", () => {
                Near(RunoffEvaluator.DirectRunoffDepthMm(127, 50), 17.5846153846154, 1e-10);
            });
            Check("Initial abstraction below/equal/above boundary: CN80 S63.5 Ia12.7", () => {
                Near(RunoffEvaluator.DirectRunoffDepthMm(12.699, 80), 0, 0);
                Near(RunoffEvaluator.DirectRunoffDepthMm(12.7, 80), 0, 1e-25);
                True(RunoffEvaluator.DirectRunoffDepthMm(12.701, 80) > 0);
            });
            Check("Zero storm and zero baseline admit finite absolute difference", () => {
                var i = Input(Site(capacity: 3)); i.storm.rainfallMm = 0; var baseline = RunoffEvaluator.Evaluate(i);
                Near(baseline.generatedM3, 0, 0); Near(baseline.retainedM3, 0, 0);
                var proposal = Run(Site(capacity: 3)); Near(proposal.dischargedM3 - baseline.dischargedM3, 7); Balance(baseline);
            });
            Check("Zero-area storage-only unit connects explicitly, with no generation of its own", () => {
                var r = Run(Site(), Site("device", area: 0, capacity: 3)); Near(r.generatedM3, 10); Near(r.retainedM3, 3); Near(r.sites[1].generatedM3, 0); Balance(r);
            });
            Check("Disconnected capacity cannot capture another group's runoff", () => {
                var r = Run(Site(), Site("device", "west", 0, capacity: 100)); Near(r.retainedM3, 0); Near(r.dischargedM3, 10); Balance(r);
            });
            Check("Multiple devices share generation once; proportional source allocation", () => {
                var r = Run(Site(capacity: 3), Site("b", area: 2000, capacity: 6)); Near(r.generatedM3, 30); Near(r.retainedM3, 9);
                Near(r.sites[0].retainedM3, 3); Near(r.sites[1].retainedM3, 6); Balance(r);
            });
            Check("Saturated storage caps at group generation, with proportional used/unused device facts", () => {
                var r = Run(Site(capacity: 100), Site("b", capacity: 300)); Near(r.generatedM3, 20); Near(r.retainedM3, 20); Near(r.dischargedM3, 0);
                Near(r.explanations.Single(f => f.code == "retention-capacity-used" && f.siteId == "a").value, 5);
                Near(r.explanations.Single(f => f.code == "retention-capacity-unused" && f.siteId == "b").value, 285); Balance(r);
            });
            Check("Cover and retention compose once: grass CN61 at 50 mm minus 1 m3 capacity", () => {
                var i = Input(Site(cn: 61, capacity: 1)); i.storm.rainfallMm = 50; var r = RunoffEvaluator.Evaluate(i);
                Near(r.generatedM3, 1.706343414451108); Near(r.retainedM3, 1); Near(r.dischargedM3, .706343414451108); Balance(r);
            });
            Check("Repeatability, permutation invariance, input immutability, no output aliasing, restoration", () => {
                var i = Input(Site("z", capacity: 2), Site("a", cn: 78), Site("m", area: 3000)); i.storm.rainfallMm = 50;
                var before = Serialize(i); var result = RunoffEvaluator.Evaluate(i); string snapshot = Serialize(result);
                True(snapshot == Serialize(RunoffEvaluator.Evaluate(i))); True(before == Serialize(i));
                var reversed = Clone(i); Array.Reverse(reversed.sites); True(snapshot == Serialize(RunoffEvaluator.Evaluate(reversed)));
                result.sites[0].dischargedM3 = -99; result.explanations[0].value = -99; True(snapshot == Serialize(RunoffEvaluator.Evaluate(i)));
                var edited = Clone(i); edited.sites[0].curveNumber = 55; True(snapshot != Serialize(RunoffEvaluator.Evaluate(edited)));
                edited.sites[0].curveNumber = i.sites[0].curveNumber; True(snapshot == Serialize(RunoffEvaluator.Evaluate(edited)));
            });
            Check("Monotonic rainfall/CN/area/capacity, nonnegative conservation across 1000 combinations", () => {
                foreach (float p in new float[] { 0, 1, 10, 25, 50, 100, 500, 1000, 2500, 10000 })
                foreach (float cn in new float[] { .1f, 1, 10, 30, 55, 61, 73.75f, 78, 90, 100 })
                foreach (float cap in new float[] { 0, 1, 2, 5, 10, 50, 100, 1000, 10000, 100000 }) {
                    var i = Input(Site(cn: cn, capacity: cap), Site("b", area: 2000, cn: cn)); i.storm.rainfallMm = p;
                    var r = RunoffEvaluator.Evaluate(i); Balance(r);
                    True(r.generatedM3 <= p * 3 + .001); Near(r.sites[1].generatedM3, 2d * r.sites[0].generatedM3, .01);
                    i.sites[0].retentionCapacityM3 += 1; True(RunoffEvaluator.Evaluate(i).dischargedM3 <= r.dischargedM3);
                    True(RunoffEvaluator.DirectRunoffDepthMm(p + 1, cn) >= RunoffEvaluator.DirectRunoffDepthMm(p, cn));
                    True(RunoffEvaluator.DirectRunoffDepthMm(p, Math.Min(100, cn + 1)) >= RunoffEvaluator.DirectRunoffDepthMm(p, cn));
                }
            });
            Check("Malformed inputs reject with useful diagnostics", () => {
                Reject(() => RunoffEvaluator.Evaluate(null), "input");
                var invalid = new (Action<SimulationInput> Change, string Reason)[] {
                    (i => i.schemaVersion = 2, "schemaVersion"), (i => i.revision = -1, "revision"),
                    (i => i.storm = null, "storm"), (i => i.storm.id = "", "metadata"),
                    (i => i.storm.sourceId = null, "metadata"), (i => i.storm.soilGroup = "", "metadata"),
                    (i => i.storm.antecedentCondition = "", "metadata"), (i => i.storm.rainfallMm = -1, "rainfallMm"),
                    (i => i.storm.rainfallMm = float.NaN, "rainfallMm"), (i => i.storm.rainfallMm = float.PositiveInfinity, "rainfallMm"),
                    (i => i.storm.initialAbstractionRatio = .05f, "initialAbstractionRatio"),
                    (i => i.storm.initialAbstractionRatio = float.NaN, "initialAbstractionRatio"),
                    (i => i.sites = null, "sites"), (i => i.sites = Array.Empty<SimulationSiteInput>(), "sites"),
                    (i => i.sites[0] = null, "site is required"), (i => i.sites[0].siteId = " ", "siteId"),
                    (i => i.sites[0].siteId = " a", "siteId"), (i => i.sites = new[] { Site(), Site() }, "unique"),
                    (i => i.sites[0].contributingAreaId = null, "contributingAreaId"),
                    (i => i.sites[0].areaM2 = -1, "areaM2"), (i => i.sites[0].areaM2 = float.NaN, "areaM2"),
                    (i => i.sites[0].areaM2 = float.PositiveInfinity, "areaM2"),
                    (i => i.sites[0].curveNumber = 0, "curveNumber"), (i => i.sites[0].curveNumber = 101, "curveNumber"),
                    (i => i.sites[0].curveNumber = float.NaN, "curveNumber"), (i => i.sites[0].curveNumber = float.NegativeInfinity, "curveNumber"),
                    (i => i.sites[0].retentionCapacityM3 = -1, "retentionCapacityM3"),
                    (i => i.sites[0].retentionCapacityM3 = float.NaN, "retentionCapacityM3"),
                    (i => i.sites[0].retentionCapacityM3 = float.PositiveInfinity, "retentionCapacityM3")
                };
                foreach (var test in invalid) { var i = Input(Site()); test.Change(i); Reject(() => RunoffEvaluator.Evaluate(i), test.Reason); }
            });
            Check("Finite inputs that overflow float outputs fail without a partial result", () => {
                var i = Input(Site(area: float.MaxValue)); i.storm.rainfallMm = float.MaxValue;
                Reject(() => RunoffEvaluator.Evaluate(i), "float contract range");
                Balance(Run(Site(capacity: float.MaxValue), Site("b", capacity: float.MaxValue, area: float.MaxValue)));
            });
            Check("Scalar formula rejects invalid public arguments and handles CN100, tiny CN", () => {
                foreach (double p in new[] { -1d, double.NaN, double.PositiveInfinity }) Reject(() => RunoffEvaluator.DirectRunoffDepthMm(p, 78), "rainfallMm");
                foreach (double cn in new[] { -1d, 0, 101, double.NaN, double.PositiveInfinity }) Reject(() => RunoffEvaluator.DirectRunoffDepthMm(50, cn), "curveNumber");
                Near(RunoffEvaluator.DirectRunoffDepthMm(1, float.Epsilon), 0, 0); Near(RunoffEvaluator.DirectRunoffDepthMm(50, 100), 50, 0);
            });
            var mapping = new Dictionary<string, float> { ["cultivated"] = 78, ["grass"] = 61, ["woodland"] = 55 };
            var cover = new[] { new CoverFraction { coverId = "cultivated", fraction = .75f }, new CoverFraction { coverId = "grass", fraction = .25f } };
            Check("Frozen area-weighted CN mixture is 73.75, distinctly not component runoff aggregation", () => {
                string before = Serialize(cover); var cn = CurveNumberMixture.AreaWeighted(cover, mapping); Near(cn, 73.75, 0);
                Near(RunoffEvaluator.DirectRunoffDepthMm(50, cn), 8.328602575579806, 1e-10);
                // Independent fixture volumes: 0.75*11.85764145695332 + 0.25*1.706343414451108 = 9.319816946327767.
                True(Math.Abs(RunoffEvaluator.DirectRunoffDepthMm(50, cn) - 9.319816946327767) > .9);
                True(before == Serialize(cover)); Array.Reverse(cover); Near(CurveNumberMixture.AreaWeighted(cover, mapping), cn, 0);
            });
            Check("Cover validation: null, missing, duplicate, invalid fractions/mappings and non-unit sum", () => {
                Reject(() => CurveNumberMixture.AreaWeighted(null, mapping), "fractions");
                Reject(() => CurveNumberMixture.AreaWeighted(cover, null), "mapping");
                Reject(() => CurveNumberMixture.AreaWeighted(Array.Empty<CoverFraction>(), mapping), "fractions");
                Reject(() => CurveNumberMixture.AreaWeighted(new CoverFraction[] { null }, mapping), "component");
                Reject(() => CurveNumberMixture.AreaWeighted(new[] { cover[0], cover[0] }, mapping), "unique");
                foreach (float f in new[] { -.1f, 1.1f, float.NaN, float.PositiveInfinity })
                    Reject(() => CurveNumberMixture.AreaWeighted(new[] { new CoverFraction { coverId = "grass", fraction = f } }, mapping), "fractions");
                Reject(() => CurveNumberMixture.AreaWeighted(new[] { new CoverFraction { coverId = "grass", fraction = .9f } }, mapping), "sum");
                Reject(() => CurveNumberMixture.AreaWeighted(new[] { new CoverFraction { coverId = "missing", fraction = 1 } }, mapping), "mapping");
                foreach (float cn in new[] { 0, 101, float.NaN, float.PositiveInfinity }) {
                    var bad = new Dictionary<string, float> { ["grass"] = cn };
                    Reject(() => CurveNumberMixture.AreaWeighted(new[] { new CoverFraction { coverId = "grass", fraction = 1 } }, bad), "mapping");
                }
            });
            Check("Current Meadowbrook recipes use frozen mixtures; unmapped sealed cover fails explicitly", () => {
                var world = JsonSerializer.Deserialize<WorldData>(File.ReadAllText(Path.Combine(root, "worlds/meadowbrook-v1/world.json")), Json);
                var scenario = JsonSerializer.Deserialize<Scenario>(File.ReadAllText(Path.Combine(root, "worlds/meadowbrook-v1/scenario.json")), Json);
                var expectedCn = new Dictionary<string, float> { ["cn-b-crop-78"] = 78, ["cn-b-margin-73-75"] = 73.75f, ["cn-b-grass-61"] = 61 };
                foreach (var treatment in scenario.treatments) Near(CurveNumberMixture.AreaWeighted(treatment.cover, mapping), expectedCn[treatment.modelParameterId], 0);
                foreach (var site in world.sites) {
                    if (site.baselineCover.Any(c => c.coverId == "sealed"))
                        Reject(() => CurveNumberMixture.AreaWeighted(site.baselineCover, mapping), "mapping for sealed");
                    else {
                        float cn = CurveNumberMixture.AreaWeighted(site.baselineCover, mapping);
                        var input = Input(Site(site.id, site.contributingAreaId, site.analysisAreaM2, cn)); input.storm = scenario.storm;
                        Balance(RunoffEvaluator.Evaluate(input));
                    }
                }
            });
            Console.WriteLine(Serialize(new { passed = true, suiteCount = Passed.Count, modelVersion = RunoffEvaluator.ModelVersion, cases = Passed, numericalToleranceM3 = .001, engine = ".NET standalone; no Unity Editor or browser used" }));
            return 0;
        } catch (Exception e) { Console.Error.WriteLine(e); return 1; }
    }
}
