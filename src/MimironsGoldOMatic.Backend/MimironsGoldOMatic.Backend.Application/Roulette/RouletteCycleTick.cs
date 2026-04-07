using Marten;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;

namespace MimironsGoldOMatic.Backend.Application.Roulette;

/// <summary>Single synchronizer iteration (shared by <see cref="RouletteSynchronizerHostedService"/> and tests).</summary>
public static class RouletteCycleTick
{
    public static async Task ApplyAsync(IDocumentStore store, DateTime utcNow, CancellationToken cancellationToken)
    {
        await using var session = store.LightweightSession();
        utcNow = DateTime.SpecifyKind(utcNow, DateTimeKind.Utc);
        var pool = await session.LoadAsync<PoolDocument>(EbsIds.PoolDocumentId, cancellationToken)
                   ?? new PoolDocument { Id = EbsIds.PoolDocumentId };
        var spin = await session.LoadAsync<SpinStateDocument>(EbsIds.SpinStateDocumentId, cancellationToken);
        var cycleStart = RouletteTime.FloorToFiveMinuteUtc(utcNow);

        if (spin == null)
        {
            spin = new SpinStateDocument
            {
                CycleStartUtc = cycleStart,
                SpinCycleId = Guid.NewGuid(),
                VerificationDeadlineUtc = cycleStart.AddMinutes(5).AddSeconds(30),
                PoolWasEmptyAtCycleStart = pool.Members.Count == 0,
            };
            session.Store(spin);
            session.Store(pool);
            await session.SaveChangesAsync(cancellationToken);
        }
        else if (spin.CycleStartUtc != cycleStart)
        {
            spin.CycleStartUtc = cycleStart;
            spin.SpinCycleId = Guid.NewGuid();
            spin.CandidateTwitchUserId = null;
            spin.CandidateCharacterName = null;
            spin.CandidateTwitchDisplayName = null;
            spin.CandidateSelectedAtUtc = null;
            spin.PayoutCreatedForCycle = false;
            spin.PoolWasEmptyAtCycleStart = pool.Members.Count == 0;
            spin.VerificationDeadlineUtc = cycleStart.AddMinutes(5).AddSeconds(30);
            session.Store(spin);
            session.Store(pool);
            await session.SaveChangesAsync(cancellationToken);
        }
        else if (!spin.PoolWasEmptyAtCycleStart
                 && pool.Members.Count > 0
                 && spin.CandidateSelectedAtUtc == null
                 && utcNow >= cycleStart.AddSeconds(RouletteTime.CollectingSeconds))
        {
            var idx = Random.Shared.Next(pool.Members.Count);
            var m = pool.Members[idx];
            spin.CandidateTwitchUserId = m.TwitchUserId;
            spin.CandidateCharacterName = m.CharacterName;
            spin.CandidateTwitchDisplayName = m.TwitchDisplayName;
            spin.CandidateSelectedAtUtc = utcNow;
            session.Store(spin);
            session.Store(pool);
            await session.SaveChangesAsync(cancellationToken);
        }
        else
        {
            session.Store(pool);
            await session.SaveChangesAsync(cancellationToken);
        }
    }
}

