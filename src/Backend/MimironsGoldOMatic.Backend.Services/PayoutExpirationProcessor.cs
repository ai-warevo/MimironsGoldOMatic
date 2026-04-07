using Marten;
using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Abstract;

namespace MimironsGoldOMatic.Backend.Services;

/// <summary>One expiration sweep (shared by <see cref="PayoutExpirationHostedService"/> and tests).</summary>
public static class PayoutExpirationProcessor
{
    /// <returns>Number of payouts transitioned to <see cref="PayoutStatus.Expired"/>.</returns>
    public static async Task<int> ExpireStalePayoutsAsync(IDocumentStore store, CancellationToken cancellationToken)
    {
        await using var write = store.LightweightSession();
        // Marten maps these columns to PostgreSQL "timestamp without time zone"; Npgsql rejects Kind=UTC in parameters.
        var cutoff = DateTime.SpecifyKind(DateTime.UtcNow.AddHours(-24), DateTimeKind.Unspecified);
        var old = await write.Query<PayoutReadDocument>()
            .Where(p =>
                (p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.InProgress) &&
                p.CreatedAt < cutoff)
            .ToListAsync(cancellationToken);

        var changed = 0;
        foreach (var p in old)
        {
            var loaded = await write.LoadAsync<PayoutReadDocument>(p.Id, cancellationToken);
            if (loaded == null || (loaded.Status != PayoutStatus.Pending && loaded.Status != PayoutStatus.InProgress))
                continue;
            if (loaded.CreatedAt >= cutoff)
                continue;
            var from = loaded.Status;
            loaded.Status = PayoutStatus.Expired;
            loaded.UpdatedAt = DateTime.UtcNow;
            write.Store(loaded);
            write.Events.Append(loaded.Id, new PayoutStatusChanged(from, PayoutStatus.Expired, DateTime.UtcNow));
            changed++;
        }

        if (changed > 0)
            await write.SaveChangesAsync(cancellationToken);

        return changed;
    }
}

