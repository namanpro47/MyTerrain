using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Terra.Contracts;

namespace Terra.Core
{
    // This verifies consistency with the supplied projection. The numeric adapter must derive
    // that projection from its saved accepted report; this helper cannot prove external truth.
    public static class ReportNarration
    {
        public const string ContractVersion = "report-selection-v2.0.0";
        public const string PromptVersion = "report-highlight-selection-v2.0.0";
        public const string DefaultModelVersion = "gemini-3.5-flash-lite";
        public const string AiMode = "ai-assisted", LocalMode = "local";
        static readonly Encoding Utf8 = new UTF8Encoding(false, true);
        static readonly HashSet<string> Reasons = new HashSet<string>(new[] {
            "missing-config", "network", "timeout", "quota", "refusal", "invalid-selection",
            "provider-unavailable", "service-unavailable", "model-mismatch", "local-selected"
        }, StringComparer.Ordinal);

        public static void ValidateRequest(ReportNarrationRequest request)
        {
            Require(request != null && request.schemaVersion == 2, "Expected a report-only v2 request.");
            Require(request.year >= 1 && request.year <= 10, "Accepted report year must be 1–10.");
            Identifier(request.acceptedTransitionId, 128);
            Require(IsHash(request.acceptedTransitionHash), "Accepted transition hash must be lowercase SHA-256.");
            Identifier(request.rulesVersion, 100); Identifier(request.reportVersion, 100);
            Require(request.promptVersion == PromptVersion, "Unsupported report selection prompt version.");
            Identifier(request.modelVersion, 100);
            PlainText(request.title, 100); PlainText(request.introduction, 500);
            Require(request.facts != null && request.facts.Length >= 1 && request.facts.Length <= 64,
                "Expected 1–64 canonical report facts.");
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (var fact in request.facts)
            {
                Require(fact != null, "Report fact is missing."); Identifier(fact.id, 100);
                Require(ids.Add(fact.id), "Duplicate report fact ID.");
                PlainText(fact.title, 100); PlainText(fact.text, 600);
                Identifiers(fact.siteIds); Identifiers(fact.causalRuleIds);
            }
        }

        public static ReportNarrationRequest CopyRequest(ReportNarrationRequest request)
        {
            ValidateRequest(request);
            return new ReportNarrationRequest {
                year = request.year, acceptedTransitionId = request.acceptedTransitionId,
                acceptedTransitionHash = request.acceptedTransitionHash, rulesVersion = request.rulesVersion,
                reportVersion = request.reportVersion, promptVersion = request.promptVersion,
                modelVersion = request.modelVersion, title = request.title, introduction = request.introduction,
                facts = request.facts.Select(f => new ReportNarrationFact { id = f.id, title = f.title,
                    text = f.text, siteIds = (string[])f.siteIds.Clone(), causalRuleIds = (string[])f.causalRuleIds.Clone() }).ToArray()
            };
        }

        public static string RequestHash(ReportNarrationRequest request)
        {
            ValidateRequest(request);
            return Hash(writer => {
                Write(writer, ContractVersion); writer.Write(request.schemaVersion); writer.Write(request.year);
                Write(writer, request.acceptedTransitionId); Write(writer, request.acceptedTransitionHash);
                Write(writer, request.rulesVersion); Write(writer, request.reportVersion);
                Write(writer, request.promptVersion); Write(writer, request.modelVersion);
                Write(writer, request.title); Write(writer, request.introduction); writer.Write(request.facts.Length);
                foreach (var fact in request.facts) {
                    Write(writer, fact.id); Write(writer, fact.title); Write(writer, fact.text);
                    Write(writer, fact.siteIds); Write(writer, fact.causalRuleIds);
                }
            });
        }

        public static string CacheKey(ReportNarrationRequest request)
        {
            ValidateRequest(request);
            return Hash(writer => {
                Write(writer, "report-narration-cache-v2"); Write(writer, ContractVersion);
                Write(writer, request.acceptedTransitionHash);
                Write(writer, request.promptVersion); Write(writer, request.modelVersion);
            });
        }

        public static void ValidateSelection(ReportNarrationRequest request, ReportNarrationSelection selection)
        {
            ValidateRequest(request);
            Require(selection != null && selection.factIds != null && selection.factIds.Length >= 1 && selection.factIds.Length <= 3,
                "Select 1–3 distinct supplied fact IDs.");
            var available = new HashSet<string>(request.facts.Select(f => f.id), StringComparer.Ordinal);
            var selected = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in selection.factIds)
                Require(id != null && available.Contains(id) && selected.Add(id), "Selected fact ID is unknown or repeated.");
        }

        public static ReportNarrationReceipt BuildReceipt(ReportNarrationRequest request,
            ReportNarrationSelection selection, string mode = AiMode, string reasonCode = "none")
        {
            ValidateSelection(request, selection);
            var facts = request.facts.ToDictionary(f => f.id, StringComparer.Ordinal);
            var receipt = new ReportNarrationReceipt {
                acceptedTransitionId = request.acceptedTransitionId, acceptedTransitionHash = request.acceptedTransitionHash,
                requestHash = RequestHash(request), rulesVersion = request.rulesVersion, reportVersion = request.reportVersion,
                promptVersion = request.promptVersion, contractVersion = ContractVersion, modelVersion = request.modelVersion,
                mode = mode, reasonCode = reasonCode, title = request.title, introduction = request.introduction,
                cards = selection.factIds.Select(id => new ReportNarrationCard {
                    factId = id, title = facts[id].title, text = facts[id].text }).ToArray()
            };
            ValidateReceipt(request, receipt);
            return receipt;
        }

