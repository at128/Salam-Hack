using SalamHack.Domain.Customers;
using SalamHack.Domain.Projects;
using SalamHack.Domain.Services;
using SalamHack.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SalamHack.Infrastructure.Data.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.UserId)
            .IsRequired();

        builder.Property(p => p.CustomerId)
            .IsRequired();

        builder.Property(p => p.ServiceId)
            .IsRequired();

        builder.Property(p => p.ProjectName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.EstimatedHours)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(p => p.ToolCost)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Revision)
            .IsRequired();

        builder.Property(p => p.IsUrgent)
            .IsRequired();

        builder.Property(p => p.ProfitMargin)
            // Profit margin is a percent and can be very negative if price is tiny.
            // Keep a wider range to avoid DB overflow (e.g. -55900% scenarios).
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.SuggestedPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.MinPrice)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.AdvanceAmount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(p => p.Status)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(p => p.ActualHours)
            .HasColumnType("decimal(10,2)")
            .IsRequired();

        builder.Property(p => p.StartDate)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.Property(p => p.EndDate)
            .HasColumnType("datetimeoffset")
            .IsRequired();

        builder.ConfigureAuditColumns();

        builder.Property(p => p.DeletedAtUtc)
            .HasColumnType("datetimeoffset");

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(p => p.Customer)
            .WithMany(c => c.Projects)
            .HasForeignKey(p => new { p.CustomerId, p.UserId })
            .HasPrincipalKey(c => new { c.Id, c.UserId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Service)
            .WithMany(s => s.Projects)
            .HasForeignKey(p => new { p.ServiceId, p.UserId })
            .HasPrincipalKey(s => new { s.Id, s.UserId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(p => p.UserId);

        builder.HasAlternateKey(p => new { p.Id, p.UserId });

        builder.HasAlternateKey(p => new { p.Id, p.CustomerId });

        builder.HasIndex(p => new { p.UserId, p.Status });

        builder.HasIndex(p => new { p.UserId, p.ProjectName });

        builder.HasIndex(p => new { p.UserId, p.CustomerId });

        builder.HasIndex(p => new { p.UserId, p.ServiceId });

        builder.HasIndex(p => p.DeletedAtUtc);
    }

}
