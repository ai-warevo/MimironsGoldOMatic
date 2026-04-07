namespace MimironsGoldOMatic.Backend.Domain.System.Abstract;

/// <summary>Unit-of-work boundary for transactional persistence (implemented in Infrastructure/DataAccess).</summary>
public interface IUnitOfWork : IAsyncDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
