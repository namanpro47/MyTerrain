using System;
using System.Collections.Generic;
using System.Linq;
using Terra.Contracts;

namespace Terra.Core
{
    // Chapter v1 narrates the existing authored model. No vetted causal pack exists for
    // additional AI effects yet: accepting arbitrary in-range effects would invent science.
    public static class AnnualChapters
    {
        public const string RulesVersion = "annual-chapter-v1";
        public const string Notice = "Illustrative annual scenario, not a forecast. AI explains the saved results; authored rules determine outcomes.";

        // Rebuild the original frozen request from accepted evidence, for an optional
        // explanation after fast-forward/reload. This never reopens the live year.
        public static AnnualChapterRequest AcceptedRequest(AnnualTown town)
        {
            Require(town.PlayerRulesActive&&town.History.Length>0,"An accepted player-rules year is required.");
            var report=town.History.Last();
            var snapshot=town.Snapshot();
            snapshot.history=snapshot.history.Take(snapshot.history.Length-1).ToArray();
            snapshot.year=report.year;snapshot.revision=report.acceptedRequest.revision;snapshot.phase=TownPhase.Resolving;
            snapshot.cashCents=report.ledger.openingCashCents;snapshot.arrearsCents=report.ledger.openingArrearsCents;
            snapshot.assets=TownCopies.Copy(report.acceptedRequest.assets);snapshot.queue=TownCopies.Copy(report.acceptedRequest.actions);
            snapshot.undo=Array.Empty<TownPlanSnapshot>();snapshot.redo=Array.Empty<TownPlanSnapshot>();
            snapshot.pendingRequest=TownCopies.Copy(report.acceptedRequest);
            AnnualTown.Restore(town.Definition,snapshot);
            return new AnnualChapterRequest {definition=town.Definition,snapshot=snapshot};
        }

        public static AnnualTown Preview(AnnualChapterRequest input)
        {
            Require(input != null && input.schemaVersion == 1 && input.definition != null && input.snapshot != null,
                "A versioned definition and frozen snapshot are required.");
            Require(input.definition.schemaVersion<=2&&input.snapshot.schemaVersion==1,"Numeric-v4 years use accepted-report narration, never the legacy resolving endpoint.");
            Require(input.snapshot.phase == TownPhase.Resolving && input.snapshot.pendingRequest != null,
                "Submit the year before requesting its chapter.");
            var town = AnnualTown.Restore(input.definition, input.snapshot);
            town.CompleteResolution(new AuthoredAnnualConsequenceProvider().Resolve(town.PendingRequest));
            return town;
        }

