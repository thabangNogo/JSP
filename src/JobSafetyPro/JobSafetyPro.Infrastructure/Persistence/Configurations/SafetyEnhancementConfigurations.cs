using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Entities.Incidents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSafetyPro.Infrastructure.Persistence.Configurations;

public class StopUnsafeWorkConfiguration : IEntityTypeConfiguration<StopUnsafeWork>
{
    public void Configure(EntityTypeBuilder<StopUnsafeWork> builder)
    {
        builder.ToTable("StopUnsafeWorkReports");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Department).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Location).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Section).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.ActionsTaken).HasMaxLength(4000);
        builder.Property(e => e.Category).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.ImmediateRisk).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.ReportedByUserId);
        builder.HasOne(e => e.ReportedByUser)
            .WithMany()
            .HasForeignKey(e => e.ReportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}

public class SafetyNotificationConfiguration : IEntityTypeConfiguration<SafetyNotification>
{
    public void Configure(EntityTypeBuilder<SafetyNotification> builder)
    {
        builder.ToTable("SafetyNotifications");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Message).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.RelatedEntityType).HasMaxLength(100);
        builder.Property(e => e.Priority).HasConversion<string>().HasMaxLength(20);
        builder.HasIndex(e => new { e.UserId, e.IsRead });
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAuditFields();
    }
}

public class UserDeviceConfiguration : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(EntityTypeBuilder<UserDevice> builder)
    {
        builder.ToTable("UserDevices");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FcmToken).IsRequired().HasMaxLength(500);
        builder.Property(e => e.Platform).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.UserId);
        builder.HasIndex(e => e.FcmToken).IsUnique();
        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAuditFields();
    }
}
