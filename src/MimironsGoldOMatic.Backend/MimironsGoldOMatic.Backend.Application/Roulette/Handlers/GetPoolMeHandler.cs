using Marten;
using MediatR;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;

namespace MimironsGoldOMatic.Backend.Application.Roulette.Handlers;

public sealed class GetPoolMeHandler(IDocumentStore store)
    : IRequestHandler<GetPoolMeQuery, HandlerResult<PoolMeResponse>>
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

