using System;
using System.Collections.Generic;
using Terra.Contracts;

namespace Terra.Simulation
{
    /// <summary>Frozen v1 rule: weight CN by cover area, then evaluate one CN per site.</summary>
    public static class CurveNumberMixture
    {
        public static float AreaWeighted(CoverFraction[] cover, IReadOnlyDictionary<string, float> curveNumbers)
        {
            RunoffEvaluator.Need(cover != null && cover.Length > 0, "cover fractions are required.");
            RunoffEvaluator.Need(curveNumbers != null, "curve-number mapping is required.");
            var ordered = (CoverFraction[])cover.Clone();
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var part in ordered)
            {
                RunoffEvaluator.Need(part != null, "cover component is required.");
                RunoffEvaluator.Need(RunoffEvaluator.Id(part.coverId) && ids.Add(part.coverId), "cover IDs must be present and unique.");
                RunoffEvaluator.Need(RunoffEvaluator.Finite(part.fraction) && part.fraction >= 0 && part.fraction <= 1, "cover fractions must be finite in [0, 1].");
                RunoffEvaluator.Need(curveNumbers.TryGetValue(part.coverId, out var cn) && RunoffEvaluator.Finite(cn) && cn > 0 && cn <= 100, "Missing or invalid CN mapping for " + part.coverId + ".");
            }
            Array.Sort(ordered, (a, b) => StringComparer.Ordinal.Compare(a.coverId, b.coverId));
            double total = 0, weighted = 0;
            foreach (var part in ordered) { total += part.fraction; weighted += part.fraction * (double)curveNumbers[part.coverId]; }
            RunoffEvaluator.Need(Math.Abs(total - 1d) < .0001d, "cover fractions must sum to one (tolerance 0.0001).");
            // Normalize only accepted float rounding so CN remains within its component range.
            return (float)(weighted / total);
        }
    }
}
