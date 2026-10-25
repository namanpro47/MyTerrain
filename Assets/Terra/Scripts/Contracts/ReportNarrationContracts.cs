using System;

namespace Terra.Contracts
{
    // Presentation over an already saved transition. No snapshot, effects or commit command.
    [Serializable] public sealed class ReportNarrationRequest
    {
        public int schemaVersion = 2, year;
        public string acceptedTransitionId, acceptedTransitionHash, rulesVersion, reportVersion;
        public string promptVersion, modelVersion, title, introduction;
        public ReportNarrationFact[] facts;
    }
    [Serializable] public sealed class ReportNarrationFact
    {
        public string id, title, text;
        public string[] siteIds, causalRuleIds;
    }
    [Serializable] public sealed class ReportNarrationSelection
    {
        public string[] factIds;
    }
    [Serializable] public sealed class ReportNarrationCard
    {
        public string factId, title, text;
    }
    [Serializable] public sealed class ReportNarrationReceipt
    {
        public int schemaVersion = 2;
        public string acceptedTransitionId, acceptedTransitionHash, requestHash;
        public string rulesVersion, reportVersion, promptVersion, contractVersion, modelVersion;
        public string mode, reasonCode, title, introduction;
        public ReportNarrationCard[] cards;
    }
}
