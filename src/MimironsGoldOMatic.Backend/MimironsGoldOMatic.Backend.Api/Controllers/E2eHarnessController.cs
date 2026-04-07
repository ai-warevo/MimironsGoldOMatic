using Marten;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MimironsGoldOMatic.Backend.Configuration;
using MimironsGoldOMatic.Backend.Domain;
using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Services;
using MimironsGoldOMatic.Shared;

namespace MimironsGoldOMatic.Backend.Api.Controllers;

/// <summary>
/// Development + opt-in only: aligns spin state and runs verify-candidate so CI reaches Pending without waiting for roulette wall-clock.
/// </summary>
[ApiController]
[Route("api/e2e")]
[Authorize(AuthenticationSchemes = "ApiKey")]
public sealed class E2eHarnessController(
    IWebHostEnvironment env,
    IOptions<MgmOptions> mgm,
    IDocumentStore store,
    IMediator mediator) : ControllerBase
{
    [HttpPost("prepare-pending-payout")]
    public async Task<IActionResult> PreparePendingPayout([FromBody] E2ePreparePendingRequest body, CancellationToken ct)
    {
        if (!env.IsDevelopment() || !mgm.Value.EnableE2eHarness)
            return NotFound();

        if (string.IsNullOrWhiteSpace(body.TwitchUserId))
            return BadRequest(new { error = "twitchUserId is required." });

        var twitchUserId = body.TwitchUserId.Trim();
        string characterName;
        Guid spinCycleId;

        await using (var session = store.LightweightSession())
        {
            var pool = await session.LoadAsync<PoolDocument>(EbsIds.PoolDocumentId, ct) ?? new PoolDocument();
            var member = pool.Members.FirstOrDefault(m => m.TwitchUserId == twitchUserId);
            if (member == null)
                return BadRequest(new { error = "Viewer is not in the pool; run enrollment first." });

            characterName = member.CharacterName;
            var utcNow = DateTime.UtcNow;
            var cycleStart = RouletteTime.FloorToFiveMinuteUtc(utcNow);
            var spin = await session.LoadAsync<SpinStateDocument>(EbsIds.SpinStateDocumentId, ct);

            if (spin == null)
            {
                spinCycleId = Guid.NewGuid();
                spin = new SpinStateDocument
                {
                    CycleStartUtc = cycleStart,
                    SpinCycleId = spinCycleId,
                    VerificationDeadlineUtc = utcNow.AddHours(1),
                    PoolWasEmptyAtCycleStart = false,
                    PayoutCreatedForCycle = false,
                    CandidateTwitchUserId = member.TwitchUserId,
                    CandidateCharacterName = member.CharacterName,
                    CandidateTwitchDisplayName = member.TwitchDisplayName,
                    CandidateSelectedAtUtc = utcNow,
                };
            }
            else
            {
                spinCycleId = spin.SpinCycleId;
                spin.PoolWasEmptyAtCycleStart = false;
                spin.PayoutCreatedForCycle = false;
                spin.CandidateTwitchUserId = member.TwitchUserId;
                spin.CandidateCharacterName = member.CharacterName;
                spin.CandidateTwitchDisplayName = member.TwitchDisplayName;
                spin.CandidateSelectedAtUtc = utcNow;
                spin.VerificationDeadlineUtc = utcNow.AddHours(1);
            }

            session.Store(spin);
            await session.SaveChangesAsync(ct);
        }

        var vr = await mediator.Send(
            new VerifyCandidateCommand(
                new VerifyCandidateRequest(1, spinCycleId, characterName, true, DateTime.UtcNow)),
            ct);
        if (!vr.Ok)
            return StatusCode(vr.StatusCode, vr.Error);

        await using var q = store.QuerySession();
        var pending = await q.Query<PayoutReadDocument>()
            .Where(p => p.TwitchUserId == twitchUserId && p.Status == PayoutStatus.Pending)
            .OrderByDescending(p => p.CreatedAt)
            .FirstOrDefaultAsync(ct);
        if (pending == null)
            return StatusCode(500, new { error = "verify-candidate succeeded but no Pending payout found." });

        return Ok(new E2ePreparePendingResponse(pending.Id, pending.CharacterName));
    }
}

