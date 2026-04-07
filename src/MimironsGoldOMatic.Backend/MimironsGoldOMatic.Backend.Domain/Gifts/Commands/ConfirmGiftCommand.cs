namespace MimironsGoldOMatic.Backend.Domain.Gifts.Commands;

public sealed record ConfirmGiftCommand(Guid Id, bool Confirmed) : IRequest<HandlerResult<GiftRequestDto>>;
