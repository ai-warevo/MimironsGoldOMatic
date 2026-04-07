using Marten;
using MediatR;
using Microsoft.Extensions.Logging;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;
using MimironsGoldOMatic.Backend.Application.Roulette.Enrollment;

namespace MimironsGoldOMatic.Backend.Application.Roulette.Handlers;

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
            {
                log.LogError("Helix reward-sent announcement failed after Sent for payout {Id}", p.Id);
            }

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