        public static AnnualChapterFact[] Facts(AnnualTown town)
        {
            var summary = AnnualTownSummary.Latest(town);
            Require(summary != null, "A completed year is required.");
            var facts = new List<AnnualChapterFact> {
                Fact("year", "Year " + summary.year + " of " + town.Definition.years + ". " + summary.headline
                    + (summary.finalYear ? " This is the final year of the run." : "")), Fact("weather", summary.scenario),
                Fact("economic", Score(summary.economic) + summary.economic.evidence + " " + summary.economic.rubric),
                Fact("social", Score(summary.social) + summary.social.evidence + " " + summary.social.rubric),
                Fact("environmental", Score(summary.environmental) + summary.environmental.evidence + " " + summary.environmental.rubric),
                Fact("budget", "USD: spent " + Dollars(summary.spentCents) + "; received " + Dollars(summary.receivedCents)
                    + "; closing cash " + Dollars(summary.ledger.closingCashCents) + "; future obligations " + Dollars(summary.ledger.futureCommitmentsCents) + "."
                    +(town.PlayerRulesActive?" Unfinished capital refunded "+Dollars(summary.ledger.capitalRefundCents)+". Negative cash blocks new construction until repaid; public funding and eligible receipts repay it automatically. Cancellation frees land next year without restoring its cover.":"")),
                Fact("limits", Notice + " " + town.Definition.assumptions)
            };
            var rows = town.History.Last().outcomes.sites;
            if(town.PlayerRulesActive)
            {
                var history=summary.finalYear?town.History:town.History.Skip(Math.Max(0,town.History.Length-3)).ToArray();
                facts[0].text+=" Accepted history "+(summary.finalYear?"(all ten years)":"(latest three years)")+": "+string.Join("; ",history.Select(r=>
                    "Y"+r.year+" choices "+(r.acceptedRequest.actions.Length==0?"wait":string.Join(",",r.acceptedRequest.actions.GroupBy(a=>a.actionId).Take(3).Select(g=>g.Key+" x"+g.Count())))
                    +"; cash "+Dollars(r.ledger.closingCashCents)+"; E/S/Env "+r.outcomes.economicScore.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)+"/"+r.outcomes.socialScore.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)+"/"+r.outcomes.environmentalScore.ToString("0.0",System.Globalization.CultureInfo.InvariantCulture)))+". Choice groups are abbreviated to three per year; complete history is saved.";
                facts[1].text+=" Potential regional disruptions (inferred annual game chances, not measured disaster or site-damage probabilities): "+string.Join("; ",town.Definition.playerRules.hazards.Select(h=>h.title+" "+(h.annualChance*100).ToString("0.#",System.Globalization.CultureInfo.InvariantCulture)+"%"))+". Only the saved events above occurred this year.";
            }
            for (int i = 0; i < rows.Length; i++) facts.Add(Fact("site:" + rows[i].siteId, summary.siteChanges[i]));
            return facts.ToArray();
        }

        public static void ValidateText(AnnualChapterText chapter, AnnualChapterFact[] facts)
        {
            Require(chapter != null, "Chapter text is missing.");
            Text(chapter.title, 100); Text(chapter.introduction, 500);
            Require(chapter.cards != null && chapter.cards.Length >= 1 && chapter.cards.Length <= 3, "Return one to three cards.");
            var ids = new HashSet<string>(facts.Select(f => f.id), StringComparer.Ordinal);
            foreach (var card in chapter.cards)
            {
                Require(card != null, "Null chapter card."); Text(card.title, 100); Text(card.explanation, 600);
                Require(card.factIds != null && card.factIds.Length >= 1 && card.factIds.Length <= 4
                    && card.factIds.Distinct().Count() == card.factIds.Length && card.factIds.All(ids.Contains),
                    "Each card must reference one to four distinct supplied fact IDs.");
            }
        }

        public static AnnualTown ValidateReceipt(AnnualChapterRequest input, AnnualChapterReceipt receipt)
        {
            Require(receipt != null && receipt.schemaVersion == 1 && receipt.rulesVersion == RulesVersion
                && (receipt.mode == "gemini" || receipt.mode == "offline") && !string.IsNullOrWhiteSpace(receipt.model), "Unsupported chapter receipt.");
            var expected = Preview(input);
            Require(TownCopies.Hash(receipt.consequences) == TownCopies.Hash(expected.History.Last().acceptedConsequences),
                "Chapter v1 cannot change engine outcomes or the frozen request.");
            ValidateText(receipt.chapter, Facts(expected));
            return expected;
        }

        public static AnnualNarrationRequest NarrationRequest(AnnualChapterRequest input)
        {
            var expected = Preview(input);
            return new AnnualNarrationRequest {
                rulesVersion = RulesVersion, clientFingerprint = Fingerprint(input),
                consequences = expected.History.Last().acceptedConsequences,
                facts = NarrationFacts(Facts(expected), input.snapshot.pendingRequest)
            };
        }

        public static AnnualChapterFact[] NarrationFacts(AnnualChapterFact[] facts, TownYearRequest pending)
        {
            if (facts.Length <= 64) return facts;
            var byId = facts.ToDictionary(f => f.id, StringComparer.Ordinal);
            var priority = pending.actions.Select(a => "site:" + a.siteId)
                .Concat(pending.assets.OrderByDescending(a => a.startedYear).Select(a => "site:" + a.siteId));
            var selected = facts.Take(7).Concat(priority.Where(byId.ContainsKey).Select(id => byId[id]))
                .Concat(facts.Skip(7)).GroupBy(f => f.id, StringComparer.Ordinal).Select(g => g.First()).Take(64)
                .Select(f => new AnnualChapterFact { id = f.id, text = f.text }).ToArray();
            selected.First(f => f.id == "limits").text += " Narrative detail covers 57 of " + (facts.Length - 7)
                + " sites, prioritizing this year's choices and prior projects. Aggregate totals include every site.";
            return selected;
        }

        public static AnnualChapterReceipt Offline(AnnualChapterRequest input) => OfflineAccepted(Preview(input));

        public static AnnualChapterReceipt OfflineAccepted(AnnualTown town)
        {
            var summary = AnnualTownSummary.Latest(town);
            return new AnnualChapterReceipt {
                rulesVersion = RulesVersion, mode = "offline", model = "authored-offline-v2",
                consequences = town.History.Last().acceptedConsequences,
                chapter = new AnnualChapterText {
                    title = "Year " + summary.year + (summary.finalYear ? " — your ten-year future" : " — the next chapter"),
                    introduction = "Offline chapter. " + summary.headline,
                    cards = new[] {
                        Card("Public budget", "Spent " + Dollars(summary.spentCents) + "; received " + Dollars(summary.receivedCents)
                            + " including the annual appropriation. Closing cash " + Dollars(summary.ledger.closingCashCents)
                            + "; future obligations " + Dollars(summary.ledger.futureCommitmentsCents) + "."
                            +(town.PlayerRulesActive?" Unfinished capital refunded "+Dollars(summary.ledger.capitalRefundCents)+". Negative cash blocks new construction until funding repays it.":" Accepted payments cannot be undone."), "budget"),
                        Card("People and livelihoods", Score(summary.economic) + summary.economic.evidence + "\n\n"
                            + Score(summary.social) + summary.social.evidence, "economic", "social"),
                        Card("The land and this year's conditions", Score(summary.environmental) + summary.environmental.evidence
                            + "\n\n" + summary.scenario, "environmental", "weather")
                    }
                }
            };
        }

        public static string Fingerprint(AnnualChapterRequest input) => TownCopies.Hash(input);
        public static string RequestKey(AnnualChapterRequest input) => TownCopies.Hash(new[] { input.snapshot.runId, input.snapshot.pendingRequest.requestId });
        static AnnualChapterFact Fact(string id, string text) => new AnnualChapterFact { id = id, text = text };
        static string Dollars(long cents) => (cents / 100m).ToString("C2", System.Globalization.CultureInfo.GetCultureInfo("en-US"));
        static string Score(AnnualOutcomeSummary value) => value.label + " " + value.score.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)
            + " / 100" + (value.hasPreviousYear ? " (" + value.change.ToString("+0.0;-0.0;0.0", System.Globalization.CultureInfo.InvariantCulture) + " vs last year)" : "") + ". ";
        static AnnualChapterCard Card(string title, string text, params string[] facts) => new AnnualChapterCard { title = title, explanation = text, factIds = facts };
        static void Text(string text, int limit) => Require(!string.IsNullOrWhiteSpace(text) && text.Length <= limit
            && !text.Contains("<") && !text.Contains(">") && !text.Any(c => char.IsControl(c) && c != '\n'), "Text is missing, too long, or contains markup/control characters.");
        static void Require(bool valid, string message) { if (!valid) throw new ArgumentException(message); }
    }
}
