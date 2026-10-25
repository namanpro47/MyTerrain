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
