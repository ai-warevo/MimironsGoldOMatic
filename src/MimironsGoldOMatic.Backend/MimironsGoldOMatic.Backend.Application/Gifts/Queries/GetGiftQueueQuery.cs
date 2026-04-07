namespace MimironsGoldOMatic.Backend.Application.Gifts.Queries;

public sealed record GetGiftQueueQuery(string? StreamerId, string? ViewerId)
    : IRequest<HandlerResult<IReadOnlyList<GiftRequestDto>>>;
