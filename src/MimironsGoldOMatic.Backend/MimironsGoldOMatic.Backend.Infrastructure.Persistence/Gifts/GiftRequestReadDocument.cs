namespace MimironsGoldOMatic.Backend.Infrastructure.Persistence;

public sealed class GiftRequestReadDocument
{
    public Guid Id { get; set; }
    public string StreamerId { get; set; } = "";
    public string ViewerId { get; set; } = "";
    public string ViewerDisplayName { get; set; } = "";
    public string CharacterName { get; set; } = "";
    public GiftRequestState State { get; set; } = GiftRequestState.Pending;
    public GiftSelectedItemDocument? SelectedItem { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public DateTime? TimeoutAt { get; set; }
    public string? FailureReason { get; set; }
}

