using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimironsGoldOMatic.Backend.Domain;

namespace MimironsGoldOMatic.Backend.Api.Controllers;

[ApiController]
[Route("api/gift-requests")]
[Authorize(AuthenticationSchemes = "ApiKey")]
public sealed class DesktopGiftRequestsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get([FromQuery] string? streamerId, CancellationToken ct)
    {
        var r = await mediator.Send(new GetGiftQueueQuery(streamerId, null), ct);
        return r.Ok ? Ok(r.Value) : StatusCode(r.StatusCode, r.Error);
    }

    [HttpPatch("{id:guid}")]
    public async Task<IActionResult> Patch(Guid id, [FromBody] PatchGiftRequestState body, CancellationToken ct)
    {
        var r = await mediator.Send(new PatchGiftRequestCommand(id, body.State, body.Reason), ct);
        return r.Ok ? Ok(r.Value) : StatusCode(r.StatusCode, r.Error);
    }

    [HttpPost("{id:guid}/select-item")]
    public async Task<IActionResult> SelectItem(Guid id, [FromBody] SelectGiftItemRequest body, CancellationToken ct)
    {
        var r = await mediator.Send(new SelectGiftItemCommand(id, body.Item), ct);
        return r.Ok ? Ok(r.Value) : StatusCode(r.StatusCode, r.Error);
    }

    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id, [FromBody] ConfirmGiftRequest body, CancellationToken ct)
    {
        var r = await mediator.Send(new ConfirmGiftCommand(id, body.Confirmed), ct);
        return r.Ok ? Ok(r.Value) : StatusCode(r.StatusCode, r.Error);
    }
}

