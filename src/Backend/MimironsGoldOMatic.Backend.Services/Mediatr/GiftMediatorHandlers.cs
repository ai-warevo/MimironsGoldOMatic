using Marten;
using MediatR;
using MimironsGoldOMatic.Backend.Domain;
using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Shared;

namespace MimironsGoldOMatic.Backend.Services.Mediatr;

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
            mapped = mapped.Where(x => x.ViewerId == request.ViewerId || x.State == GiftRequestStateDto.SelectingItem).ToList();
        return new HandlerResult<IReadOnlyList<GiftRequestDto>>(true, mapped, 200, null);
    }
}

public sealed class CreateGiftRequestHandler(
    IDocumentStore store,
    GiftQueueService queue,
    ITwitchSubscriberVerifier subscriberVerifier)
    : IRequestHandler<CreateGiftRequestCommand, HandlerResult<GiftRequestDto>>
{
    public async Task<HandlerResult<GiftRequestDto>> Handle(CreateGiftRequestCommand request, CancellationToken ct)
    {
        if (!CharacterNameRules.IsValid(request.Body.CharacterName))
            return new HandlerResult<GiftRequestDto>(false, null, 400, new ApiErrorDto("invalid_character_name", "Invalid character name.", new { }));

        if (!await subscriberVerifier.IsSubscriberAsync(request.Body.StreamerId, request.ViewerId, ct))
            return new HandlerResult<GiftRequestDto>(false, null, 403, new ApiErrorDto("not_subscriber", "Only subscribers can use !twgift.", new { }));

        await using var session = store.QuerySession();
        var usageId = $"{request.Body.StreamerId}:{request.ViewerId}";
        var used = await session.LoadAsync<GiftCommandUsageDocument>(usageId, ct);
        if (used != null)
            return new HandlerResult<GiftRequestDto>(false, null, 409, new ApiErrorDto("gift_already_used", "!twgift can be used once per streamer.", new { }));

        var active = await session.Query<GiftRequestReadDocument>()
            .Where(x => x.StreamerId == request.Body.StreamerId && x.ViewerId == request.ViewerId &&
                        (x.State == GiftRequestState.Pending || x.State == GiftRequestState.SelectingItem ||
                         x.State == GiftRequestState.ItemSelected || x.State == GiftRequestState.WaitingConfirmation))
            .AnyAsync(ct);
        if (active)
            return new HandlerResult<GiftRequestDto>(false, null, 409, new ApiErrorDto("gift_request_active", "Active gift request already exists.", new { }));

        var created = await queue.EnqueueAsync(request.Body.StreamerId, request.ViewerId, request.ViewerDisplayName, request.Body.CharacterName, ct);
        await using var read = store.QuerySession();
        var ordered = (await read.Query<GiftRequestReadDocument>()
            .Where(x => x.StreamerId == created.StreamerId && x.State != GiftRequestState.Completed && x.State != GiftRequestState.Failed)
            .OrderBy(x => x.CreatedAt)
            .ToListAsync(ct)).ToList();
        var idx = ordered.FindIndex(x => x.Id == created.Id);
        return new HandlerResult<GiftRequestDto>(true, GiftMap.ToDto(created, idx >= 0 ? idx + 1 : 1), 201, null);
    }
}

