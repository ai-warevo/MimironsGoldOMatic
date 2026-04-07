using FluentValidation;
using Marten;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimironsGoldOMatic.Shared;
using MimironsGoldOMatic.Backend.Configuration;
using MimironsGoldOMatic.Backend.Domain;
using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Shared;

namespace MimironsGoldOMatic.Backend.Services.Mediatr;

public sealed class PostClaimHandler(
    IDocumentStore store,
    IOptions<MgmOptions> mgm,
    IValidator<CreatePayoutRequest> validator)
    : IRequestHandler<PostClaimCommand, HandlerResult<PoolEnrollmentResponse>>
{
    public async Task<HandlerResult<PoolEnrollmentResponse>> Handle(PostClaimCommand request, CancellationToken ct)
    {
        var v = await validator.ValidateAsync(request.Body, ct);
        if (!v.IsValid)
            return new HandlerResult<PoolEnrollmentResponse>(false, null, 400,
                new ApiErrorDto("invalid_character_name", v.ToString(), new { }));

        if (mgm.Value.DevSkipSubscriberCheck == false)
        {
            // MVP: Extension claim path should verify subscriber via Helix; without tokens treat as non-subscriber.
            // Dev: set Mgm:DevSkipSubscriberCheck true for local testing.
            return new HandlerResult<PoolEnrollmentResponse>(false, null, 403,
                new ApiErrorDto("not_subscriber", "Subscriber verification requires Helix configuration or DevSkipSubscriberCheck.", new { }));
        }

        await using var session = store.LightweightSession();

        var idem = await session.LoadAsync<EnrollmentIdempotencyDocument>(request.Body.EnrollmentRequestId, ct);
        if (idem != null)
        {
            if (idem.TwitchUserId != request.TwitchUserId)
                return new HandlerResult<PoolEnrollmentResponse>(false, null, 409,
                    new ApiErrorDto("duplicate_enrollment", "EnrollmentRequestId belongs to another user.", new { }));
            return new HandlerResult<PoolEnrollmentResponse>(true,
                new PoolEnrollmentResponse(idem.CharacterName, request.Body.EnrollmentRequestId), 200, null);
        }

        var active = await session.Query<PayoutReadDocument>()
            .Where(p => p.TwitchUserId == request.TwitchUserId &&
                        (p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.InProgress))
            .AnyAsync(ct);
        if (active)
            return new HandlerResult<PoolEnrollmentResponse>(false, null, 409,
                new ApiErrorDto("active_payout_exists", "User already has an active payout.", new { }));

        var sentRows = await session.Query<PayoutReadDocument>()
            .Where(p => p.TwitchUserId == request.TwitchUserId && p.Status == PayoutStatus.Sent)
            .ToListAsync(ct);
        var sentGold = sentRows.Sum(p => p.GoldAmount);
        if (sentGold + PayoutEconomics.MvpWinningPayoutGold > 10_000)
            return new HandlerResult<PoolEnrollmentResponse>(false, null, 409,
                new ApiErrorDto("lifetime_cap_reached", "Lifetime 10,000g cap reached.", new { }));

        var pool = await session.LoadAsync<PoolDocument>(EbsIds.PoolDocumentId, ct) ?? new PoolDocument { Id = EbsIds.PoolDocumentId };
        var nameTaken = pool.Members.Any(m =>
            string.Equals(m.CharacterName, request.Body.CharacterName, StringComparison.Ordinal) &&
            m.TwitchUserId != request.TwitchUserId);
        if (nameTaken)
            return new HandlerResult<PoolEnrollmentResponse>(false, null, 409,
                new ApiErrorDto("character_name_taken_in_pool", "Character name is taken by another viewer.", new { }));

        pool.Members.RemoveAll(m => m.TwitchUserId == request.TwitchUserId);
        pool.Members.Add(new PoolMemberEntry
        {
            TwitchUserId = request.TwitchUserId,
            TwitchDisplayName = request.TwitchDisplayName,
            CharacterName = request.Body.CharacterName.Trim(),
        });
        session.Store(pool);
        session.Store(new EnrollmentIdempotencyDocument
        {
            Id = request.Body.EnrollmentRequestId,
            TwitchUserId = request.TwitchUserId,
            TwitchDisplayName = request.TwitchDisplayName,
            CharacterName = request.Body.CharacterName.Trim(),
            EnrolledAtUtc = DateTime.UtcNow,
        });
        await session.SaveChangesAsync(ct);
        return new HandlerResult<PoolEnrollmentResponse>(true,
            new PoolEnrollmentResponse(request.Body.CharacterName.Trim(), request.Body.EnrollmentRequestId), 201, null);
    }
}

