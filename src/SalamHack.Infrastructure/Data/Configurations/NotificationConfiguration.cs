using SalamHack.Domain.Notifications;
using SalamHack.Domain.Projects;
using SalamHack.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace SalamHack.Infrastructure.Data.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("notifications");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.UserId)
            .IsRequired();

        builder.Property(n => n.Type)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Message)
            .HasMaxLength(1000)
            .IsRequired();

        builder.Property(n => n.IsRead)
            .IsRequired();

        builder.Property(n => n.ScheduledAt)
            .HasColumnType("datetimeoffset");

        builder.Property(n => n.SentAt)
            .HasColumnType("datetimeoffset");

        builder.ConfigureAuditColumns();

        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(n => n.Invoice)
            .WithMany(i => i.Notifications)
            .HasForeignKey(n => n.InvoiceId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(n => n.Project)
            .WithMany()
            .HasForeignKey(n => new { n.ProjectId, n.UserId })
            .HasPrincipalKey(p => new { p.Id, p.UserId })
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(n => n.UserId);

        builder.HasIndex(n => new { n.UserId, n.IsRead, n.ScheduledAt });

        builder.HasIndex(n => new { n.UserId, n.Type });

        builder.HasIndex(n => n.InvoiceId);

        builder.HasIndex(n => new { n.UserId, n.ProjectId });
    }

}
