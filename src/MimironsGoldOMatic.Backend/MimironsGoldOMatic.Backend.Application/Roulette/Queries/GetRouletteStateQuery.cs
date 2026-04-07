using MimironsGoldOMatic.Backend.Application.Roulette.Dtos;

namespace MimironsGoldOMatic.Backend.Application.Roulette.Queries;

public sealed record GetRouletteStateQuery : IRequest<HandlerResult<RouletteStateResponse>>;
