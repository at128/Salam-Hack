using SalamHack.Domain.Common.Constants;
using SalamHack.Domain.Customers;
using SalamHack.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SalamHack.Infrastructure.Data.Configurations;

public sealed class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.CustomerName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(c => c.Email)
            .HasMaxLength(ApplicationConstants.FieldLengths.EmailMaxLength)
            .IsRequired();

        builder.Property(c => c.Phone)
            .HasMaxLength(ApplicationConstants.FieldLengths.PhoneNumberMaxLength)
            .IsRequired();

        builder.Property(c => c.ClientType)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(c => c.CompanyName)
            .HasMaxLength(200);

        builder.Property(c => c.Notes)
            .HasMaxLength(2000);

        builder.ConfigureAuditColumns();

        builder.Property(c => c.DeletedAtUtc)
            .HasColumnType("datetimeoffset");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(c => c.UserId);

        builder.HasIndex(c => new { c.UserId, c.Email })
            .IsUnique()
            .HasFilter("[DeletedAtUtc] IS NULL");

        builder.HasAlternateKey(c => new { c.Id, c.UserId });

        builder.HasIndex(c => c.DeletedAtUtc);
    }
}
