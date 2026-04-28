using SalamHack.Domain.Expenses;
using SalamHack.Domain.Projects;
using SalamHack.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SalamHack.Infrastructure.Data.Configurations;

public sealed class ExpenseConfiguration : IEntityTypeConfiguration<Expense>
{
    public void Configure(EntityTypeBuilder<Expense> builder)
    {
        builder.ToTable("expenses");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId)
            .IsRequired();

        builder.Property(e => e.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(e => e.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(e => e.IsRecurring)
            .IsRequired();

        builder.Property(e => e.ExpenseDate)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(e => e.RecurrenceInterval)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(e => e.RecurrenceEndDate)
            .HasColumnType("datetimeoffset");

        builder.Property(e => e.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.ConfigureAuditColumns();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Project)
            .WithMany(p => p.Expenses)
            .HasForeignKey(e => new { e.ProjectId, e.UserId })
            .HasPrincipalKey(p => new { p.Id, p.UserId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.UserId);

        builder.HasIndex(e => new { e.UserId, e.ExpenseDate });

        builder.HasIndex(e => new { e.UserId, e.Category });

        builder.HasIndex(e => new { e.UserId, e.IsRecurring });

        builder.HasIndex(e => new { e.UserId, e.ProjectId });
    }

}
