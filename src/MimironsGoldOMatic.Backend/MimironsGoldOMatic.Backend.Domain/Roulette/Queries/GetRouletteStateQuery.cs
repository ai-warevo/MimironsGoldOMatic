using MimironsGoldOMatic.Backend.Domain.Roulette.Dtos;

namespace MimironsGoldOMatic.Backend.Domain.Roulette.Queries;

public sealed record GetRouletteStateQuery : IRequest<HandlerResult<RouletteStateResponse>>;
