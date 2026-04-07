using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimironsGoldOMatic.Backend.Abstract;
using MimironsGoldOMatic.Backend.Domain;

namespace MimironsGoldOMatic.Backend.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize(AuthenticationSchemes = "Bearer")]
public sealed class RouletteController(IMediator mediator) : ControllerBase
{
    [HttpGet("roulette/state")]
    public async Task<IActionResult> GetState(CancellationToken ct)
    {
        var r = await mediator.Send(new GetRouletteStateQuery(), ct);
        return r.Ok ? Ok(r.Value) : StatusCode(r.StatusCode, r.Error);
    }

    [HttpGet("pool/me")]
    public async Task<IActionResult> GetPoolMe(CancellationToken ct)
    {
        var uid = TwitchUserId();
        if (string.IsNullOrEmpty(uid))
            return Unauthorized();
        var r = await mediator.Send(new GetPoolMeQuery(uid), ct);
        return r.Ok ? Ok(r.Value) : StatusCode(r.StatusCode, r.Error);
    }

    [HttpGet("payouts/my-last")]
    public async Task<IActionResult> GetMyLast(CancellationToken ct)
    {
        var uid = TwitchUserId();
        if (string.IsNullOrEmpty(uid))
            return Unauthorized();
        var r = await mediator.Send(new GetMyLastPayoutQuery(uid), ct);
        if (r.StatusCode == 404)
            return NotFound();
        return r.Ok ? Ok(r.Value) : StatusCode(r.StatusCode, r.Error);
    }

    [HttpPost("payouts/claim")]
    public async Task<IActionResult> Claim([FromBody] CreatePayoutRequest body, CancellationToken ct)
    {
        var uid = TwitchUserId();
        if (string.IsNullOrEmpty(uid))
            return Unauthorized();
        var display = User.FindFirst(ClaimTypes.Name)?.Value ?? User.FindFirst("display_name")?.Value ?? uid;
        var r = await mediator.Send(new PostClaimCommand(uid, display, body), ct);
        if (!r.Ok)
            return StatusCode(r.StatusCode, r.Error);
        return r.StatusCode == 201 ? Created(string.Empty, r.Value) : Ok(r.Value);
    }

    private string? TwitchUserId() =>
        User.FindFirst("user_id")?.Value
        ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
        ?? User.FindFirst("sub")?.Value;
}

