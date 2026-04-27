using SalamHack.Domain.Analyses;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SalamHack.Infrastructure.Data.Configurations;

public sealed class AnalysisConfiguration : IEntityTypeConfiguration<Analysis>
{
    public void Configure(EntityTypeBuilder<Analysis> builder)
    {
        builder.ToTable("analyses");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.ProjectId)
            .IsRequired();

        builder.Property(a => a.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.WhatHappened)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(a => a.WhatItMeans)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(a => a.WhatToDo)
            .HasMaxLength(4000)
            .IsRequired();

        builder.Property(a => a.HealthStatus)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.GeneratedAt)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(a => a.Title)
            .HasMaxLength(200);

        builder.Property(a => a.Summary)
            .HasMaxLength(1000);

        builder.Property(a => a.ConfidenceScore)
            .HasColumnType("decimal(5,4)");

        builder.Property(a => a.MetadataJson)
            .HasColumnType("nvarchar(max)");

        builder.Property(a => a.ReviewedAtUtc)
            .HasColumnType("datetimeoffset");

        builder.ConfigureAuditColumns();

        builder.HasOne(a => a.Project)
            .WithMany(p => p.Analyses)
            .HasForeignKey(a => a.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasQueryFilter(a => a.Project.DeletedAtUtc == null);

        builder.HasIndex(a => a.ProjectId);

        builder.HasIndex(a => new { a.ProjectId, a.Type, a.GeneratedAt });
    }

}
