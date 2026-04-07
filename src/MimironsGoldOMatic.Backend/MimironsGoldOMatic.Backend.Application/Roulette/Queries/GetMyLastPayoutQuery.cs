namespace MimironsGoldOMatic.Backend.Application.Roulette.Queries;

public sealed record GetMyLastPayoutQuery(string TwitchUserId) : IRequest<HandlerResult<PayoutDto?>>;
