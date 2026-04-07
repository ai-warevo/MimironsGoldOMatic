// <!-- Created: 2026-04-05 (Tier B E2E harness) -->
// <!-- Updated: 2026-04-05 (Tier B integration & first run) -->
// HTTP stand-in for Desktop payout API choreography (no WPF / WinAPI).
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient();
var app = builder.Build();

var apiJson = new JsonSerializerOptions
{
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.CamelCase) },
};

var runLock = new object();
LastRunState state = new();

app.MapGet("/health", () => Results.Json(new { status = "healthy", component = "SyntheticDesktop" }));

app.MapGet("/last-run", () =>
{
    lock (runLock)
        return Results.Json(state, apiJson);
});

app.MapPost("/run-sequence", async (RunSequenceRequest body, IHttpClientFactory httpFactory, IConfiguration config, CancellationToken ct) =>
{
    var backend = config["SyntheticDesktop:BackendBaseUrl"] ?? "http://127.0.0.1:8080";
    var apiKey = config["Mgm:ApiKey"] ?? "";
    if (string.IsNullOrWhiteSpace(apiKey))
    {
        lock (runLock)
            state = LastRunState.Failed("Mgm:ApiKey is not configured.");
        return Results.Json(state, apiJson, statusCode: 500);
    }

    if (body.PayoutId == Guid.Empty || string.IsNullOrWhiteSpace(body.CharacterName))
    {
        lock (runLock)
            state = LastRunState.Failed("payoutId and characterName are required.");
        return Results.BadRequest();
    }

    using var client = httpFactory.CreateClient();
    client.BaseAddress = new Uri(backend.TrimEnd('/') + "/");
    client.DefaultRequestHeaders.Add("X-MGM-ApiKey", apiKey);

    var steps = new List<StepResult>();
    try
    {
        var confirmUrl = $"api/payouts/{body.PayoutId}/confirm-acceptance";
        var confirmPayload = JsonSerializer.Serialize(new { characterName = body.CharacterName }, apiJson);
        var r1 = await client.PostAsync(confirmUrl, new StringContent(confirmPayload, Encoding.UTF8, "application/json"), ct);
        steps.Add(new StepResult("POST confirm-acceptance", (int)r1.StatusCode, await r1.Content.ReadAsStringAsync(ct)));
        r1.EnsureSuccessStatusCode();

        var patchInProgress = JsonSerializer.Serialize(new { status = PayoutStatus.InProgress }, apiJson);
        var r2 = await client.PatchAsync($"api/payouts/{body.PayoutId}/status",
            new StringContent(patchInProgress, Encoding.UTF8, "application/json"), ct);
        steps.Add(new StepResult("PATCH status InProgress", (int)r2.StatusCode, await r2.Content.ReadAsStringAsync(ct)));
        r2.EnsureSuccessStatusCode();

        var patchSent = JsonSerializer.Serialize(new { status = PayoutStatus.Sent }, apiJson);
        var r3 = await client.PatchAsync($"api/payouts/{body.PayoutId}/status",
            new StringContent(patchSent, Encoding.UTF8, "application/json"), ct);
        steps.Add(new StepResult("PATCH status Sent", (int)r3.StatusCode, await r3.Content.ReadAsStringAsync(ct)));
        r3.EnsureSuccessStatusCode();

        lock (runLock)
            state = new LastRunState { Ok = true, Steps = steps, Error = null, CompletedAtUtc = DateTime.UtcNow };

        return Results.Json(state, apiJson);
    }
    catch (Exception ex)
    {
        lock (runLock)
            state = LastRunState.Failed(ex.Message, steps);
        return Results.Json(state, apiJson, statusCode: 502);
    }
});

app.Run();

internal sealed record RunSequenceRequest(Guid PayoutId, string CharacterName);

internal sealed record StepResult(string Name, int StatusCode, string BodySnippet);

internal sealed class LastRunState
{
    public bool Ok { get; init; }
    public List<StepResult> Steps { get; init; } = [];
    public string? Error { get; init; }
    public DateTime? CompletedAtUtc { get; init; }

    public static LastRunState Failed(string error, List<StepResult>? steps = null) => new()
    {
        Ok = false,
        Steps = steps ?? [],
        Error = error,
        CompletedAtUtc = DateTime.UtcNow,
    };
}
