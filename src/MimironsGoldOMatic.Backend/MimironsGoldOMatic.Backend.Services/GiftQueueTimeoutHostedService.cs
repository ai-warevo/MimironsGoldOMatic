using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace MimironsGoldOMatic.Backend.Services;

public sealed class GiftQueueTimeoutHostedService(
    GiftQueueService queue,
    ILogger<GiftQueueTimeoutHostedService> log)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await queue.FailTimedOutAsync(stoppingToken);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                log.LogError(ex, "Gift queue timeout worker failed");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}

