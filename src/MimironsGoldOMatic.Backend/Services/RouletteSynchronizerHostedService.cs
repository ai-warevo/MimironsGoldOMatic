using MimironsGoldOMatic.Backend.Persistence;
using Marten;

namespace MimironsGoldOMatic.Backend.Services;

/// <summary>Advances spin cycles and selects a uniform-random candidate per SPEC.</summary>
public sealed class RouletteSynchronizerHostedService(
    IDocumentStore store,
    ILogger<RouletteSynchronizerHostedService> log)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await using var session = store.LightweightSession();
                var utcNow = DateTime.UtcNow;
                var pool = await session.LoadAsync<PoolDocument>(EbsIds.PoolDocumentId, stoppingToken)
                           ?? new PoolDocument { Id = EbsIds.PoolDocumentId };
                var spin = await session.LoadAsync<SpinStateDocument>(EbsIds.SpinStateDocumentId, stoppingToken);
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
                    await session.SaveChangesAsync(stoppingToken);
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
                    await session.SaveChangesAsync(stoppingToken);
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
                    await session.SaveChangesAsync(stoppingToken);
                }
                else
                {
                    session.Store(pool);
                    await session.SaveChangesAsync(stoppingToken);
                }
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                log.LogError(ex, "Roulette synchronizer tick failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}
