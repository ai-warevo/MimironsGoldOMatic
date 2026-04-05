using Marten;

namespace MimironsGoldOMatic.Backend.Services;

/// <summary>Hourly sweep: Pending/InProgress older than 24h → Expired (SPEC section 7).</summary>
public sealed class PayoutExpirationHostedService(
    IDocumentStore store,
    ILogger<PayoutExpirationHostedService> log)
    : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var n = await PayoutExpirationProcessor.ExpireStalePayoutsAsync(store, stoppingToken);
                if (n > 0)
                    log.LogInformation("Expired {Count} payouts older than 24h", n);
            }
            catch (Exception ex) when (!stoppingToken.IsCancellationRequested)
            {
                log.LogError(ex, "Payout expiration job failed");
            }

            try
            {
                await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
        }
    }
}
