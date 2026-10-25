using System;

namespace Terra.Contracts
{
    // Separate from legacy keys and the annual ledger. Rules live in immutable
    // definition records, not once per entry or once per accepted year.
    [Serializable] public sealed class AnnualNumericRunDirectory
    {
        public int schemaVersion = 1, townSchemaVersion = 3;
        public long sequence;
        public string worldPackId, modelFamily, directoryHash, activeRunId;
        public string operationId, operationHash;
        public AnnualNumericRunEntry[] runs = Array.Empty<AnnualNumericRunEntry>();
    }
    [Serializable] public sealed class AnnualNumericRunEntry
    {
        public string runId, definitionHash;
    }
    [Serializable] public sealed class AnnualNumericRunContent
    {
        public AnnualTownDefinition definition;
        public AnnualNumericSave save;
    }
    [Serializable] public sealed class AnnualNumericRunSelection
    {
        public AnnualNumericRunDirectory directory;
        public AnnualTownDefinition definition;
        public AnnualNumericSave save;
    }
}
