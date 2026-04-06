using System.Net.Http.Headers;
using System.Text.Json;
using MimironsGoldOMatic.Backend.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MimironsGoldOMatic.Backend.Services;

public sealed class HelixSubscriberVerifier(
    IHttpClientFactory httpClientFactory,
    IOptions<TwitchOptions> twitchOptions,
    ILogger<HelixSubscriberVerifier> log)
    : ITwitchSubscriberVerifier
{
    private readonly TwitchOptions _twitch = twitchOptions.Value;

    public async Task<bool> IsSubscriberAsync(string streamerId, string viewerId, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(streamerId) || string.IsNullOrEmpty(viewerId))
            return false;
        if (string.IsNullOrEmpty(_twitch.BroadcasterAccessToken) || string.IsNullOrEmpty(_twitch.HelixClientId))
            return false;

        try
        {
            var client = httpClientFactory.CreateClient("Helix");
            using var req = new HttpRequestMessage(HttpMethod.Get,
                $"helix/subscriptions/user?broadcaster_id={Uri.EscapeDataString(streamerId)}&user_id={Uri.EscapeDataString(viewerId)}");
            req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _twitch.BroadcasterAccessToken);
            req.Headers.TryAddWithoutValidation("Client-Id", _twitch.HelixClientId);
            using var resp = await client.SendAsync(req, ct);
            if (!resp.IsSuccessStatusCode)
            {
                log.LogWarning("Helix subscriber check failed: {Status}", resp.StatusCode);
                return false;
            }

            await using var body = await resp.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(body, cancellationToken: ct);
            if (!doc.RootElement.TryGetProperty("data", out var arr) || arr.ValueKind != JsonValueKind.Array)
                return false;
            return arr.GetArrayLength() > 0;
        }
        catch (Exception ex)
        {
            log.LogWarning(ex, "Helix subscriber check exception");
            return false;
        }
    }
}

