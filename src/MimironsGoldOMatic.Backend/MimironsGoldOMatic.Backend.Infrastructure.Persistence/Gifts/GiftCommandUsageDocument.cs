namespace MimironsGoldOMatic.Backend.Infrastructure.Persistence;

public sealed class GiftCommandUsageDocument
{
    public string Id { get; set; } = "";
    public string StreamerId { get; set; } = "";
    public string ViewerId { get; set; } = "";
    public DateTime CompletedAtUtc { get; set; }
    public Guid GiftRequestId { get; set; }
}

