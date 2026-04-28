using SalamHack.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SalamHack.Infrastructure.Data.Configurations;

internal static class AuditableEntityConfigurationExtensions
{
    public static void ConfigureAuditColumns<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : AuditableEntity
    {
        builder.Property(e => e.CreatedAtUtc)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(e => e.CreatedBy)
            .HasMaxLength(128);

        builder.Property(e => e.LastModifiedUtc)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(e => e.LastModifiedBy)
            .HasMaxLength(128);
    }
}
