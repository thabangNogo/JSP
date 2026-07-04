using JobSafetyPro.Domain.Entities.Identity;
using JobSafetyPro.Domain.Entities.Incidents;
using JobSafetyPro.Domain.Entities.Organization;
using JobSafetyPro.Domain.Entities.Safety;
using JobSafetyPro.Domain.Entities.Shared;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSafetyPro.Infrastructure.Persistence.Configurations;

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.Code).IsUnique();
        builder.ConfigureAuditFields();
    }
}

public class PlantConfiguration : IEntityTypeConfiguration<Plant>
{
    public void Configure(EntityTypeBuilder<Plant> builder)
    {
        builder.ToTable("Plants");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => new { e.CompanyId, e.Code }).IsUnique();
        builder.HasOne(e => e.Company).WithMany(e => e.Plants).HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}

public class DepartmentConfiguration : IEntityTypeConfiguration<Department>
{
    public void Configure(EntityTypeBuilder<Department> builder)
    {
        builder.ToTable("Departments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => new { e.PlantId, e.Code }).IsUnique();
        builder.HasOne(e => e.Plant).WithMany(e => e.Departments).HasForeignKey(e => e.PlantId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.PasswordHash).IsRequired().HasMaxLength(512);
        builder.HasIndex(e => new { e.CompanyId, e.Email }).IsUnique();
        builder.HasOne(e => e.Company).WithMany(e => e.Users).HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Plant).WithMany(e => e.Users).HasForeignKey(e => e.PlantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Department).WithMany(e => e.Users).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}

public class EmployeeProfileConfiguration : IEntityTypeConfiguration<EmployeeProfile>
{
    public void Configure(EntityTypeBuilder<EmployeeProfile> builder)
    {
        builder.ToTable("EmployeeProfiles");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Surname).IsRequired().HasMaxLength(100);
        builder.Property(e => e.CompanyNumber).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Occupation).IsRequired().HasMaxLength(200);
        builder.HasIndex(e => e.UserId).IsUnique();
        builder.HasOne(e => e.User)
            .WithOne(e => e.EmployeeProfile)
            .HasForeignKey<EmployeeProfile>(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.WorkDepartment)
            .WithMany(e => e.EmployeeProfiles)
            .HasForeignKey(e => e.WorkDepartmentId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}

public class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.ToTable("Roles");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(e => new { e.CompanyId, e.Name }).IsUnique();
        builder.ConfigureAuditFields();
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.UserId, e.RoleId, e.PlantId }).IsUnique();
        builder.HasOne(e => e.User).WithMany(e => e.UserRoles).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Role).WithMany(e => e.UserRoles).HasForeignKey(e => e.RoleId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.TokenHash).IsRequired().HasMaxLength(512);
        builder.HasIndex(e => new { e.UserId, e.TokenHash });
        builder.HasOne(e => e.User).WithMany(e => e.RefreshTokens).HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Cascade);
        builder.ConfigureAuditFields();
    }
}

public class JobSafetyAssessmentConfiguration : IEntityTypeConfiguration<JobSafetyAssessment>
{
    public void Configure(EntityTypeBuilder<JobSafetyAssessment> builder)
    {
        builder.ToTable("JobSafetyAssessments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.JobDescription).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.WorkflowDataJson).HasMaxLength(8000);
        builder.Property(e => e.SignOffName).HasMaxLength(100);
        builder.Property(e => e.SignOffSurname).HasMaxLength(100);
        builder.Property(e => e.SignOffCompanyNumber).HasMaxLength(50);
        builder.Property(e => e.SignOffOccupation).HasMaxLength(200);
        builder.Property(e => e.SignatureStoragePath).HasMaxLength(500);
        builder.Property(e => e.Department).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Location).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Section).IsRequired().HasMaxLength(200);
        builder.HasIndex(e => new { e.PlantId, e.Status });
        builder.HasIndex(e => e.CreatedByUserId);
        builder.HasIndex(e => e.Department);
        builder.HasIndex(e => e.Location);
        builder.HasIndex(e => e.Section);
        builder.HasIndex(e => e.WorkLocationId);
        builder.HasIndex(e => e.WorkSectionId);
        builder.HasOne(e => e.WorkLocation)
            .WithMany()
            .HasForeignKey(e => e.WorkLocationId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.WorkSection)
            .WithMany()
            .HasForeignKey(e => e.WorkSectionId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Company).WithMany(e => e.JobSafetyAssessments).HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Plant).WithMany(e => e.JobSafetyAssessments).HasForeignKey(e => e.PlantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.OrganizationDepartment).WithMany(e => e.JobSafetyAssessments).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}

public class RiskLevelConfiguration : IEntityTypeConfiguration<RiskLevel>
{
    public void Configure(EntityTypeBuilder<RiskLevel> builder)
    {
        builder.ToTable("RiskLevels");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.HasIndex(e => e.Code).IsUnique();
        builder.ConfigureAuditFields();
    }
}

