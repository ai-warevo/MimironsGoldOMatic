using Marten;
using MediatR;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;
using MimironsGoldOMatic.Backend.Common;

namespace MimironsGoldOMatic.Backend.Application.Gifts.Handlers;

public sealed class GetGiftQueueHandler(IDocumentStore store)
    : IRequestHandler<GetGiftQueueQuery, HandlerResult<IReadOnlyList<GiftRequestDto>>>
{
    public async Task<HandlerResult<IReadOnlyList<GiftRequestDto>>> Handle(GetGiftQueueQuery request, CancellationToken ct)
    {
        await using var session = store.QuerySession();
        var q = session.Query<GiftRequestReadDocument>()
            .Where(x => x.State != GiftRequestState.Completed && x.State != GiftRequestState.Failed);
        if (!string.IsNullOrWhiteSpace(request.StreamerId))
            q = q.Where(x => x.StreamerId == request.StreamerId);
        var list = await q
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct);
        var mapped = list.Select((x, i) => GiftMap.ToDto(x, i + 1)).ToList();
        if (!string.IsNullOrEmpty(request.ViewerId))
            mapped = mapped.Where(x => x.ViewerId == request.ViewerId || x.State == GiftRequestState.SelectingItem).ToList();
        return new HandlerResult<IReadOnlyList<GiftRequestDto>>(true, mapped, 200, null);
    }
}
