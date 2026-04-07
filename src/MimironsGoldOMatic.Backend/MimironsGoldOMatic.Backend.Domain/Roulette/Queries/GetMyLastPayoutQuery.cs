namespace MimironsGoldOMatic.Backend.Domain.Roulette.Queries;

public sealed record GetMyLastPayoutQuery(string TwitchUserId) : IRequest<HandlerResult<PayoutDto?>>;
