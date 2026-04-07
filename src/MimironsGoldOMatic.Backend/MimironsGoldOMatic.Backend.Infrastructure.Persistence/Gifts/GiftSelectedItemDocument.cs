namespace MimironsGoldOMatic.Backend.Infrastructure.Persistence;

public sealed class GiftSelectedItemDocument
{
    public string Name { get; set; } = "";
    public int Id { get; set; }
    public int Count { get; set; }
    public string Link { get; set; } = "";
    public string Texture { get; set; } = "";
    public int BagId { get; set; }
    public int SlotId { get; set; }
}

