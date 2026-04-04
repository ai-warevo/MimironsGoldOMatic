using MimironsGoldOMatic.Backend.Persistence;

namespace MimironsGoldOMatic.Backend.Services;

public static class SpinPhaseResolver
{
    public static string Resolve(DateTime utcNow, PoolDocument pool, SpinStateDocument spin)
    {
        utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
        if (spin.PoolWasEmptyAtCycleStart || pool.Members.Count == 0)
            return "idle";

        var t0 = spin.CycleStartUtc;
        var tSelect = t0.AddSeconds(RouletteTime.CollectingSeconds);
        var tVerifyStart = tSelect.AddSeconds(RouletteTime.SpinningSeconds);
        var tCycleEnd = t0.AddMinutes(5);

        if (utcNow < tSelect)
            return "collecting";
        if (spin.CandidateSelectedAtUtc == null)
            return "completed";
        if (utcNow < tVerifyStart)
            return "spinning";
        if (utcNow < tCycleEnd)
            return "verification";
        if (utcNow < spin.VerificationDeadlineUtc)
            return "verification";
        return "completed";
    }
}
