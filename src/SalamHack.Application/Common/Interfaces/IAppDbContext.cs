using System.Data;
using SalamHack.Domain.Analyses;
using SalamHack.Domain.Customers;
using SalamHack.Domain.Expenses;
using SalamHack.Domain.Invoices;
using SalamHack.Domain.Notifications;
using SalamHack.Domain.Payments;
using SalamHack.Domain.Projects;
using SalamHack.Domain.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace SalamHack.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Customer> Customers { get; }

    DbSet<Service> Services { get; }

    DbSet<Project> Projects { get; }

    DbSet<Expense> Expenses { get; }

    DbSet<Invoice> Invoices { get; }

    DbSet<Payment> Payments { get; }

    DbSet<Notification> Notifications { get; }

    DbSet<Analysis> Analyses { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct);

    Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct);

    Task ReloadEntityAsync<TEntity>(TEntity entity, CancellationToken ct)
        where TEntity : class;

    void ClearChangeTracker();
}