public sealed class GetRouletteStateHandler(IDocumentStore store)
    : IRequestHandler<GetRouletteStateQuery, HandlerResult<RouletteStateResponse>>
{
    public async Task<HandlerResult<RouletteStateResponse>> Handle(GetRouletteStateQuery request, CancellationToken ct)
    {
        await using var session = store.QuerySession();
        var utcNow = DateTime.UtcNow;
        var pool = await session.LoadAsync<PoolDocument>(EbsIds.PoolDocumentId, ct) ?? new PoolDocument();
        var spin = await session.LoadAsync<SpinStateDocument>(EbsIds.SpinStateDocumentId, ct);
        if (spin == null)
            return new HandlerResult<RouletteStateResponse>(true,
                new RouletteStateResponse(
                    NextSpinAt: RouletteTime.NextSpinBoundaryUtc(utcNow),
                    ServerNow: utcNow,
                    SpinIntervalSeconds: 300,
                    PoolParticipantCount: pool.Members.Count,
                    SpinPhase: "idle",
                    CurrentSpinCycleId: null),
                200, null);

        var phase = SpinPhaseResolver.Resolve(utcNow, pool, spin);
        var next = RouletteTime.NextSpinBoundaryUtc(utcNow);
        Guid? cycleId = phase == "idle" ? null : spin.SpinCycleId;
        return new HandlerResult<RouletteStateResponse>(true,
            new RouletteStateResponse(next, utcNow, 300, pool.Members.Count, phase, cycleId), 200, null);
    }
}

public sealed class GetPoolMeHandler(IDocumentStore store) : IRequestHandler<GetPoolMeQuery, HandlerResult<PoolMeResponse>>
{
    public async Task<HandlerResult<PoolMeResponse>> Handle(GetPoolMeQuery request, CancellationToken ct)
    {
        await using var session = store.QuerySession();
        var pool = await session.LoadAsync<PoolDocument>(EbsIds.PoolDocumentId, ct) ?? new PoolDocument();
        var row = pool.Members.FirstOrDefault(m => m.TwitchUserId == request.TwitchUserId);
        if (row == null)
            return new HandlerResult<PoolMeResponse>(true, new PoolMeResponse(false, null), 200, null);
        return new HandlerResult<PoolMeResponse>(true, new PoolMeResponse(true, row.CharacterName), 200, null);
    }
}

public sealed class GetPendingPayoutsHandler(IDocumentStore store)
    : IRequestHandler<GetPendingPayoutsQuery, HandlerResult<IReadOnlyList<PayoutDto>>>
{
    public async Task<HandlerResult<IReadOnlyList<PayoutDto>>> Handle(GetPendingPayoutsQuery request, CancellationToken ct)
    {
        await using var session = store.QuerySession();
        var list = await session.Query<PayoutReadDocument>()
            .Where(p => p.Status == PayoutStatus.Pending)
            .OrderBy(p => p.CreatedAt)
            .ToListAsync(ct);
        return new HandlerResult<IReadOnlyList<PayoutDto>>(true, list.Select(Map).ToList(), 200, null);
    }

    private static PayoutDto Map(PayoutReadDocument d) =>
        new(d.Id, d.TwitchUserId, d.TwitchDisplayName, d.CharacterName, d.GoldAmount, d.EnrollmentRequestId, d.Status,
            d.CreatedAt, d.IsRewardSentAnnouncedToChat);
}

public sealed class GetMyLastPayoutHandler(IDocumentStore store)
    : IRequestHandler<GetMyLastPayoutQuery, HandlerResult<PayoutDto?>>
{
    public async Task<HandlerResult<PayoutDto?>> Handle(GetMyLastPayoutQuery request, CancellationToken ct)
    {
        await using var session = store.QuerySession();
        var p = await session.Query<PayoutReadDocument>()
            .Where(x => x.TwitchUserId == request.TwitchUserId)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);
        if (p == null)
            return new HandlerResult<PayoutDto?>(true, null, 404, null);
        var dto = new PayoutDto(p.Id, p.TwitchUserId, p.TwitchDisplayName, p.CharacterName, p.GoldAmount,
            p.EnrollmentRequestId, p.Status, p.CreatedAt, p.IsRewardSentAnnouncedToChat);
        return new HandlerResult<PayoutDto?>(true, dto, 200, null);
    }
}

