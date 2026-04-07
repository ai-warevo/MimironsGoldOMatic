using Marten;
using MediatR;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;

namespace MimironsGoldOMatic.Backend.Application.Roulette.Handlers;

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