public sealed class PatchGiftRequestHandler(IDocumentStore store, GiftQueueService queue)
    : IRequestHandler<PatchGiftRequestCommand, HandlerResult<GiftRequestDto>>
{
    public async Task<HandlerResult<GiftRequestDto>> Handle(PatchGiftRequestCommand request, CancellationToken ct)
    {
        await using var session = store.LightweightSession();
        var row = await session.LoadAsync<GiftRequestReadDocument>(request.Id, ct);
        if (row == null)
            return new HandlerResult<GiftRequestDto>(false, null, 404, new ApiErrorDto("not_found", "Gift request not found.", new { }));

        var from = row.State;
        row.State = GiftMap.ToDomainState(request.State);
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

public sealed class SelectGiftItemHandler(IDocumentStore store)
    : IRequestHandler<SelectGiftItemCommand, HandlerResult<GiftRequestDto>>
{
    public async Task<HandlerResult<GiftRequestDto>> Handle(SelectGiftItemCommand request, CancellationToken ct)
    {
        await using var session = store.LightweightSession();
        var row = await session.LoadAsync<GiftRequestReadDocument>(request.Id, ct);
        if (row == null)
            return new HandlerResult<GiftRequestDto>(false, null, 404, new ApiErrorDto("not_found", "Gift request not found.", new { }));
        if (row.State != GiftRequestState.SelectingItem)
            return new HandlerResult<GiftRequestDto>(false, null, 409, new ApiErrorDto("invalid_state", "Gift request is not in SelectingItem state.", new { }));

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
        session.Events.Append(row.Id, new GiftRequestStateChanged(GiftRequestState.SelectingItem, GiftRequestState.WaitingConfirmation, DateTime.UtcNow));
        await session.SaveChangesAsync(ct);
        return new HandlerResult<GiftRequestDto>(true, GiftMap.ToDto(row, 1), 200, null);
    }
}

public sealed class ConfirmGiftHandler(IDocumentStore store, GiftQueueService queue)
    : IRequestHandler<ConfirmGiftCommand, HandlerResult<GiftRequestDto>>
{
    public async Task<HandlerResult<GiftRequestDto>> Handle(ConfirmGiftCommand request, CancellationToken ct)
    {
        await using var session = store.LightweightSession();
        var row = await session.LoadAsync<GiftRequestReadDocument>(request.Id, ct);
        if (row == null)
            return new HandlerResult<GiftRequestDto>(false, null, 404, new ApiErrorDto("not_found", "Gift request not found.", new { }));
        if (row.State != GiftRequestState.WaitingConfirmation)
            return new HandlerResult<GiftRequestDto>(false, null, 409, new ApiErrorDto("invalid_state", "Gift request not waiting confirmation.", new { }));

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

internal static class GiftMap
{
    public static GiftRequestDto ToDto(GiftRequestReadDocument d, int queuePosition)
    {
        var item = d.SelectedItem == null
            ? null
            : new GiftSelectedItemDto(d.SelectedItem.Name, d.SelectedItem.Id, d.SelectedItem.Count, d.SelectedItem.Link, d.SelectedItem.Texture,
                d.SelectedItem.BagId, d.SelectedItem.SlotId);
        var wait = Math.Max(0, (queuePosition - 1) * 65);
        return new GiftRequestDto(d.Id, d.StreamerId, d.ViewerId, d.ViewerDisplayName, d.CharacterName, ToDtoState(d.State), item, queuePosition, wait, d.CreatedAt,
            d.UpdatedAt, d.TimeoutAt, d.FailureReason);
    }

    public static GiftRequestStateDto ToDtoState(GiftRequestState state) => state switch
    {
        GiftRequestState.Pending => GiftRequestStateDto.Pending,
        GiftRequestState.SelectingItem => GiftRequestStateDto.SelectingItem,
        GiftRequestState.ItemSelected => GiftRequestStateDto.ItemSelected,
        GiftRequestState.WaitingConfirmation => GiftRequestStateDto.WaitingConfirmation,
        GiftRequestState.Completed => GiftRequestStateDto.Completed,
        GiftRequestState.Failed => GiftRequestStateDto.Failed,
        _ => GiftRequestStateDto.Failed,
    };

    public static GiftRequestState ToDomainState(GiftRequestStateDto state) => state switch
    {
        GiftRequestStateDto.Pending => GiftRequestState.Pending,
        GiftRequestStateDto.SelectingItem => GiftRequestState.SelectingItem,
        GiftRequestStateDto.ItemSelected => GiftRequestState.ItemSelected,
        GiftRequestStateDto.WaitingConfirmation => GiftRequestState.WaitingConfirmation,
        GiftRequestStateDto.Completed => GiftRequestState.Completed,
        GiftRequestStateDto.Failed => GiftRequestState.Failed,
        _ => GiftRequestState.Failed,
    };
}