public sealed class PatchPayoutStatusHandler(
    IDocumentStore store,
    HelixChatService helix,
    ILogger<PatchPayoutStatusHandler> log)
    : IRequestHandler<PatchPayoutStatusCommand, HandlerResult<PayoutDto>>
{
    public async Task<HandlerResult<PayoutDto>> Handle(PatchPayoutStatusCommand request, CancellationToken ct)
    {
        await using var session = store.LightweightSession();
        var p = await session.LoadAsync<PayoutReadDocument>(request.Id, ct);
        if (p == null)
            return new HandlerResult<PayoutDto>(false, null, 404,
                new ApiErrorDto("not_found", "Payout not found.", new { }));

        if (!IsAllowed(p.Status, request.NewStatus))
            return new HandlerResult<PayoutDto>(false, null, 409,
                new ApiErrorDto("terminal_status_change_not_allowed", "Transition not allowed.", new { }));

        var from = p.Status;
        p.Status = request.NewStatus;
        p.UpdatedAt = DateTime.UtcNow;
        session.Store(p);
        session.Events.Append(p.Id, new PayoutStatusChanged(from, request.NewStatus, DateTime.UtcNow));

        if (request.NewStatus == PayoutStatus.Sent)
        {
            var pool = await session.LoadAsync<PoolDocument>(EbsIds.PoolDocumentId, ct) ?? new PoolDocument { Id = EbsIds.PoolDocumentId };
            pool.Members.RemoveAll(m => m.TwitchUserId == p.TwitchUserId);
            session.Store(pool);
        }

        await session.SaveChangesAsync(ct);

        if (request.NewStatus == PayoutStatus.Sent && !p.IsRewardSentAnnouncedToChat)
        {
            var ok = await helix.TrySendRewardSentAnnouncementAsync(p.CharacterName, ct);
            if (ok)
            {
                await using var s2 = store.LightweightSession();
                var p2 = await s2.LoadAsync<PayoutReadDocument>(request.Id, ct);
                if (p2 != null)
                {
                    p2.IsRewardSentAnnouncedToChat = true;
                    p2.UpdatedAt = DateTime.UtcNow;
                    s2.Store(p2);
                    s2.Events.Append(p2.Id, new HelixRewardSentAnnouncementSucceeded(DateTime.UtcNow));
                    await s2.SaveChangesAsync(ct);
                }
            }
            else
                log.LogError("Helix reward-sent announcement failed after Sent for payout {Id}", p.Id);
            if (ok)
                p.IsRewardSentAnnouncedToChat = true;
        }

        var dto = new PayoutDto(p.Id, p.TwitchUserId, p.TwitchDisplayName, p.CharacterName, p.GoldAmount,
            p.EnrollmentRequestId, p.Status, p.CreatedAt, p.IsRewardSentAnnouncedToChat);
        return new HandlerResult<PayoutDto>(true, dto, 200, null);
    }

    private static bool IsAllowed(PayoutStatus from, PayoutStatus to) => (from, to) switch
    {
        (PayoutStatus.Pending, PayoutStatus.InProgress) => true,
        (PayoutStatus.Pending, PayoutStatus.Cancelled) => true,
        (PayoutStatus.Pending, PayoutStatus.Failed) => true,
        (PayoutStatus.InProgress, PayoutStatus.Sent) => true,
        (PayoutStatus.InProgress, PayoutStatus.Cancelled) => true,
        (PayoutStatus.InProgress, PayoutStatus.Failed) => true,
        (PayoutStatus.InProgress, PayoutStatus.Pending) => true,
        _ => false,
    };
}

public sealed class ConfirmAcceptanceHandler(IDocumentStore store)
    : IRequestHandler<ConfirmAcceptanceCommand, HandlerResult<Unit>>
{
    public async Task<HandlerResult<Unit>> Handle(ConfirmAcceptanceCommand request, CancellationToken ct)
    {
        await using var session = store.LightweightSession();
        var p = await session.LoadAsync<PayoutReadDocument>(request.Id, ct);
        if (p == null)
            return new HandlerResult<Unit>(false, default, 404, new ApiErrorDto("not_found", "Payout not found.", new { }));
        if (!string.Equals(p.CharacterName, request.CharacterName, StringComparison.Ordinal))
            return new HandlerResult<Unit>(false, default, 400,
                new ApiErrorDto("invalid_character_name", "characterName does not match payout.", new { }));
        if (p.WinnerAcceptedWillingToReceiveAt != null)
            return new HandlerResult<Unit>(true, Unit.Value, 200, null);
        p.WinnerAcceptedWillingToReceiveAt = DateTime.UtcNow;
        p.UpdatedAt = DateTime.UtcNow;
        session.Store(p);
        session.Events.Append(p.Id, new WinnerAcceptanceRecorded(DateTime.UtcNow));
        await session.SaveChangesAsync(ct);
        return new HandlerResult<Unit>(true, Unit.Value, 200, null);
    }
}

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

