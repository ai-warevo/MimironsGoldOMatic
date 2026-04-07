namespace MimironsGoldOMatic.Shared.Gifts;

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
