using System.Text.Json;
using Terra.Chapters;
using Terra.Contracts;

var builder = WebApplication.CreateBuilder(args);
LocalEnvironment.LoadForProject(builder.Environment.ContentRootPath);
// Local prototype only. Public hosting needs authentication, per-user quotas and shared storage.
builder.WebHost.UseUrls("http://127.0.0.1:8787");
builder.WebHost.ConfigureKestrel(options => options.Limits.MaxRequestBodySize = 256 * 1024);
builder.Logging.ClearProviders(); // No prompt, save data, or credential-bearing HTTP logs.
var app = builder.Build();
var apiKey = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
var model = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-3.5-flash-lite";
var cache = Environment.GetEnvironmentVariable("TERRA_CHAPTER_CACHE") ?? Path.Combine(app.Environment.ContentRootPath, "data");
var origins = (Environment.GetEnvironmentVariable("TERRA_ALLOWED_ORIGINS") ?? "http://localhost:8080,http://127.0.0.1:8080")
    .Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);
using var http = new HttpClient { Timeout = TimeSpan.FromSeconds(25) };
var service = new ChapterService(new GeminiNarrator(http, apiKey, model), cache);
app.Use(async (context, next) => {
    // Reject DNS rebinding and cross-origin requests before they can consume provider quota.
    if (context.Request.Host.Host is not ("127.0.0.1" or "localhost")) { context.Response.StatusCode = 403; return; }
    string origin = context.Request.Headers.Origin;
    if (!string.IsNullOrEmpty(origin)) {
        if (!origins.Contains(origin, StringComparer.Ordinal)) { context.Response.StatusCode = 403; return; }
        context.Response.Headers.AccessControlAllowOrigin = origin;
        context.Response.Headers.Vary = "Origin";
        context.Response.Headers.AccessControlAllowHeaders = "Content-Type";
        context.Response.Headers.AccessControlAllowMethods = "POST, GET, OPTIONS";
    }
    context.Response.Headers.CacheControl = "no-store";
    if (context.Request.Method == "OPTIONS") { context.Response.StatusCode = 204; return; }
    await next(context);
});
app.MapGet("/health", () => Results.Json(new { status = "ready", provider = "gemini", model, configured = !string.IsNullOrWhiteSpace(apiKey) }));
app.MapPost("/chapter", async (HttpContext context) => {
    using var timeout = CancellationTokenSource.CreateLinkedTokenSource(context.RequestAborted);
    timeout.CancelAfter(TimeSpan.FromSeconds(30)); // Total across initial call and up to two repairs.
    try {
        var input = await JsonSerializer.DeserializeAsync<AnnualChapterRequest>(context.Request.Body, ChapterJson.Options, timeout.Token);
        return Results.Text(ChapterJson.Write(await service.Resolve(input, timeout.Token)), "application/json");
    }
    catch (ProviderFailure error) { return Results.Json(new { error = error.Message }, statusCode: error.StatusCode); }
    catch (OperationCanceledException) { return Results.Json(new { error = "Chapter timed out. Retry the same year or choose offline; no year was accepted." }, statusCode: 504); }
    catch (Exception error) when (error is ArgumentException || error is JsonException || error is InvalidOperationException || error is OverflowException) {
        return Results.Json(new { error = "Invalid annual input or chapter. Check the saved definition/request, then retry or choose offline." }, statusCode: 422);
    }
    catch { return Results.Json(new { error = "Chapter service unavailable. The year remains pending." }, statusCode: 503); }
});
Console.WriteLine("Terra chapter service: http://127.0.0.1:8787 (Gemini " + (string.IsNullOrWhiteSpace(apiKey) ? "key missing" : "configured") + ")");
await app.RunAsync();
