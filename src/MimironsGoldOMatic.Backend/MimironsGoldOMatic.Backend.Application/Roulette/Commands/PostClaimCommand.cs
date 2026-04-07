using MimironsGoldOMatic.Backend.Application.Roulette.Dtos;

namespace MimironsGoldOMatic.Backend.Application.Roulette.Commands;

public sealed record PostClaimCommand(string TwitchUserId, string TwitchDisplayName, CreatePayoutRequest Body)
    : IRequest<HandlerResult<PoolEnrollmentResponse>>;
