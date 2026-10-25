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

        public static AnnualTown Preview(AnnualChapterRequest input)
        {
            Require(input != null && input.schemaVersion == 1 && input.definition != null && input.snapshot != null,
                "A versioned definition and frozen snapshot are required.");
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
                Fact("year", summary.headline), Fact("weather", summary.scenario),
                Fact("economic", summary.economic.evidence + " " + summary.economic.rubric),
                Fact("social", summary.social.evidence + " " + summary.social.rubric),
                Fact("environmental", summary.environmental.evidence + " " + summary.environmental.rubric),
                Fact("budget", "USD cents: spent " + summary.spentCents + "; received " + summary.receivedCents
                    + "; closing cash " + summary.ledger.closingCashCents + "; future obligations " + summary.ledger.futureCommitmentsCents + "."),
                Fact("limits", Notice + " " + town.Definition.assumptions)
            };
            var rows = town.History.Last().outcomes.sites;
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

        public static AnnualChapterReceipt Offline(AnnualChapterRequest input)
        {
            var town = Preview(input);
            var summary = AnnualTownSummary.Latest(town);
            return new AnnualChapterReceipt {
                rulesVersion = RulesVersion, mode = "offline", model = "authored-offline-v1",
                consequences = town.History.Last().acceptedConsequences,
                chapter = new AnnualChapterText {
                    title = "Year " + summary.year + (summary.finalYear ? " — your ten-year future" : " — the next chapter"),
                    introduction = "Offline chapter. " + summary.headline,
                    cards = new[] {
                        Card("Public budget", "Public spending and future obligations follow the accepted ledger.", "budget"),
                        Card("Life in the valley", summary.economic.evidence, "economic"),
                        Card("The land", summary.environmental.evidence, "environmental")
                    }
                }
            };
        }

        public static string Fingerprint(AnnualChapterRequest input) => TownCopies.Hash(input);
        public static string RequestKey(AnnualChapterRequest input) => TownCopies.Hash(new[] { input.snapshot.runId, input.snapshot.pendingRequest.requestId });
        static AnnualChapterFact Fact(string id, string text) => new AnnualChapterFact { id = id, text = text };
        static AnnualChapterCard Card(string title, string text, string fact) => new AnnualChapterCard { title = title, explanation = text, factIds = new[] { fact } };
        static void Text(string text, int limit) => Require(!string.IsNullOrWhiteSpace(text) && text.Length <= limit
            && !text.Contains("<") && !text.Contains(">") && !text.Any(c => char.IsControl(c) && c != '\n'), "Text is missing, too long, or contains markup/control characters.");
        static void Require(bool valid, string message) { if (!valid) throw new ArgumentException(message); }
    }
}
