using MimironsGoldOMatic.Shared;

namespace MimironsGoldOMatic.Desktop.Services;

/// <summary>In-memory map for <c>[MGM_ACCEPT:UUID]</c> → <c>characterName</c> for <c>confirm-acceptance</c>.</summary>
public sealed class PayoutSnapshotCache
{
    private readonly object _lock = new();
    private Dictionary<Guid, string> _byId = new();

    public void UpdateFromPending(IReadOnlyList<PayoutDto> pending)
    {
        lock (_lock)
        {
            _byId = pending.ToDictionary(p => p.Id, p => p.CharacterName);
        }
    }

    public bool TryGetCharacterName(Guid payoutId, out string characterName)
    {
        lock (_lock)
        {
            return _byId.TryGetValue(payoutId, out characterName!);
        }
    }
}
