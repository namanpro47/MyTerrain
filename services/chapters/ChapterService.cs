using System.Text.Json;
using System.Text.Json.Serialization;
using Terra.Contracts;
using Terra.Core;

namespace Terra.Chapters;

public static class ChapterJson
{
    public static readonly JsonSerializerOptions Options = new() {
        IncludeFields = true, PropertyNameCaseInsensitive = false,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, MaxDepth = 40
    };
    public static string Write<T>(T value) => JsonSerializer.Serialize(value, Options);
    public static T Read<T>(string json) => JsonSerializer.Deserialize<T>(json, Options);
}

public interface IChapterNarrator
{
    string Model { get; }
    Task<AnnualChapterText> Generate(AnnualChapterFact[] facts, string diagnostics, CancellationToken cancellation);
}

public sealed class ChapterService(IChapterNarrator narrator, string cacheDirectory)
{
    // A deliberately single-process local service. One owner prevents concurrent rerolls.
    readonly SemaphoreSlim gate = new(1, 1);
    public sealed class SavedReceipt
    {
        public string fingerprint;
        public AnnualChapterReceipt receipt;
    }

    public async Task<AnnualChapterReceipt> Resolve(AnnualChapterRequest input, CancellationToken cancellation)
    {
        var preview = AnnualChapters.Preview(input); // Replay/validate before spending API quota.
        var fingerprint = AnnualChapters.Fingerprint(input);
        var path = Path.Combine(cacheDirectory, AnnualChapters.RequestKey(input) + ".json");
        await gate.WaitAsync(cancellation);
        try
        {
            if (File.Exists(path))
            {
                var saved = ChapterJson.Read<SavedReceipt>(await File.ReadAllTextAsync(path, cancellation));
                if (saved.fingerprint != fingerprint) throw new ArgumentException("Request ID already belongs to a different frozen input.");
                AnnualChapters.ValidateReceipt(input, saved.receipt);
                return saved.receipt;
            }
            var facts = AnnualChapters.Facts(preview);
            AnnualChapterText chapter = null;
            string diagnostics = "";
            for (int attempt = 0; attempt < 3; attempt++)
            {
                try
                {
                    chapter = await narrator.Generate(facts, diagnostics, cancellation);
                    AnnualChapters.ValidateText(chapter, facts);
                    break;
                }
                catch (Exception error) when (error is ArgumentException || error is JsonException)
                {
                    chapter = null;
                    diagnostics = error is JsonException ? "Return only the required JSON fields with correct types." : error.Message;
                }
            }
            if (chapter == null) throw new InvalidOperationException("The chapter was invalid after two repair attempts. Retry or choose the offline chapter.");
            var receipt = new AnnualChapterReceipt {
                rulesVersion = AnnualChapters.RulesVersion, mode = "gemini", model = narrator.Model,
                consequences = preview.History.Last().acceptedConsequences, chapter = chapter
            };
            AnnualChapters.ValidateReceipt(input, receipt);
            Directory.CreateDirectory(cacheDirectory);
            var temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                await File.WriteAllTextAsync(temporary, ChapterJson.Write(new SavedReceipt { fingerprint = fingerprint, receipt = receipt }), cancellation);
                File.Move(temporary, path); // Durable receipt before sending; never replace first success.
            }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
            return receipt;
        }
        finally { gate.Release(); }
    }
}
