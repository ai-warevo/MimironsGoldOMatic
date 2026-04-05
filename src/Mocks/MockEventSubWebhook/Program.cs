// <!-- Created: 2026-04-05 (Tier A E2E mocks) -->
// Simulates the Twitch EventSub delivery edge: verifies HMAC (same rules as EBS), logs, then forwards the raw request to the real Backend.
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpClient("Ebs", c => c.Timeout = TimeSpan.FromSeconds(60));

var app = builder.Build();

var backendBase = builder.Configuration["Backend:BaseUrl"] ?? "http://127.0.0.1:8080";
var eventSubSecret = builder.Configuration["Twitch:EventSubSecret"] ?? "";

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "MockEventSubWebhook" }));

/// <summary>Receives a Twitch-shaped EventSub POST (e.g. channel.chat.message), verifies signature, logs, forwards to EBS <c>POST /api/twitch/eventsub</c>.</summary>
app.MapPost("/api/twitch/eventsub", async (HttpRequest req, IHttpClientFactory httpFactory, ILoggerFactory loggerFactory, CancellationToken ct) =>
{
    var log = loggerFactory.CreateLogger("MockEventSubWebhook");
    using var reader = new StreamReader(req.Body);
    var body = await reader.ReadToEndAsync(ct);

    if (!VerifyEventSubSignature(req, body, eventSubSecret))
    {
        log.LogWarning("EventSub HMAC verification failed");
        return Results.Unauthorized();
    }

    log.LogInformation("EventSub payload accepted ({Bytes} bytes), forwarding to EBS", body.Length);

    var client = httpFactory.CreateClient("Ebs");
    var target = $"{backendBase.TrimEnd('/')}/api/twitch/eventsub";
    using var forward = new HttpRequestMessage(HttpMethod.Post, target);
    forward.Content = new StringContent(body, Encoding.UTF8, new MediaTypeHeaderValue("application/json"));

    foreach (var headerName in new[] { "Twitch-Eventsub-Message-Id", "Twitch-Eventsub-Message-Timestamp", "Twitch-Eventsub-Message-Signature" })
    {
        if (req.Headers.TryGetValue(headerName, out var values))
            forward.Headers.TryAddWithoutValidation(headerName, values.ToArray());
    }

    using var resp = await client.SendAsync(forward, HttpCompletionOption.ResponseHeadersRead, ct);
    var responseBody = await resp.Content.ReadAsStringAsync(ct);
    var mediaType = resp.Content.Headers.ContentType?.MediaType ?? "application/json";
    log.LogInformation("EBS responded {StatusCode}", (int)resp.StatusCode);
    return Results.Text(responseBody, mediaType, Encoding.UTF8, (int)resp.StatusCode);
});

app.Run();

static bool VerifyEventSubSignature(HttpRequest req, string body, string secret)
{
    if (string.IsNullOrEmpty(secret))
        return true;

    if (!req.Headers.TryGetValue("Twitch-Eventsub-Message-Id", out var id) ||
        !req.Headers.TryGetValue("Twitch-Eventsub-Message-Timestamp", out var ts) ||
        !req.Headers.TryGetValue("Twitch-Eventsub-Message-Signature", out var sig))
        return false;

    var payload = Encoding.UTF8.GetBytes(id.ToString() + ts.ToString() + body);
    using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
    var hash = "sha256=" + Convert.ToHexString(h.ComputeHash(payload)).ToLowerInvariant();
    return string.Equals(hash, sig.ToString(), StringComparison.Ordinal);
}
