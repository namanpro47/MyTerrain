using System.Collections.Concurrent;
using System.Text.Json;
using Terra.Contracts;
using Terra.Core;

namespace Terra.Chapters;

public interface IReportNarrator
{
    string ModelVersion { get; }
    Task<ReportNarrationSelection> Select(ReportNarrationRequest report, string diagnostics, CancellationToken cancellation);
}

public sealed class ReportProviderFailure : Exception
{
    public string ReasonCode { get; }
    public ReportProviderFailure(string reasonCode) : base(ReportNarration.ReasonMessage(reasonCode)) { ReasonCode = reasonCode; }
}
public sealed class ReportNarrationConflictException() : Exception("Accepted report identity already has a different projection.");
public sealed class ReportNarrationCacheException() : Exception("The saved report explanation could not be verified or persisted.");

public sealed class ReportNarrationService(IReportNarrator narrator, string cacheDirectory, TimeSpan? providerDeadline = null)
{
    // Local prototype: serialize duplicates by receipt path, including separate service objects.
    // Independent already-accepted years may finish in any order; they never commit numeric state.
    static readonly ConcurrentDictionary<string, SemaphoreSlim> Gates = new(StringComparer.Ordinal);
    public sealed class SavedReceipt
    {
        public string requestHash;
        public ReportNarrationReceipt receipt;
    }

    public async Task<ReportNarrationReceipt> Resolve(ReportNarrationRequest input, CancellationToken cancellation)
    {
        var request = ReportNarration.CopyRequest(input); // Detach before the first await.
        var hash = ReportNarration.RequestHash(request);
        var path = Path.GetFullPath(Path.Combine(cacheDirectory, "report-v2", ReportNarration.CacheKey(request) + ".json"));
        var gate = Gates.GetOrAdd(path, _ => new SemaphoreSlim(1, 1));
        await gate.WaitAsync(cancellation);
        try
        {
            cancellation.ThrowIfCancellationRequested();
            if (File.Exists(path)) return await ReadSaved(path, request, hash, cancellation);
            ReportNarrationReceipt receipt;
            using (var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellation))
            {
                timeout.CancelAfter(providerDeadline ?? TimeSpan.FromSeconds(30));
                try
                {
                    if (narrator.ModelVersion != request.modelVersion)
                        throw new ReportProviderFailure("model-mismatch");
                    ReportNarrationSelection selection = null;
                    string diagnostics = "";
                    for (int attempt = 0; attempt < 3; attempt++)
                    {
                        try
                        {
                            selection = await narrator.Select(ReportNarration.CopyRequest(request), diagnostics, timeout.Token);
                            timeout.Token.ThrowIfCancellationRequested();
                            ReportNarration.ValidateSelection(request, selection);
                            break;
                        }
                        catch (Exception error) when (error is ArgumentException || error is JsonException)
                        {
                            selection = null;
                            diagnostics = "Return only factIds: an array of 1–3 distinct IDs from the supplied facts. No title, prose or extra fields.";
                        }
                    }
                    if (selection == null) throw new ReportProviderFailure("invalid-selection");
                    receipt = ReportNarration.BuildReceipt(request, selection);
                }
                catch (ReportProviderFailure error) { receipt = ReportNarration.Local(request, error.ReasonCode); }
                catch (OperationCanceledException) when (!cancellation.IsCancellationRequested)
                { receipt = ReportNarration.Local(request, "timeout"); }
                catch (HttpRequestException) { receipt = ReportNarration.Local(request, "network"); }
            }
            cancellation.ThrowIfCancellationRequested();
            ReportNarration.ValidateReceipt(request, receipt);
            string temporary = path + "." + Guid.NewGuid().ToString("N") + ".tmp";
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                await File.WriteAllTextAsync(temporary, ChapterJson.Write(new SavedReceipt { requestHash = hash, receipt = receipt }), cancellation);
                cancellation.ThrowIfCancellationRequested();
                try { File.Move(temporary, path); } // Publish the first AI or local receipt atomically.
                catch (IOException) when (File.Exists(path))
                { return await ReadSaved(path, request, hash, cancellation); } // Another process won; never overwrite it.
            }
            catch (OperationCanceledException) { throw; }
            catch (ReportNarrationConflictException) { throw; }
            catch (ReportNarrationCacheException) { throw; }
            catch (Exception error) when (error is IOException || error is UnauthorizedAccessException)
            { throw new ReportNarrationCacheException(); }
            finally { if (File.Exists(temporary)) File.Delete(temporary); }
            return receipt;
        }
        finally { gate.Release(); }
    }

    static async Task<ReportNarrationReceipt> ReadSaved(string path, ReportNarrationRequest request, string hash, CancellationToken cancellation)
    {
        try
        {
            var saved = ChapterJson.Read<SavedReceipt>(await File.ReadAllTextAsync(path, cancellation));
            if (saved == null || saved.receipt == null) throw new ReportNarrationCacheException();
            if (saved.requestHash != hash) throw new ReportNarrationConflictException();
            ReportNarration.ValidateReceipt(request, saved.receipt);
            return saved.receipt;
        }
        catch (Exception error) when (error is ArgumentException || error is JsonException || error is IOException || error is UnauthorizedAccessException)
        { throw new ReportNarrationCacheException(); }
    }
}
