namespace MimironsGoldOMatic.Backend.Application.Gifts.Commands;

public sealed record PatchGiftRequestCommand(Guid Id, GiftRequestState State, string? Reason)
    : IRequest<HandlerResult<GiftRequestDto>>;
