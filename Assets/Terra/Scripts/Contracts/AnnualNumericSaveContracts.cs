using System;

namespace Terra.Contracts
{
    // A separate save envelope; never load it through legacy chapter-save handling.
    [Serializable] public sealed class AnnualNumericSave
    {
        public int schemaVersion = 1;
        public long sequence;
        public string saveHash;
        public AnnualTownSnapshot town;
        public AnnualNumericNarration[] narration = Array.Empty<AnnualNumericNarration>();
    }
    [Serializable] public sealed class AnnualNumericNarration
    {
        public string status; // unprojected, pending or visible; position binds to accepted history.
        public ReportNarrationRequest request;
        public ReportNarrationReceipt receipt;
    }
}
