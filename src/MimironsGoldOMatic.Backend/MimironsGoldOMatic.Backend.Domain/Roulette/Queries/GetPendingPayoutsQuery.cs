namespace MimironsGoldOMatic.Backend.Domain.Roulette.Queries;

public sealed record GetPendingPayoutsQuery : IRequest<HandlerResult<IReadOnlyList<PayoutDto>>>;
