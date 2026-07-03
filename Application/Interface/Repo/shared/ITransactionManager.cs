using System.Data;

namespace Application.Interface.Repo.Shared;

public interface ITransactionManager
{
    Task BeginTransactionAsync(CancellationToken ct = default);
    Task BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct = default);
    Task CommitTransactionAsync(CancellationToken ct = default);
    Task RollbackTransactionAsync(CancellationToken ct = default);
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
