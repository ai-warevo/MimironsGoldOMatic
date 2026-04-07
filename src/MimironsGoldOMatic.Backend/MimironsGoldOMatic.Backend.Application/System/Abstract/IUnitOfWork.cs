namespace MimironsGoldOMatic.Backend.Application.System.Abstract;

/// <summary>Unit-of-work boundary for transactional persistence (implemented in Infrastructure).</summary>
public interface IUnitOfWork : IAsyncDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
