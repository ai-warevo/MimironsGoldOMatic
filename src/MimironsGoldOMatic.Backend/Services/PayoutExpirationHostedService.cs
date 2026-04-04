using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Shared;
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
                await using var write = store.LightweightSession();
                var cutoff = DateTime.UtcNow.AddHours(-24);
                var old = await write.Query<PayoutReadDocument>()
                    .Where(p =>
                        (p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.InProgress) &&
                        p.CreatedAt < cutoff)
                    .ToListAsync(stoppingToken);

                if (old.Count == 0)
                    continue;

                foreach (var p in old)
                {
                    var loaded = await write.LoadAsync<PayoutReadDocument>(p.Id, stoppingToken);
                    if (loaded == null || (loaded.Status != PayoutStatus.Pending && loaded.Status != PayoutStatus.InProgress))
                        continue;
                    if (loaded.CreatedAt >= cutoff)
                        continue;
                    var from = loaded.Status;
                    loaded.Status = PayoutStatus.Expired;
                    loaded.UpdatedAt = DateTime.UtcNow;
                    write.Store(loaded);
                    write.Events.Append(loaded.Id, new PayoutStatusChanged(from, PayoutStatus.Expired, DateTime.UtcNow));
                }

                await write.SaveChangesAsync(stoppingToken);
                log.LogInformation("Expired {Count} payouts older than 24h", old.Count);
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
