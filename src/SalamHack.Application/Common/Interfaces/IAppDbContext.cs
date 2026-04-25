using System.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace SalamHack.Application.Common.Interfaces;

public interface IAppDbContext
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct);

    Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct);

    Task ReloadEntityAsync<TEntity>(TEntity entity, CancellationToken ct)
        where TEntity : class;

    void ClearChangeTracker();
}
