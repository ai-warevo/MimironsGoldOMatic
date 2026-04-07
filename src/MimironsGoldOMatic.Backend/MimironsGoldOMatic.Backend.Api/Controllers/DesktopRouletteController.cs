using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MimironsGoldOMatic.Backend.Domain;
using MimironsGoldOMatic.Shared;

namespace MimironsGoldOMatic.Backend.Api.Controllers;

[ApiController]
[Route("api/roulette")]
[Authorize(AuthenticationSchemes = "ApiKey")]
public sealed class DesktopRouletteController(IMediator mediator) : ControllerBase
{
    [HttpPost("verify-candidate")]
    public async Task<IActionResult> VerifyCandidate([FromBody] VerifyCandidateRequest body, CancellationToken ct)
    {
        var r = await mediator.Send(new VerifyCandidateCommand(body), ct);
        return r.Ok ? Ok() : StatusCode(r.StatusCode, r.Error);
    }
}

