using SalamHack.Domain.Customers;
using SalamHack.Domain.Invoices;
using SalamHack.Domain.Projects;
using SalamHack.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SalamHack.Infrastructure.Data.Configurations;

public sealed class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("invoices");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.UserId)
            .IsRequired();

        builder.Property(i => i.ProjectId)
            .IsRequired();

        builder.Property(i => i.CustomerId)
            .IsRequired();

        builder.Property(i => i.InvoiceNumber)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.TotalAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.TaxAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.TotalWithTax)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.AdvanceAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.PaidAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(i => i.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(i => i.IssueDate)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(i => i.DueDate)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(i => i.Notes)
            .HasMaxLength(2000);

        builder.Property(i => i.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.ConfigureAuditColumns();

        builder.Property(i => i.DeletedAtUtc)
            .HasColumnType("datetimeoffset");

        builder.HasOne(i => i.Project)
            .WithMany(p => p.Invoices)
            .HasForeignKey(i => new { i.ProjectId, i.CustomerId })
            .HasPrincipalKey(p => new { p.Id, p.CustomerId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(i => i.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<Customer>()
            .WithMany()
            .HasForeignKey(i => new { i.CustomerId, i.UserId })
            .HasPrincipalKey(c => new { c.Id, c.UserId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(i => new { i.UserId, i.InvoiceNumber })
            .IsUnique()
            .HasFilter("[DeletedAtUtc] IS NULL");

        builder.HasIndex(i => new { i.ProjectId, i.CustomerId });

        builder.HasIndex(i => i.UserId);

        builder.HasIndex(i => i.CustomerId);

        builder.HasIndex(i => i.Status);

        builder.HasIndex(i => i.DueDate);

        builder.HasIndex(i => i.DeletedAtUtc);
    }

}
