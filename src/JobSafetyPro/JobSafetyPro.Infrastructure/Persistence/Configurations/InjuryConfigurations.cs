using JobSafetyPro.Domain.Entities.Incidents;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSafetyPro.Infrastructure.Persistence.Configurations;

public class InjuryConfiguration : IEntityTypeConfiguration<Injury>
{
    public void Configure(EntityTypeBuilder<Injury> builder)
    {
        builder.ToTable("Injuries");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.EmployeeName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Department).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Location).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Section).IsRequired().HasMaxLength(200);
        builder.Property(e => e.IncidentDescription).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.ImmediateActionTaken).HasMaxLength(2000);
        builder.Property(e => e.RootCause).HasMaxLength(2000);
        builder.Property(e => e.CorrectiveAction).HasMaxLength(2000);
        builder.Property(e => e.Witnesses).HasMaxLength(1000);
        builder.Property(e => e.InjuryType).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.BodyPartInjured).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(e => e.SubmittedAt);
        builder.HasIndex(e => e.InjuryFreeDaysResetDate);
        builder.HasOne(e => e.Company).WithMany().HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Plant).WithMany().HasForeignKey(e => e.PlantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.OrganizationDepartment).WithMany().HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.EmployeeUser).WithMany().HasForeignKey(e => e.EmployeeUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CapturedByUser).WithMany().HasForeignKey(e => e.CapturedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}

public class InjuryPhotoConfiguration : IEntityTypeConfiguration<InjuryPhoto>
{
    public void Configure(EntityTypeBuilder<InjuryPhoto> builder)
    {
        builder.ToTable("InjuryPhotos");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.FileName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.StoragePath).IsRequired().HasMaxLength(500);
        builder.HasOne(e => e.Injury).WithMany(e => e.Photos).HasForeignKey(e => e.InjuryId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAuditFields();
    }
}

public class InjuryNotificationConfiguration : IEntityTypeConfiguration<InjuryNotification>
{
    public void Configure(EntityTypeBuilder<InjuryNotification> builder)
    {
        builder.ToTable("InjuryNotifications");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Message).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.Priority).HasConversion<string>().HasMaxLength(50);
        builder.HasOne(e => e.Injury).WithMany(e => e.Notifications).HasForeignKey(e => e.InjuryId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}
