using Marten;
using MediatR;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;
using MimironsGoldOMatic.Backend.Common;
using MimironsGoldOMatic.Backend.Application.Roulette.Enrollment;

namespace MimironsGoldOMatic.Backend.Application.Gifts.Handlers;

public sealed class CreateGiftRequestHandler(
    IDocumentStore store,
    GiftQueueService queue,
    ITwitchSubscriberVerifier subscriberVerifier)
    : IRequestHandler<CreateGiftRequestCommand, HandlerResult<GiftRequestDto>>
{
    public async Task<HandlerResult<GiftRequestDto>> Handle(CreateGiftRequestCommand request, CancellationToken ct)
    {
        if (!CharacterNameRules.IsValid(request.Body.CharacterName))
            return new HandlerResult<GiftRequestDto>(false, null, 400,
                new ApiErrorDto("invalid_character_name", "Invalid character name.", new { }));

        if (!await subscriberVerifier.IsSubscriberAsync(request.Body.StreamerId, request.ViewerId, ct))
            return new HandlerResult<GiftRequestDto>(false, null, 403,
                new ApiErrorDto("not_subscriber", "Only subscribers can use !twgift.", new { }));

        await using var session = store.QuerySession();
        var usageId = $"{request.Body.StreamerId}:{request.ViewerId}";
        var used = await session.LoadAsync<GiftCommandUsageDocument>(usageId, ct);
        if (used != null)
            return new HandlerResult<GiftRequestDto>(false, null, 409,
                new ApiErrorDto("gift_already_used", "!twgift can be used once per streamer.", new { }));

        var active = await session.Query<GiftRequestReadDocument>()
            .Where(x => x.StreamerId == request.Body.StreamerId && x.ViewerId == request.ViewerId &&
                        (x.State == GiftRequestState.Pending || x.State == GiftRequestState.SelectingItem ||
                         x.State == GiftRequestState.ItemSelected || x.State == GiftRequestState.WaitingConfirmation))
            .AnyAsync(ct);
        if (active)
            return new HandlerResult<GiftRequestDto>(false, null, 409,
                new ApiErrorDto("gift_request_active", "Active gift request already exists.", new { }));

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

