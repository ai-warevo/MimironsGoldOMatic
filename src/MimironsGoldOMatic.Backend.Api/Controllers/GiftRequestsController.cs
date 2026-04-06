using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimironsGoldOMatic.Backend.Domain;

namespace MimironsGoldOMatic.Backend.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize(AuthenticationSchemes = "Bearer")]
public sealed class GiftRequestsController(IMediator mediator) : ControllerBase
{
    [HttpGet("streamers/{streamerId}/gift-queue")]
    public async Task<IActionResult> GetQueue(string streamerId, CancellationToken ct)
    {
        var uid = TwitchUserId();
        var r = await mediator.Send(new GetGiftQueueQuery(streamerId, uid), ct);
        return r.Ok ? Ok(r.Value) : StatusCode(r.StatusCode, r.Error);
    }

    [HttpPost("gift-requests")]
    public async Task<IActionResult> PostGiftRequest([FromBody] CreateGiftRequest body, CancellationToken ct)
    {
        var uid = TwitchUserId();
        if (string.IsNullOrEmpty(uid))
            return Unauthorized();
        var display = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("display_name")?.Value ?? uid;
        var r = await mediator.Send(new CreateGiftRequestCommand(uid, display, body), ct);
        if (!r.Ok)
            return StatusCode(r.StatusCode, r.Error);
        return r.StatusCode == 201 ? Created(string.Empty, r.Value) : Ok(r.Value);
    }

    private string? TwitchUserId() =>
        User.FindFirst("user_id")?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
}

