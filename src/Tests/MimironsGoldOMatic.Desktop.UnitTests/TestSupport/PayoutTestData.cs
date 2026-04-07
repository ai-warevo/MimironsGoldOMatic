
namespace MimironsGoldOMatic.Desktop.UnitTests.TestSupport;

internal static class PayoutTestData
{
    public static PayoutDto Pending(
        Guid? id = null,
        string characterName = "Thrall",
        long goldAmount = 100,
        DateTime? createdAt = null) =>
        new(
            id ?? Guid.NewGuid(),
            "twitch-1",
            "ViewerOne",
            characterName,
            goldAmount,
            "enr-1",
            PayoutStatus.Pending,
            createdAt ?? DateTime.UtcNow);
}
