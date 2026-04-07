using MimironsGoldOMatic.Backend.Domain.Roulette.Dtos;

namespace MimironsGoldOMatic.Backend.Domain.Roulette.Queries;

public sealed record GetPoolMeQuery(string TwitchUserId) : IRequest<HandlerResult<PoolMeResponse>>;
