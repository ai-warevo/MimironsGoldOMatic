namespace MimironsGoldOMatic.Backend.Domain.Roulette.Commands;

public sealed record PatchPayoutStatusCommand(Guid Id, PayoutStatus NewStatus) : IRequest<HandlerResult<PayoutDto>>;
