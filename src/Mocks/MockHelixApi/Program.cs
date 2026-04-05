// <!-- Created: 2026-04-05 (Tier B E2E mock) -->
// <!-- Updated: 2026-04-05 (Tier B integration & first run) -->
// Loopback stub for Twitch Helix Send Chat Message — records POST /helix/chat/messages for CI assertions.
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

var lastLock = new object();
JsonDocument? lastBody = null;
string? lastAuthBearer = null;
string? lastClientId = null;

app.MapGet("/health", () => Results.Json(new { status = "healthy", component = "MockHelixApi" }));

app.MapPost("/helix/chat/messages", async (HttpRequest req, IConfiguration config, CancellationToken ct) =>
{
    var strict = string.Equals(config["MockHelix:StrictAuth"], "true", StringComparison.OrdinalIgnoreCase);
    if (strict)
    {
        if (!req.Headers.TryGetValue("Authorization", out var auth) ||
            !auth.ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return Results.Unauthorized();
        if (!req.Headers.ContainsKey("Client-Id"))
            return Results.Unauthorized();
    }

    lastAuthBearer = req.Headers.Authorization.ToString();
    req.Headers.TryGetValue("Client-Id", out var cid);
    lastClientId = cid.ToString();

    using var doc = await JsonDocument.ParseAsync(req.Body, cancellationToken: ct);
    lock (lastLock)
    {
        lastBody?.Dispose();
        lastBody = JsonDocument.Parse(doc.RootElement.GetRawText());
    }

    // Helix may return 204; Backend treats any success status as OK.
    return Results.NoContent();
});

app.MapGet("/last-request", () =>
{
    lock (lastLock)
    {
        if (lastBody == null)
            return Results.Json(new { captured = false });

        return Results.Json(new
        {
            captured = true,
            body = JsonSerializer.Deserialize<object>(lastBody.RootElement.GetRawText()),
            authorizationHeader = lastAuthBearer,
            clientIdHeader = lastClientId,
        });
    }
});

app.Run();
