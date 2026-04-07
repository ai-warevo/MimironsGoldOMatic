using MimironsGoldOMatic.Shared;
using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Backend.Shared;
using Marten;
using Microsoft.Extensions.DependencyInjection;

namespace MimironsGoldOMatic.Desktop.IntegrationTests.Support;

internal static class PayoutDocumentSeed
{
    public static async Task InsertPendingAsync(IServiceProvider services, PayoutReadDocument doc, CancellationToken ct = default)
    {
        await using var scope = services.CreateAsyncScope();
        var store = scope.ServiceProvider.GetRequiredService<IDocumentStore>();
        await using var s = store.LightweightSession();
        s.Store(doc);
        await s.SaveChangesAsync(ct);
    }

    public static PayoutReadDocument CreatePending(Guid id, string characterName = "Hero", string twitchUserId = "u1") =>
        new()
        {
            Id = id,
            TwitchUserId = twitchUserId,
            TwitchDisplayName = "D",
            CharacterName = characterName,
            GoldAmount = PayoutEconomics.MvpWinningPayoutGold,
            EnrollmentRequestId = "e2e",
            Status = PayoutStatus.Pending,
            CreatedAt = DateTime.UtcNow,
        };
}
