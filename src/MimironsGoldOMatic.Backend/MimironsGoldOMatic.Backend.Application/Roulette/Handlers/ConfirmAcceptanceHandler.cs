using Marten;
using MediatR;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;

namespace MimironsGoldOMatic.Backend.Application.Roulette.Handlers;

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

