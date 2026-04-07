// <!-- Updated: 2026-04-05 (Tier B integration & first run) -->
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using MimironsGoldOMatic.Backend.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace MimironsGoldOMatic.Backend.Application.Roulette.Enrollment;

/// <summary>Helix Send Chat Message (SPEC section 11) — inline retries, no Outbox.</summary>
public sealed class HelixChatService(
    IHttpClientFactory httpClientFactory,
    IOptions<TwitchOptions> twitchOptions,
    ILogger<HelixChatService> log)
{
    private readonly TwitchOptions _twitch = twitchOptions.Value;

    public async Task<bool> TrySendRewardSentAnnouncementAsync(string winnerCharacterName, CancellationToken ct)
    {
        if (string.IsNullOrEmpty(_twitch.BroadcasterAccessToken) || string.IsNullOrEmpty(_twitch.BroadcasterUserId))
        {
            log.LogWarning("Helix: missing BroadcasterAccessToken or BroadcasterUserId; skipping chat announcement.");
            return false;
        }

        var message =
            $"Награда отправлена персонажу {winnerCharacterName} на почту, проверяй ящик!";
        var client = httpClientFactory.CreateClient("Helix");
        const string relativePath = "helix/chat/messages";

        var body = JsonSerializer.Serialize(
            new
            {
                broadcaster_id = _twitch.BroadcasterUserId,
                sender_id = _twitch.BroadcasterUserId,
                message,
            },
            JsonSerializerOptions.Web);
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                using var req = new HttpRequestMessage(HttpMethod.Post, relativePath);
                req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _twitch.BroadcasterAccessToken);
                req.Headers.TryAddWithoutValidation("Client-Id", _twitch.HelixClientId);
                req.Content = new StringContent(body, Encoding.UTF8, "application/json");
                var resp = await client.SendAsync(req, ct);
                if (resp.IsSuccessStatusCode)
                    return true;
                log.LogWarning("Helix Send Chat Message attempt {Attempt} failed: {Status}", attempt, resp.StatusCode);
            }
            catch (Exception ex) when (attempt < 3)
            {
                log.LogWarning(ex, "Helix Send Chat Message attempt {Attempt} error", attempt);
            }
        }

        return false;
    }
}

