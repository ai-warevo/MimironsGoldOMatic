using Marten;
using MediatR;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;

namespace MimironsGoldOMatic.Backend.Application.Gifts.Handlers;

public sealed class ConfirmGiftHandler(IDocumentStore store, GiftQueueService queue)
    : IRequestHandler<ConfirmGiftCommand, HandlerResult<GiftRequestDto>>
{
    public async Task<HandlerResult<GiftRequestDto>> Handle(ConfirmGiftCommand request, CancellationToken ct)
    {
        await using var session = store.LightweightSession();
        var row = await session.LoadAsync<GiftRequestReadDocument>(request.Id, ct);
        if (row == null)
            return new HandlerResult<GiftRequestDto>(false, null, 404,
                new ApiErrorDto("not_found", "Gift request not found.", new { }));
        if (row.State != GiftRequestState.WaitingConfirmation)
            return new HandlerResult<GiftRequestDto>(false, null, 409,
                new ApiErrorDto("invalid_state", "Gift request not waiting confirmation.", new { }));

        var to = request.Confirmed ? GiftRequestState.Completed : GiftRequestState.Failed;
        row.State = to;
        row.TimeoutAt = null;
        row.UpdatedAt = DateTime.UtcNow;
        row.FailureReason = request.Confirmed ? null : "receiver_declined";
        session.Store(row);
        session.Events.Append(row.Id, new GiftRequestStateChanged(GiftRequestState.WaitingConfirmation, to, DateTime.UtcNow, row.FailureReason));
        if (to == GiftRequestState.Completed)
        {
            session.Store(new GiftCommandUsageDocument
            {
                Id = $"{row.StreamerId}:{row.ViewerId}",
                StreamerId = row.StreamerId,
                ViewerId = row.ViewerId,
                CompletedAtUtc = DateTime.UtcNow,
                GiftRequestId = row.Id,
            });
        }
        await session.SaveChangesAsync(ct);
        await queue.TryPromoteNextAsync(row.StreamerId, ct);
        return new HandlerResult<GiftRequestDto>(true, GiftMap.ToDto(row, 1), 200, null);
    }
}

