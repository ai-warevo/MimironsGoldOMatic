namespace MimironsGoldOMatic.Backend.Domain.System.Abstract;

public interface IUnitOfWorkFactory
{
    IUnitOfWork Create();
}
