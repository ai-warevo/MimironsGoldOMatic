using Marten;
using MediatR;
using MimironsGoldOMatic.Backend.Common;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;

namespace MimironsGoldOMatic.Backend.Application.Roulette.Handlers;

public sealed class VerifyCandidateHandler(IDocumentStore store)
    : IRequestHandler<VerifyCandidateCommand, HandlerResult<Unit>>
{
    public async Task<HandlerResult<Unit>> Handle(VerifyCandidateCommand request, CancellationToken ct)
    {
        var b = request.Body;
        if (b.SchemaVersion != 1)
            return new HandlerResult<Unit>(false, default, 400,
                new ApiErrorDto("invalid_payload", "schemaVersion must be 1.", new { }));

        if (!CharacterNameRules.IsValid(b.CharacterName))
            return new HandlerResult<Unit>(false, default, 400,
                new ApiErrorDto("invalid_character_name", "Invalid character name.", new { }));

        await using var session = store.LightweightSession();
        var spin = await session.LoadAsync<SpinStateDocument>(EbsIds.SpinStateDocumentId, ct);
        if (spin == null || spin.SpinCycleId != b.SpinCycleId)
            return new HandlerResult<Unit>(false, default, 400,
                new ApiErrorDto("out_of_sequence", "spinCycleId does not match active cycle.", new { }));

        var utcNow = DateTime.UtcNow;
        if (utcNow > spin.VerificationDeadlineUtc)
            return new HandlerResult<Unit>(false, default, 400,
                new ApiErrorDto("out_of_sequence", "verify-candidate window closed.", new { }));

        if (spin.PayoutCreatedForCycle)
            return new HandlerResult<Unit>(true, Unit.Value, 200, null);

        if (!b.Online)
        {
            spin.PayoutCreatedForCycle = true;
            session.Store(spin);
            await session.SaveChangesAsync(ct);
            return new HandlerResult<Unit>(true, Unit.Value, 200, null);
        }

        if (!string.Equals(spin.CandidateCharacterName, b.CharacterName, StringComparison.Ordinal))
            return new HandlerResult<Unit>(false, default, 400,
                new ApiErrorDto("out_of_sequence", "characterName does not match server candidate.", new { }));

        if (string.IsNullOrEmpty(spin.CandidateTwitchUserId))
            return new HandlerResult<Unit>(false, default, 400,
                new ApiErrorDto("out_of_sequence", "No candidate for this cycle.", new { }));

        var active = await session.Query<PayoutReadDocument>()
            .Where(p => p.TwitchUserId == spin.CandidateTwitchUserId &&
                        (p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.InProgress))
            .AnyAsync(ct);
        if (active)
            return new HandlerResult<Unit>(false, default, 409,
                new ApiErrorDto("active_payout_exists", "Winner already has an active payout.", new { }));

        var payoutId = Guid.NewGuid();
        var enrollmentKey = $"spin:{spin.SpinCycleId:N}";
        var doc = new PayoutReadDocument
        {
            Id = payoutId,
            TwitchUserId = spin.CandidateTwitchUserId!,
            TwitchDisplayName = spin.CandidateTwitchDisplayName ?? "",
            CharacterName = spin.CandidateCharacterName!,
            GoldAmount = PayoutEconomics.MvpWinningPayoutGold,
            EnrollmentRequestId = enrollmentKey,
            Status = PayoutStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            SpinCycleId = spin.SpinCycleId,
        };
        session.Store(doc);
        session.Events.StartStream(payoutId,
            new PayoutCreated(
                payoutId,
                doc.TwitchUserId,
                doc.TwitchDisplayName,
                doc.CharacterName,
                doc.GoldAmount,
                doc.EnrollmentRequestId,
                doc.Status,
                doc.CreatedAt,
                spin.SpinCycleId));
        spin.PayoutCreatedForCycle = true;
        session.Store(spin);
        await session.SaveChangesAsync(ct);
        return new HandlerResult<Unit>(true, Unit.Value, 200, null);
    }
}

