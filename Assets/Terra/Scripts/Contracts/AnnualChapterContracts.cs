using System;

namespace Terra.Contracts
{
    [Serializable] public sealed class AnnualChapterRequest
    {
        public int schemaVersion = 1;
        public AnnualTownDefinition definition;
        public AnnualTownSnapshot snapshot;
    }
    [Serializable] public sealed class AnnualChapterCard
    {
        public string title, explanation;
        public string[] factIds;
    }
    [Serializable] public sealed class AnnualChapterText
    {
        public string title, introduction;
        public AnnualChapterCard[] cards;
    }
    [Serializable] public sealed class AnnualChapterFact
    {
        public string id, text;
    }
    // Narration-only wire DTO. Unity owns/revalidates the simulation; Node never reruns it.
    // This separate envelope leaves legacy requests, receipts and save hashes unchanged.
    [Serializable] public sealed class AnnualNarrationRequest
    {
        public int schemaVersion = 1;
        public string protocol = "terra-narration-v1", rulesVersion, clientFingerprint;
        public TownYearConsequences consequences;
        public AnnualChapterFact[] facts;
    }
    [Serializable] public sealed class AnnualChapterReceipt
    {
        public int schemaVersion = 1;
        public string rulesVersion, mode, model;
        public TownYearConsequences consequences;
        public AnnualChapterText chapter;
    }
    [Serializable] public sealed class AnnualChapterSave
    {
        public int schemaVersion = 1;
        public AnnualTownSnapshot town;
        public AnnualChapterReceipt[] chapters;
    }
}
