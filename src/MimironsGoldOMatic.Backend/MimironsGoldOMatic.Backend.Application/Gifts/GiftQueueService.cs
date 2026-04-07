using Marten;
using Microsoft.Extensions.Logging;
using MimironsGoldOMatic.Backend.Infrastructure.Persistence;

namespace MimironsGoldOMatic.Backend.Application.Gifts;

public sealed class GiftQueueService(IDocumentStore store, ILogger<GiftQueueService> log)
{
    public static readonly TimeSpan SelectingTimeout = TimeSpan.FromSeconds(60);
    public static readonly TimeSpan WaitingConfirmationTimeout = TimeSpan.FromMinutes(5);

    public async Task<GiftRequestReadDocument> EnqueueAsync(
        string streamerId,
        string viewerId,
        string viewerDisplayName,
        string characterName,
        CancellationToken ct)
    {
        await using var session = store.LightweightSession();
        var request = new GiftRequestReadDocument
        {
            Id = Guid.NewGuid(),
            StreamerId = streamerId,
            ViewerId = viewerId,
            ViewerDisplayName = viewerDisplayName,
            CharacterName = characterName.Trim(),
            State = GiftRequestState.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
        session.Store(request);
        session.Events.StartStream(request.Id, new GiftRequestInitiated(
            request.Id, streamerId, viewerId, viewerDisplayName, request.CharacterName, request.CreatedAt));
        await session.SaveChangesAsync(ct);
        return await TryPromoteNextAsync(streamerId, ct) ?? request;
    }

    public async Task<GiftRequestReadDocument?> TryPromoteNextAsync(string streamerId, CancellationToken ct)
    {
        await using var session = store.LightweightSession();
        var active = await session.Query<GiftRequestReadDocument>()
            .Where(x => x.StreamerId == streamerId &&
                        (x.State == GiftRequestState.SelectingItem || x.State == GiftRequestState.WaitingConfirmation))
            .AnyAsync(ct);
        if (active)
            return null;

        var pending = await session.Query<GiftRequestReadDocument>()
            .Where(x => x.StreamerId == streamerId && x.State == GiftRequestState.Pending)
            .OrderBy(x => x.CreatedAt)
            .FirstOrDefaultAsync(ct);
        if (pending == null)
            return null;

        var loaded = await session.LoadAsync<GiftRequestReadDocument>(pending.Id, ct);
        if (loaded == null || loaded.State != GiftRequestState.Pending)
            return null;
        var from = loaded.State;
        loaded.State = GiftRequestState.SelectingItem;
        loaded.TimeoutAt = DateTime.UtcNow.Add(SelectingTimeout);
        loaded.UpdatedAt = DateTime.UtcNow;
        session.Store(loaded);
        session.Events.Append(loaded.Id, new GiftRequestStateChanged(from, loaded.State, DateTime.UtcNow));
        await session.SaveChangesAsync(ct);
        return loaded;
    }

    public async Task FailTimedOutAsync(CancellationToken ct)
    {
        await using var session = store.LightweightSession();
        var now = DateTime.UtcNow;
        var stale = await session.Query<GiftRequestReadDocument>()
            .Where(x =>
                (x.State == GiftRequestState.SelectingItem || x.State == GiftRequestState.WaitingConfirmation) &&
                x.TimeoutAt != null && x.TimeoutAt < now)
            .ToListAsync(ct);
        if (stale.Count == 0)
            return;

        foreach (var row in stale)
        {
            var loaded = await session.LoadAsync<GiftRequestReadDocument>(row.Id, ct);
            if (loaded == null || loaded.TimeoutAt == null || loaded.TimeoutAt >= now)
                continue;
            var from = loaded.State;
            loaded.State = GiftRequestState.Failed;
            loaded.UpdatedAt = now;
            loaded.FailureReason = from == GiftRequestState.SelectingItem ? "selection_timeout" : "confirmation_timeout";
            loaded.TimeoutAt = null;
            session.Store(loaded);
            session.Events.Append(loaded.Id, new GiftRequestStateChanged(from, GiftRequestState.Failed, now, loaded.FailureReason));
            log.LogWarning("Gift request {GiftRequestId} timed out in state {State}", loaded.Id, from);
        }

        await session.SaveChangesAsync(ct);
        var streamers = stale.Select(x => x.StreamerId).Distinct(StringComparer.Ordinal).ToList();
        foreach (var streamer in streamers)
            await TryPromoteNextAsync(streamer, ct);
    }
}

