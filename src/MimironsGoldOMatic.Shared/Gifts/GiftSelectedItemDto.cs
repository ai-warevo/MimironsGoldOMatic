namespace MimironsGoldOMatic.Shared.Gifts;

public sealed record GiftSelectedItemDto(
    string Name,
    int Id,
    int Count,
    string Link,
    string Texture,
    int BagId,
    int SlotId);
