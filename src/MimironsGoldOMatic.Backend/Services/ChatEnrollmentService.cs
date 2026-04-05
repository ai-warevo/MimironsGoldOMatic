using MimironsGoldOMatic.Backend.Persistence;
using MimironsGoldOMatic.Shared;
using Marten;

namespace MimironsGoldOMatic.Backend.Services;

/// <summary>EventSub <c>!twgold &lt;CharacterName&gt;</c> enrollment (SPEC section 5).</summary>
public sealed class ChatEnrollmentService(IDocumentStore store, ILogger<ChatEnrollmentService> log)
{
    public async Task IngestAsync(
        string messageId,
        string twitchUserId,
        string twitchDisplayName,
        string chatText,
        bool isSubscriber,
        CancellationToken ct)
    {
        if (string.IsNullOrEmpty(messageId) || string.IsNullOrEmpty(twitchUserId))
            return;

        await using var session = store.LightweightSession();
        if (await session.LoadAsync<ChatMessageDedupDocument>(messageId, ct) != null)
            return;

        if (!TwGoldChatEnrollmentParser.TryGetCharacterName(chatText, out var characterName))
            return;

        if (!isSubscriber)
        {
            log.LogInformation("Ignoring non-subscriber chat enroll for user {UserId}", twitchUserId);
            return;
        }

        if (!CharacterNameRules.IsValid(characterName))
            return;

        var active = await session.Query<PayoutReadDocument>()
            .Where(p => p.TwitchUserId == twitchUserId &&
                        (p.Status == PayoutStatus.Pending || p.Status == PayoutStatus.InProgress))
            .AnyAsync(ct);
        if (active)
            return;

        var sentRows = await session.Query<PayoutReadDocument>()
            .Where(p => p.TwitchUserId == twitchUserId && p.Status == PayoutStatus.Sent)
            .ToListAsync(ct);
        if (sentRows.Sum(p => p.GoldAmount) + PayoutEconomics.MvpWinningPayoutGold > 10_000)
            return;

        var pool = await session.LoadAsync<PoolDocument>(EbsIds.PoolDocumentId, ct) ?? new PoolDocument { Id = EbsIds.PoolDocumentId };
        var taken = pool.Members.Any(x =>
            string.Equals(x.CharacterName, characterName, StringComparison.Ordinal) &&
            x.TwitchUserId != twitchUserId);
        if (taken)
            return;

        pool.Members.RemoveAll(x => x.TwitchUserId == twitchUserId);
        pool.Members.Add(new PoolMemberEntry
        {
            TwitchUserId = twitchUserId,
            TwitchDisplayName = twitchDisplayName,
            CharacterName = characterName.Trim(),
        });
        session.Store(pool);
        session.Store(new ChatMessageDedupDocument { Id = messageId, ProcessedAtUtc = DateTime.UtcNow });
        await session.SaveChangesAsync(ct);
    }
}
