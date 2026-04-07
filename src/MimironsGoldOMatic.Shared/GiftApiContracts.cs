namespace MimironsGoldOMatic.Shared;

public enum GiftRequestState
{
    Pending,
    SelectingItem,
    ItemSelected,
    WaitingConfirmation,
    Completed,
    Failed,
}

public sealed record GiftSelectedItemDto(
    string Name,
    int Id,
    int Count,
    string Link,
    string Texture,
    int BagId,
    int SlotId);

public sealed record GiftRequestDto(
    Guid Id,
    string StreamerId,
    string ViewerId,
    string ViewerDisplayName,
    string CharacterName,
    GiftRequestState State,
    GiftSelectedItemDto? SelectedItem,
    int QueuePosition,
    int EstimatedWaitSeconds,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    DateTime? TimeoutAt,
    string? FailureReason);

public sealed record CreateGiftRequest(string StreamerId, string CharacterName);

public sealed record PatchGiftRequestState(GiftRequestState State, string? Reason);

public sealed record SelectGiftItemRequest(GiftSelectedItemDto Item);

public sealed record ConfirmGiftRequest(bool Confirmed);
