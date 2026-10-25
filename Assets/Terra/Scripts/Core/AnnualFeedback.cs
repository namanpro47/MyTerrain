using System;
using System.Globalization;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    // Presentation-only observations. Never writes to the annual ledger or stores animations in saves.
    public enum AnnualFeedbackTone { Plan, Reserve, Release, Outcome, Decline, Mixed }

    public sealed class AnnualFeedback
    {
        public string title, detail;
        public bool accepted;
        public long availableDelta, cashDelta;
        public readonly double[] scoreDeltas = new double[3];
        public readonly double[] scores = new double[3];
        public AnnualFeedbackTone tone;
        public static string Money(long cents) => (cents < 0 ? "-" : cents > 0 ? "+" : "") + "$" + (Math.Abs((decimal)cents) / 100m).ToString("N0", CultureInfo.InvariantCulture);
        public static double DisplayScore(double value) => Math.Round(value, 1, MidpointRounding.AwayFromZero);
        public static string Points(double value) => value.ToString("+0.0;-0.0;0.0", CultureInfo.InvariantCulture);
    }

    public sealed class AnnualFeedbackTracker
    {
        string runId;
        int historyCount;
        TownPhase phase;
        long available, cash;
        double[] acceptedScores, forecastScores;
        static double[] Scores(TownYearOutcomes o) => new[] { o.economicScore, o.socialScore, o.environmentalScore }.Select(AnnualFeedback.DisplayScore).ToArray();

        public AnnualFeedback Observe(AnnualTown town, TownYearOutcomes baseline, TownYearOutcomes forecast, string cause = null)
        {
            var state = town.Snapshot();
            var current = Scores(state.history.LastOrDefault()?.outcomes ?? baseline);
            var predicted = Scores(forecast);
            AnnualFeedback result = null;
            // Loading/replay/location changes establish a new baseline, never a fake windfall.
            if (runId == state.runId && state.history.Length >= historyCount)
            {
                bool accepted = state.history.Length > historyCount;
                bool nextYear = phase == TownPhase.Reviewing && state.phase == TownPhase.Planning;
                if (accepted || cause != null || nextYear)
                {
                    result = new AnnualFeedback {
                        accepted = accepted,
                        title = accepted ? (state.history.Length - historyCount > 1 ? "Years " + (historyCount+1) + "–" + state.year + " accepted" : "Year " + state.year + " accepted") : cause ?? "Year " + state.year + " forecast",
                        availableDelta = town.AvailableCents - available,
                        cashDelta = state.cashCents - cash
                    };
                    var before = accepted ? acceptedScores : forecastScores;
                    var after = accepted ? current : predicted;
                    for (int i = 0; i < 3; i++) { result.scores[i] = after[i]; result.scoreDeltas[i] = Math.Round(after[i] - before[i], 1); }
                    string money = "Available: " + AnnualFeedback.Money(available).TrimStart('+') + " → " + AnnualFeedback.Money(town.AvailableCents).TrimStart('+') + "\nCash change: " + AnnualFeedback.Money(result.cashDelta);
                    result.detail = money + (accepted ? "\nAccepted outcomes; compare with your previous accepted year." : nextYear ? "\nNew year's conditions and project ages update the forecast. No payment made." : "\nReservation changed; cash is unchanged until End Year. Scores are year-end forecasts, not earned points.");
                    if (accepted)
                    {
                        var years = state.history.Skip(historyCount).ToArray();
                        long paid = years.Sum(r => r.ledger.upfrontPaidCents + r.ledger.operatingPaidCents + r.ledger.programPaidCents + r.ledger.repairPaidCents + r.ledger.arrearsPaidCents);
                        long income = years.Sum(r => r.ledger.appropriationCents + r.ledger.publicReceiptsCents);
                        result.detail += "\nPaid: " + AnnualFeedback.Money(-paid) + " · Income: " + AnnualFeedback.Money(income);
                    }
                    bool up = result.scoreDeltas.Any(d => d > 0), down = result.scoreDeltas.Any(d => d < 0);
                    result.tone = accepted ? (down ? (up ? AnnualFeedbackTone.Mixed : AnnualFeedbackTone.Decline) : AnnualFeedbackTone.Outcome)
                        : result.availableDelta < 0 ? AnnualFeedbackTone.Reserve : result.availableDelta > 0 ? AnnualFeedbackTone.Release : AnnualFeedbackTone.Plan;
                    if (nextYear && result.availableDelta == 0 && result.scoreDeltas.All(d => d == 0)) result = null;
                }
            }
            runId = state.runId; historyCount = state.history.Length; phase = state.phase;
            available = town.AvailableCents; cash = state.cashCents; acceptedScores = current; forecastScores = predicted;
            return result;
        }
    }
}
