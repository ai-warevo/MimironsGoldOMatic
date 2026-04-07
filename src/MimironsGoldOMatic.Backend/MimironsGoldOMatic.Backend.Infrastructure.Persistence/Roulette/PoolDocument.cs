namespace MimironsGoldOMatic.Backend.Infrastructure.Persistence;

public sealed class PoolDocument
{
    public Guid Id { get; set; } = EbsIds.PoolDocumentId;
    public List<PoolMemberEntry> Members { get; set; } = [];
}

