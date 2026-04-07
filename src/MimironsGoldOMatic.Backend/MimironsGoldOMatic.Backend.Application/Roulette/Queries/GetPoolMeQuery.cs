using MimironsGoldOMatic.Backend.Application.Roulette.Dtos;

namespace MimironsGoldOMatic.Backend.Application.Roulette.Queries;

public sealed record GetPoolMeQuery(string TwitchUserId) : IRequest<HandlerResult<PoolMeResponse>>;
