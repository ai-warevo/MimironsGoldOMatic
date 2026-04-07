namespace MimironsGoldOMatic.Backend.Infrastructure.Persistence;

public sealed class EnrollmentIdempotencyDocument
{
    public string Id { get; set; } = "";
    public string TwitchUserId { get; set; } = "";
    public string TwitchDisplayName { get; set; } = "";
    public string CharacterName { get; set; } = "";
    public DateTime EnrolledAtUtc { get; set; }
}

