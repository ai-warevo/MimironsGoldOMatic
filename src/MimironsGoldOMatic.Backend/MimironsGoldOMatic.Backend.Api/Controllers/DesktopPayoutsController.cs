using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace MimironsGoldOMatic.Backend.Api.Controllers;

[ApiController]
[Route("api/payouts")]
[Authorize(AuthenticationSchemes = "ApiKey")]
public sealed class DesktopPayoutsController(IMediator mediator) : ControllerBase
{
    [HttpGet("pending")]
    public async Task<IActionResult> Pending(CancellationToken ct)
    {
        var r = await mediator.Send(new GetPendingPayoutsQuery(), ct);
        return r.Ok ? Ok(r.Value) : StatusCode(r.StatusCode, r.Error);
    }

    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> PatchStatus(Guid id, [FromBody] PatchPayoutStatusRequest body, CancellationToken ct)
    {
        var r = await mediator.Send(new PatchPayoutStatusCommand(id, body.Status), ct);
        return r.Ok ? Ok(r.Value) : StatusCode(r.StatusCode, r.Error);
    }

    [HttpPost("{id:guid}/confirm-acceptance")]
    public async Task<IActionResult> ConfirmAcceptance(Guid id, [FromBody] ConfirmAcceptanceRequest body, CancellationToken ct)
    {
        var r = await mediator.Send(new ConfirmAcceptanceCommand(id, body.CharacterName), ct);
        return r.Ok ? Ok() : StatusCode(r.StatusCode, r.Error);
    }
}