        public static ReportNarrationReceipt Local(ReportNarrationRequest request, string reasonCode)
        {
            ValidateRequest(request);
            return BuildReceipt(request, new ReportNarrationSelection {
                factIds = request.facts.Take(3).Select(f => f.id).ToArray()
            }, LocalMode, reasonCode);
        }

        public static void ValidateReceipt(ReportNarrationRequest request, ReportNarrationReceipt receipt)
        {
            ValidateRequest(request);
            Require(receipt != null && receipt.schemaVersion == 2, "Expected a report-only v2 receipt.");
            Require(receipt.acceptedTransitionId == request.acceptedTransitionId && receipt.acceptedTransitionHash == request.acceptedTransitionHash
                && receipt.requestHash == RequestHash(request), "Narration belongs to a different accepted report.");
            Require(receipt.rulesVersion == request.rulesVersion && receipt.reportVersion == request.reportVersion
                && receipt.promptVersion == request.promptVersion && receipt.contractVersion == ContractVersion
                && receipt.modelVersion == request.modelVersion, "Narration contract/version mismatch.");
            Require((receipt.mode == AiMode && receipt.reasonCode == "none")
                || (receipt.mode == LocalMode && Reasons.Contains(receipt.reasonCode ?? "")), "Invalid narration provenance.");
            Require(receipt.title == request.title && receipt.introduction == request.introduction,
                "Narration changed the canonical report summary.");
            Require(receipt.cards != null && receipt.cards.All(c => c != null), "Narration cards are missing.");
            var selection = new ReportNarrationSelection { factIds = receipt.cards.Select(c => c.factId).ToArray() };
            ValidateSelection(request, selection);
            var facts = request.facts.ToDictionary(f => f.id, StringComparer.Ordinal);
            foreach (var card in receipt.cards)
                Require(card.title == facts[card.factId].title && card.text == facts[card.factId].text,
                    "Narration changed canonical facts, amounts or causes.");
            if (receipt.mode == LocalMode)
                Require(selection.factIds.SequenceEqual(request.facts.Take(3).Select(f => f.id)), "Local summary must preserve canonical priority order.");
        }

        public static string ModeLabel(string mode)
        {
            Require(mode == AiMode || mode == LocalMode, "Unknown narration mode.");
            return mode == AiMode ? "AI-organized explanation" : "Local summary · saved simulation results";
        }

        public static string ReasonMessage(string reason)
        {
            switch (reason) {
                case "missing-config": return "The explanation provider is not configured.";
                case "network": return "The explanation provider could not be reached.";
                case "timeout": return "The explanation request timed out.";
                case "quota": return "The explanation provider reported a quota or rate limit.";
                case "refusal": return "The explanation provider declined or returned no selection.";
                case "invalid-selection": return "The explanation did not select valid saved facts.";
                case "provider-unavailable": return "The explanation provider is unavailable.";
                case "service-unavailable": return "The explanation service is unavailable.";
                case "model-mismatch": return "The explanation service uses a different model contract.";
                case "local-selected": return "A local summary was selected.";
                default: throw new ArgumentException("Unknown narration reason.");
            }
        }

        static bool IsHash(string value) => value != null && value.Length == 64 && value.All(c => (c >= '0' && c <= '9') || (c >= 'a' && c <= 'f'));
        static void Identifier(string value, int limit)
        {
            Require(!string.IsNullOrEmpty(value) && value.Length <= limit && value.All(c =>
                (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z') || (c >= '0' && c <= '9') || "_-.:/".Contains(c)), "Invalid report identifier.");
        }
        static void Identifiers(string[] ids)
        {
            Require(ids != null && ids.Length <= 8, "Expected at most eight explicit report links.");
            var unique = new HashSet<string>(StringComparer.Ordinal);
            foreach (var id in ids) { Identifier(id, 100); Require(unique.Add(id), "Duplicate report link."); }
        }
        static void PlainText(string value, int limit)
        {
            Require(!string.IsNullOrWhiteSpace(value) && value.Length <= limit && !value.Contains("<")
                && value.All(c => !char.IsControl(c) || c == '\n'), "Invalid canonical report text.");
            Utf8.GetByteCount(value); // Reject unpaired UTF-16 surrogates consistently before hashing.
        }
        static void Write(BinaryWriter writer, string value) { var bytes = Utf8.GetBytes(value); writer.Write(bytes.Length); writer.Write(bytes); }
        static void Write(BinaryWriter writer, string[] values) { writer.Write(values.Length); foreach (var value in values) Write(writer, value); }
        static string Hash(Action<BinaryWriter> write)
        {
            using (var stream = new MemoryStream()) {
                using (var writer = new BinaryWriter(stream, Utf8, true)) { write(writer); writer.Flush(); }
                using (var sha = SHA256.Create()) return BitConverter.ToString(sha.ComputeHash(stream.ToArray())).Replace("-", "").ToLowerInvariant();
            }
        }
        static void Require(bool condition, string message) { if (!condition) throw new ArgumentException(message); }
    }
}
