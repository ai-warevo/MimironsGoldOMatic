namespace MimironsGoldOMatic.Backend.Application.Gifts.Commands;

public sealed record ConfirmGiftCommand(Guid Id, bool Confirmed) : IRequest<HandlerResult<GiftRequestDto>>;
