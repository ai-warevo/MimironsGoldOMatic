namespace MimironsGoldOMatic.Backend.Domain.Gifts.Commands;

public sealed record SelectGiftItemCommand(Guid Id, GiftSelectedItemDto Item) : IRequest<HandlerResult<GiftRequestDto>>;
