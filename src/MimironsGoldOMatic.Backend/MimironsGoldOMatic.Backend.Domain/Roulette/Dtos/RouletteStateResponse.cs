namespace MimironsGoldOMatic.Backend.Domain.Roulette.Dtos;

public sealed record RouletteStateResponse(
    DateTime NextSpinAt,
    DateTime ServerNow,
    int SpinIntervalSeconds,
    int PoolParticipantCount,
    string SpinPhase,
    Guid? CurrentSpinCycleId);
