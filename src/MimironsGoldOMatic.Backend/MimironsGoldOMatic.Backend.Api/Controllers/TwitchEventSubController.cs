using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimironsGoldOMatic.Backend.Configuration;
using MimironsGoldOMatic.Backend.Services;

namespace MimironsGoldOMatic.Backend.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/twitch/eventsub")]
public sealed class TwitchEventSubController(
    IChatEnrollmentIngest chatEnrollment,
    IOptions<TwitchOptions> twitch,
    IMediator mediator,
    ILogger<TwitchEventSubController> log)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Post(CancellationToken ct)
    {
        using var reader = new StreamReader(Request.Body);
        var body = await reader.ReadToEndAsync(ct);
        if (!VerifySignature(body))
        {
            log.LogWarning("EventSub signature verification failed");
            return Unauthorized();
        }

        using var doc = JsonDocument.Parse(string.IsNullOrEmpty(body) ? "{}" : body);
        var root = doc.RootElement;
        if (root.TryGetProperty("challenge", out var ch))
            return Content(ch.GetString() ?? "", "text/plain");

        if (!root.TryGetProperty("subscription", out var sub) ||
            sub.GetProperty("type").GetString() != "channel.chat.message")
            return Ok();

        if (!root.TryGetProperty("event", out var ev))
            return Ok();

        var messageId = ev.TryGetProperty("message_id", out var mid) ? mid.GetString() ?? "" : "";
        var chatterId = ev.TryGetProperty("chatter_user_id", out var uid) ? uid.GetString() ?? "" : "";
        var login = ev.TryGetProperty("chatter_user_login", out var lg) ? lg.GetString() ?? "" : "";
        var text = "";
        if (ev.TryGetProperty("message", out var msg) && msg.TryGetProperty("text", out var tx))
            text = tx.GetString() ?? "";

        var isSubscriber = HasSubscriberBadge(ev);
        await chatEnrollment.IngestAsync(messageId, chatterId, login, text, isSubscriber, ct);

        if (TwGiftChatParser.TryGetCharacterName(text, out var giftCharacterName))
        {
            var streamerId = ev.TryGetProperty("broadcaster_user_id", out var bid)
                ? bid.GetString() ?? twitch.Value.BroadcasterUserId
                : twitch.Value.BroadcasterUserId;
            if (!string.IsNullOrWhiteSpace(streamerId))
            {
                var r = await mediator.Send(
                    new CreateGiftRequestCommand(chatterId, login, new CreateGiftRequest(streamerId, giftCharacterName)),
                    ct);
                if (!r.Ok)
                    log.LogInformation("!twgift rejected for {UserId}: {Code}", chatterId, r.Error?.Code);
            }
        }
        return Ok();
    }

    private bool VerifySignature(string body)
    {
        var secret = twitch.Value.EventSubSecret;
        if (string.IsNullOrEmpty(secret))
            return true;

        if (!Request.Headers.TryGetValue("Twitch-Eventsub-Message-Id", out var id) ||
            !Request.Headers.TryGetValue("Twitch-Eventsub-Message-Timestamp", out var ts) ||
            !Request.Headers.TryGetValue("Twitch-Eventsub-Message-Signature", out var sig))
            return false;

        var payload = Encoding.UTF8.GetBytes(id.ToString() + ts.ToString() + body);
        using var h = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = "sha256=" + Convert.ToHexString(h.ComputeHash(payload)).ToLowerInvariant();
        return string.Equals(hash, sig.ToString(), StringComparison.Ordinal);
    }

    private static bool HasSubscriberBadge(JsonElement ev)
    {
        if (!ev.TryGetProperty("badges", out var badges))
            return false;
        foreach (var b in badges.EnumerateArray())
        {
            if (b.TryGetProperty("set_id", out var sid))
            {
                var s = sid.GetString();
                if (s is "subscriber" or "founder" or "premium")
                    return true;
            }
        }

        return false;
    }
}

