using System.Data;
using SalamHack.Application.Common.Interfaces;
using SalamHack.Domain.Analyses;
using SalamHack.Domain.Common;
using SalamHack.Domain.Common.Constants;
using SalamHack.Domain.Customers;
using SalamHack.Domain.Expenses;
using SalamHack.Domain.Invoices;
using SalamHack.Domain.Notifications;
using SalamHack.Domain.Payments;
using SalamHack.Domain.Projects;
using SalamHack.Domain.Services;
using SalamHack.Infrastructure.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace SalamHack.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator)
    : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>(options), IAppDbContext
{
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Service> Services => Set<Service>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Expense> Expenses => Set<Expense>();
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<Analysis> Analyses => Set<Analysis>();

    public override async Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        var domainEvents = CollectDomainEvents();
        var result = await base.SaveChangesAsync(ct);

        await PublishDomainEventsAsync(domainEvents, ct);

        return result;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync(CancellationToken ct)
        => Database.BeginTransactionAsync(ct);

    public Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel, CancellationToken ct)
        => Database.BeginTransactionAsync(isolationLevel, ct);

    public Task ReloadEntityAsync<TEntity>(TEntity entity, CancellationToken ct)
        where TEntity : class
        => Entry(entity).ReloadAsync(ct);

    public void ClearChangeTracker()
        => ChangeTracker.Clear();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        var adminRoleId = Guid.Parse("10000000-0000-0000-0000-000000000001");
        var userRoleId = Guid.Parse("10000000-0000-0000-0000-000000000002");

        builder.Entity<IdentityRole<Guid>>().HasData(
            new IdentityRole<Guid>
            {
                Id = adminRoleId,
                Name = ApplicationConstants.Roles.Admin,
                NormalizedName = ApplicationConstants.Roles.Admin.ToUpperInvariant(),
                ConcurrencyStamp = adminRoleId.ToString()
            },
            new IdentityRole<Guid>
            {
                Id = userRoleId,
                Name = ApplicationConstants.Roles.User,
                NormalizedName = ApplicationConstants.Roles.User.ToUpperInvariant(),
                ConcurrencyStamp = userRoleId.ToString()
            });

        foreach (var entityType in builder.Model.GetEntityTypes())
        {
            if (typeof(ISoftDeletable).IsAssignableFrom(entityType.ClrType))
            {
                var method = typeof(AppDbContext)
                    .GetMethod(nameof(ApplySoftDeleteFilter),
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)!
                    .MakeGenericMethod(entityType.ClrType);

                method.Invoke(null, [builder]);
            }
        }
    }

    private static void ApplySoftDeleteFilter<T>(ModelBuilder builder)
        where T : class, ISoftDeletable
    {
        builder.Entity<T>().HasQueryFilter(e => e.DeletedAtUtc == null);
    }

    private List<DomainEvent> CollectDomainEvents()
    {
        var entities = ChangeTracker.Entries<Entity>()
            .Where(e => e.Entity.DomainEvents.Count != 0)
            .Select(e => e.Entity)
            .ToList();

        var domainEvents = entities.SelectMany(e => e.DomainEvents).ToList();

        foreach (var entity in entities)
            entity.ClearDomainEvents();

        return domainEvents;
    }

    private async Task PublishDomainEventsAsync(List<DomainEvent> domainEvents, CancellationToken ct)
    {
        foreach (var domainEvent in domainEvents)
            await mediator.Publish(domainEvent, ct);
    }
}
