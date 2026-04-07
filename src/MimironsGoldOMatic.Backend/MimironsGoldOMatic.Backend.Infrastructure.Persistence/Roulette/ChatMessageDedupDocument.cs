namespace MimironsGoldOMatic.Backend.Infrastructure.Persistence;

public sealed class ChatMessageDedupDocument
{
    public string Id { get; set; } = "";
    public DateTime ProcessedAtUtc { get; set; }
}