public class HazardConfiguration : IEntityTypeConfiguration<Hazard>
{
    public void Configure(EntityTypeBuilder<Hazard> builder)
    {
        builder.ToTable("Hazards");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(2000);
        builder.HasOne(e => e.JobSafetyAssessment).WithMany(e => e.Hazards).HasForeignKey(e => e.JobSafetyAssessmentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.RiskLevel).WithMany(e => e.Hazards).HasForeignKey(e => e.RiskLevelId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ResidualRiskLevel).WithMany().HasForeignKey(e => e.ResidualRiskLevelId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}

public class ControlMeasureConfiguration : IEntityTypeConfiguration<ControlMeasure>
{
    public void Configure(EntityTypeBuilder<ControlMeasure> builder)
    {
        builder.ToTable("ControlMeasures");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.HierarchyOfControl).IsRequired().HasMaxLength(100);
        builder.HasOne(e => e.JobSafetyAssessment).WithMany(e => e.ControlMeasures).HasForeignKey(e => e.JobSafetyAssessmentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.Hazard).WithMany(e => e.ControlMeasures).HasForeignKey(e => e.HazardId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}

public class IncidentConfiguration : IEntityTypeConfiguration<Incident>
{
    public void Configure(EntityTypeBuilder<Incident> builder)
    {
        builder.ToTable("Incidents");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.Severity).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(e => new { e.PlantId, e.Status, e.OccurredAt });
        builder.HasOne(e => e.Company).WithMany(e => e.Incidents).HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Plant).WithMany(e => e.Incidents).HasForeignKey(e => e.PlantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Department).WithMany(e => e.Incidents).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ReportedByUser).WithMany().HasForeignKey(e => e.ReportedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.InvestigatedByUser).WithMany().HasForeignKey(e => e.InvestigatedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}

public class NearMissConfiguration : IEntityTypeConfiguration<NearMiss>
{
    public void Configure(EntityTypeBuilder<NearMiss> builder)
    {
        builder.ToTable("NearMisses");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.Department).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Location).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Section).IsRequired().HasMaxLength(200);
        builder.Property(e => e.InvestigationNotes).HasMaxLength(4000);
        builder.Property(e => e.RootCause).HasMaxLength(2000);
        builder.Property(e => e.CorrectiveActionPlan).HasMaxLength(2000);
        builder.Property(e => e.ClosureNotes).HasMaxLength(2000);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Category).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.RootCauseCategory).HasConversion<string>().HasMaxLength(50);
        builder.HasIndex(e => e.Status);
        builder.HasOne(e => e.Company).WithMany(e => e.NearMisses).HasForeignKey(e => e.CompanyId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.Plant).WithMany(e => e.NearMisses).HasForeignKey(e => e.PlantId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.OrganizationDepartment).WithMany(e => e.NearMisses).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ReportedByUser).WithMany().HasForeignKey(e => e.ReportedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.InvestigatorUser).WithMany().HasForeignKey(e => e.InvestigatorUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ResponsiblePersonUser).WithMany().HasForeignKey(e => e.ResponsiblePersonUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.PotentialRiskLevel).WithMany().HasForeignKey(e => e.PotentialRiskLevelId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}

public class CorrectiveActionConfiguration : IEntityTypeConfiguration<CorrectiveAction>
{
    public void Configure(EntityTypeBuilder<CorrectiveAction> builder)
    {
        builder.ToTable("CorrectiveActions");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);
        builder.HasCheckConstraint("CK_CorrectiveActions_IncidentOrNearMiss",
            "([IncidentId] IS NOT NULL AND [NearMissId] IS NULL) OR ([IncidentId] IS NULL AND [NearMissId] IS NOT NULL)");
        builder.HasOne(e => e.Incident).WithMany(e => e.CorrectiveActions).HasForeignKey(e => e.IncidentId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.NearMiss).WithMany(e => e.CorrectiveActions).HasForeignKey(e => e.NearMissId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.AssignedToUser).WithMany().HasForeignKey(e => e.AssignedToUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.VerifiedByUser).WithMany().HasForeignKey(e => e.VerifiedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RelatedEntityType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.FileName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.StoragePath).IsRequired().HasMaxLength(1000);
        builder.HasIndex(e => new { e.RelatedEntityType, e.RelatedEntityId });
        builder.ConfigureAuditFields();
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Action).IsRequired().HasMaxLength(100);
        builder.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.OldValues).HasColumnType("nvarchar(max)");
        builder.Property(e => e.NewValues).HasColumnType("nvarchar(max)");
        builder.HasIndex(e => new { e.EntityType, e.EntityId, e.Timestamp });
        builder.HasOne(e => e.User).WithMany().HasForeignKey(e => e.UserId).OnDelete(DeleteBehavior.Restrict);
        builder.Property(e => e.CreatedDate).IsRequired();
        builder.Property(e => e.CreatedBy).IsRequired().HasMaxLength(256);
        builder.Property(e => e.ModifiedBy).HasMaxLength(256);
        builder.Property(e => e.IsDeleted).HasDefaultValue(false);
    }
}
