namespace MimironsGoldOMatic.Backend.Application.Roulette.Queries;

public sealed record GetPendingPayoutsQuery : IRequest<HandlerResult<IReadOnlyList<PayoutDto>>>;
