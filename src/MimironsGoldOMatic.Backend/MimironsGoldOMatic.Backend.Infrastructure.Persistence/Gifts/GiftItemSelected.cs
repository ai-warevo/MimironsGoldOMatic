namespace MimironsGoldOMatic.Backend.Infrastructure.Persistence;

public record GiftItemSelected(
    string Name,
    int Id,
    int Count,
    string Link,
    string Texture,
    int BagId,
    int SlotId,
    DateTime Utc);

