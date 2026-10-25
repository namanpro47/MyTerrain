using System;
using System.Globalization;
using Newtonsoft.Json;
using Terra.Contracts;

namespace Terra.Gameplay
{
    // Exact DTO transport. JsonUtility can change a double by one ULP while
    // parsing decimal JSON; accepted hashes require exact values across runtimes.
    // Unity dependency: com.unity.nuget.newtonsoft-json@3.2.2 (Json.NET 13.0.2).
    public static class AnnualNumericJson
    {
        const int MaximumCharacters = 64 * 1024 * 1024;
        static JsonSerializerSettings Settings() => new JsonSerializerSettings {
            TypeNameHandling = TypeNameHandling.None,
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            Culture = CultureInfo.InvariantCulture,
            DateParseHandling = DateParseHandling.None,
            FloatParseHandling = FloatParseHandling.Double,
            ObjectCreationHandling = ObjectCreationHandling.Replace,
            NullValueHandling = NullValueHandling.Include,
            MissingMemberHandling = MissingMemberHandling.Error,
            CheckAdditionalContent = true,
            MaxDepth = 64
        };
        public static AnnualTownDefinition ReadTownDefinition(string json) => Read<AnnualTownDefinition>(json);
        public static AnnualTownSnapshot ReadTownSave(string json) => Read<AnnualTownSnapshot>(json);
        public static AnnualChapterSave ReadChapterSave(string json) => Read<AnnualChapterSave>(json);
        public static AnnualChapterReceipt ReadChapterReceipt(string json) => Read<AnnualChapterReceipt>(json);
        public static AnnualNumericSave ReadSave(string json) => Read<AnnualNumericSave>(json);
        public static AnnualTownDefinition ReadDefinition(string json)
        {
            var definition = Read<AnnualTownDefinition>(json);
            if (definition.schemaVersion != 3) throw new ArgumentException("Numeric codec requires a schema-3 definition.");
            return definition;
        }
        public static string WriteSave(AnnualNumericSave value) => Write(value);
        public static string WriteDefinition(AnnualTownDefinition value)
        {
            if (value == null || value.schemaVersion != 3) throw new ArgumentException("Numeric codec requires a schema-3 definition.");
            return Write(value);
        }
        static T Read<T>(string json) where T : class
        {
            if (string.IsNullOrWhiteSpace(json) || json.Length > MaximumCharacters)
                throw new ArgumentException("Numeric JSON is missing or exceeds the supported size.");
            return JsonConvert.DeserializeObject<T>(json, Settings())
                ?? throw new ArgumentException("Numeric JSON must contain its expected DTO.");
        }
        static string Write<T>(T value) where T : class
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            string json = JsonConvert.SerializeObject(value, Formatting.None, Settings());
            if (json.Length > MaximumCharacters) throw new ArgumentException("Numeric JSON exceeds the supported size.");
            return json;
        }
    }
}
