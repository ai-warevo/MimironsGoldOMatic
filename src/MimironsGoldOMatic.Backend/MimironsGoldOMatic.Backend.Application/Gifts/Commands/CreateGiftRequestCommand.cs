namespace MimironsGoldOMatic.Backend.Application.Gifts.Commands;

public sealed record CreateGiftRequestCommand(string ViewerId, string ViewerDisplayName, CreateGiftRequest Body)
    : IRequest<HandlerResult<GiftRequestDto>>;
