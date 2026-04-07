using MimironsGoldOMatic.Backend.Domain.Roulette.Dtos;

namespace MimironsGoldOMatic.Backend.Domain.Roulette.Commands;

public sealed record PostClaimCommand(string TwitchUserId, string TwitchDisplayName, CreatePayoutRequest Body)
    : IRequest<HandlerResult<PoolEnrollmentResponse>>;
