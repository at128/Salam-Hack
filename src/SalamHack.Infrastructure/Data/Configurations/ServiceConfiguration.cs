using SalamHack.Domain.Services;
using SalamHack.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SalamHack.Infrastructure.Data.Configurations;

public sealed class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId)
            .IsRequired();

        builder.Property(s => s.ServiceName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(s => s.Category)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(s => s.DefaultHourlyRate)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(s => s.DefaultRevisions)
            .IsRequired();

        builder.Property(s => s.IsActive)
            .IsRequired();

        builder.ConfigureAuditColumns();

        builder.Property(s => s.DeletedAtUtc)
            .HasColumnType("datetimeoffset");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(s => s.UserId);

        builder.HasIndex(s => new { s.UserId, s.ServiceName })
            .IsUnique()
            .HasFilter("[DeletedAtUtc] IS NULL");

        builder.HasAlternateKey(s => new { s.Id, s.UserId });

        builder.HasIndex(s => new { s.UserId, s.Category });

        builder.HasIndex(s => s.DeletedAtUtc);
    }

}
