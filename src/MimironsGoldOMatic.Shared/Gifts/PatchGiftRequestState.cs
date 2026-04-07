namespace MimironsGoldOMatic.Shared.Gifts;

public sealed record PatchGiftRequestState(
    GiftRequestState State,
    string? Reason);
