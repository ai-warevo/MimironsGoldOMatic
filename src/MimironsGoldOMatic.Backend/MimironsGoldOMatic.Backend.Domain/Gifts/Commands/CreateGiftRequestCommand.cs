namespace MimironsGoldOMatic.Backend.Domain.Gifts.Commands;

public sealed record CreateGiftRequestCommand(string ViewerId, string ViewerDisplayName, CreateGiftRequest Body)
    : IRequest<HandlerResult<GiftRequestDto>>;
