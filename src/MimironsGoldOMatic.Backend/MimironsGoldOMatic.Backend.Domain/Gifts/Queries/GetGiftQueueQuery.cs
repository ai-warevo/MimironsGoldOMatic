namespace MimironsGoldOMatic.Backend.Domain.Gifts.Queries;

public sealed record GetGiftQueueQuery(string? StreamerId, string? ViewerId)
    : IRequest<HandlerResult<IReadOnlyList<GiftRequestDto>>>;
