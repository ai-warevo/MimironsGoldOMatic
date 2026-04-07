namespace MimironsGoldOMatic.Backend.Application.Roulette.Commands;

public sealed record PatchPayoutStatusCommand(Guid Id, PayoutStatus NewStatus) : IRequest<HandlerResult<PayoutDto>>;
