namespace MimironsGoldOMatic.Desktop.Api;

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

public sealed record PatchGiftRequestBody(GiftRequestState State, string? Reason);
public sealed record SelectGiftItemBody(GiftSelectedItemDto Item);
public sealed record ConfirmGiftBody(bool Confirmed);
