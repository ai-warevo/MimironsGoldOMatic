using Marten;
using MediatR;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;

namespace MimironsGoldOMatic.Backend.Application.Roulette.Handlers;

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

