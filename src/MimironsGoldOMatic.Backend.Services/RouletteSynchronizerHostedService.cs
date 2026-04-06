using Marten;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
                await RouletteCycleTick.ApplyAsync(store, DateTime.UtcNow, stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                log.LogError(ex, "Roulette synchronizer tick failed");
            }

            await Task.Delay(TimeSpan.FromSeconds(1), stoppingToken);
        }
    }
}

