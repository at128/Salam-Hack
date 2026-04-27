using SalamHack.Domain.Payments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SalamHack.Infrastructure.Data.Configurations;

public sealed class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("payments");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.InvoiceId)
            .IsRequired();

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Method)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.PaymentDate)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(p => p.Notes)
            .HasMaxLength(2000);

        builder.Property(p => p.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.ConfigureAuditColumns();

        builder.HasOne(p => p.Invoice)
            .WithMany(i => i.Payments)
            .HasForeignKey(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(p => p.Invoice.DeletedAtUtc == null);

        builder.HasIndex(p => p.InvoiceId);

        builder.HasIndex(p => p.PaymentDate);

        builder.HasIndex(p => p.Method);
    }

}
