using Marten;
using MediatR;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;

namespace MimironsGoldOMatic.Backend.Application.Gifts.Handlers;

public sealed class SelectGiftItemHandler(IDocumentStore store)
    : IRequestHandler<SelectGiftItemCommand, HandlerResult<GiftRequestDto>>
{
    public async Task<HandlerResult<GiftRequestDto>> Handle(SelectGiftItemCommand request, CancellationToken ct)
    {
        await using var session = store.LightweightSession();
        var row = await session.LoadAsync<GiftRequestReadDocument>(request.Id, ct);
        if (row == null)
            return new HandlerResult<GiftRequestDto>(false, null, 404,
                new ApiErrorDto("not_found", "Gift request not found.", new { }));
        if (row.State != GiftRequestState.SelectingItem)
            return new HandlerResult<GiftRequestDto>(false, null, 409,
                new ApiErrorDto("invalid_state", "Gift request is not in SelectingItem state.", new { }));

        row.SelectedItem = new GiftSelectedItemDocument
        {
            Name = request.Item.Name,
            Id = request.Item.Id,
            Count = request.Item.Count,
            Link = request.Item.Link,
            Texture = request.Item.Texture,
            BagId = request.Item.BagId,
            SlotId = request.Item.SlotId,
        };
        row.State = GiftRequestState.WaitingConfirmation;
        row.TimeoutAt = DateTime.UtcNow.Add(GiftQueueService.WaitingConfirmationTimeout);
        row.UpdatedAt = DateTime.UtcNow;
        session.Store(row);
        session.Events.Append(row.Id, new GiftItemSelected(
            request.Item.Name,
            request.Item.Id,
            request.Item.Count,
            request.Item.Link,
            request.Item.Texture,
            request.Item.BagId,
            request.Item.SlotId,
            DateTime.UtcNow));
        session.Events.Append(row.Id,
            new GiftRequestStateChanged(GiftRequestState.SelectingItem, GiftRequestState.WaitingConfirmation, DateTime.UtcNow));
        await session.SaveChangesAsync(ct);
        return new HandlerResult<GiftRequestDto>(true, GiftMap.ToDto(row, 1), 200, null);
    }
}

