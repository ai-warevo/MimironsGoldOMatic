using FluentValidation;
using Marten;
using MediatR;
using Microsoft.Extensions.Options;
using MimironsGoldOMatic.Backend.Configuration;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;
using MimironsGoldOMatic.Backend.Common;

namespace MimironsGoldOMatic.Backend.Application.Roulette.Handlers;

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
