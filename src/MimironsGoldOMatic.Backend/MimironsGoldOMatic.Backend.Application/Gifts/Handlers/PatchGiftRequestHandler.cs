using Marten;
using MediatR;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;

namespace MimironsGoldOMatic.Backend.Application.Gifts.Handlers;

public sealed class PatchGiftRequestHandler(IDocumentStore store, GiftQueueService queue)
    : IRequestHandler<PatchGiftRequestCommand, HandlerResult<GiftRequestDto>>
{
    public async Task<HandlerResult<GiftRequestDto>> Handle(PatchGiftRequestCommand request, CancellationToken ct)
    {
        await using var session = store.LightweightSession();
        var row = await session.LoadAsync<GiftRequestReadDocument>(request.Id, ct);
        if (row == null)
            return new HandlerResult<GiftRequestDto>(false, null, 404,
                new ApiErrorDto("not_found", "Gift request not found.", new { }));

        var from = row.State;
        row.State = request.State;
        row.UpdatedAt = DateTime.UtcNow;
        row.FailureReason = row.State == GiftRequestState.Failed ? request.Reason ?? "failed" : null;
        if (row.State == GiftRequestState.SelectingItem)
            row.TimeoutAt = DateTime.UtcNow.Add(GiftQueueService.SelectingTimeout);
        else if (row.State == GiftRequestState.WaitingConfirmation)
            row.TimeoutAt = DateTime.UtcNow.Add(GiftQueueService.WaitingConfirmationTimeout);
        else if (row.State == GiftRequestState.Completed || row.State == GiftRequestState.Failed)
            row.TimeoutAt = null;

        session.Store(row);
        session.Events.Append(row.Id, new GiftRequestStateChanged(from, row.State, DateTime.UtcNow, request.Reason));
        if (row.State == GiftRequestState.Completed)
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

        if (row.State == GiftRequestState.Completed || row.State == GiftRequestState.Failed)
            await queue.TryPromoteNextAsync(row.StreamerId, ct);

        return new HandlerResult<GiftRequestDto>(true, GiftMap.ToDto(row, 1), 200, null);
    }
}

