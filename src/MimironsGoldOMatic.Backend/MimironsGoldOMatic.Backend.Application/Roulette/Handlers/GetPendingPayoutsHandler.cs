using Marten;
using MediatR;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;

namespace MimironsGoldOMatic.Backend.Application.Roulette.Handlers;

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

