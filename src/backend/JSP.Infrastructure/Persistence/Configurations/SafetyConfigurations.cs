using JSP.Domain.Entities.Safety;
using JSP.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSP.Infrastructure.Persistence.Configurations.Safety;

public class JobSafetyAssessmentConfiguration : IEntityTypeConfiguration<JobSafetyAssessment>
{
    public void Configure(EntityTypeBuilder<JobSafetyAssessment> builder)
    {
        builder.ToTable("JobSafetyAssessments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.JobDescription).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(50);

        builder.HasIndex(e => new { e.PlantId, e.Status })
            .HasDatabaseName("IX_JobSafetyAssessments_PlantId_Status");

        builder.HasIndex(e => new { e.CompanyId, e.IsDeleted })
            .HasDatabaseName("IX_JobSafetyAssessments_CompanyId_IsDeleted");

        builder.HasOne(e => e.Company)
            .WithMany(e => e.JobSafetyAssessments)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Plant)
            .WithMany(e => e.JobSafetyAssessments)
            .HasForeignKey(e => e.PlantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
            .WithMany(e => e.JobSafetyAssessments)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.AssessmentHazards)
            .WithOne(e => e.JobSafetyAssessment)
            .HasForeignKey(e => e.JobSafetyAssessmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditFields();
    }
}

public class HazardCategoryConfiguration : IEntityTypeConfiguration<HazardCategory>
{
    public void Configure(EntityTypeBuilder<HazardCategory> builder)
    {
        builder.ToTable("HazardCategories");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(1000);

        builder.HasIndex(e => e.Code).IsUnique().HasDatabaseName("UX_HazardCategories_Code");

        builder.ConfigureAuditFields();
    }
}

public class HazardConfiguration : IEntityTypeConfiguration<Hazard>
{
    public void Configure(EntityTypeBuilder<Hazard> builder)
    {
        builder.ToTable("Hazards");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(2000);

        builder.HasIndex(e => e.Code).IsUnique().HasDatabaseName("UX_Hazards_Code");

        builder.HasOne(e => e.HazardCategory)
            .WithMany(e => e.Hazards)
            .HasForeignKey(e => e.HazardCategoryId)
            .OnDelete(DeleteBehavior.Restrict);

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
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.ColorHex).IsRequired().HasMaxLength(7);

        builder.HasIndex(e => e.Code).IsUnique().HasDatabaseName("UX_RiskLevels_Code");

        builder.ConfigureAuditFields();
    }
}

public class AssessmentHazardConfiguration : IEntityTypeConfiguration<AssessmentHazard>
{
    public void Configure(EntityTypeBuilder<AssessmentHazard> builder)
    {
        builder.ToTable("AssessmentHazards");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description).IsRequired().HasMaxLength(2000);

        builder.HasOne(e => e.Hazard)
            .WithMany(e => e.AssessmentHazards)
            .HasForeignKey(e => e.HazardId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.InitialRiskLevel)
            .WithMany(e => e.InitialAssessmentHazards)
            .HasForeignKey(e => e.InitialRiskLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ResidualRiskLevel)
            .WithMany(e => e.ResidualAssessmentHazards)
            .HasForeignKey(e => e.ResidualRiskLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.AssessmentControls)
            .WithOne(e => e.AssessmentHazard)
            .HasForeignKey(e => e.AssessmentHazardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditFields();
    }
}

public class ControlMeasureConfiguration : IEntityTypeConfiguration<ControlMeasure>
{
    public void Configure(EntityTypeBuilder<ControlMeasure> builder)
    {
        builder.ToTable("ControlMeasures");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(2000);

        builder.HasIndex(e => e.Code).IsUnique().HasDatabaseName("UX_ControlMeasures_Code");

        builder.ConfigureAuditFields();
    }
}

public class HierarchyOfControlConfiguration : IEntityTypeConfiguration<HierarchyOfControl>
{
    public void Configure(EntityTypeBuilder<HierarchyOfControl> builder)
    {
        builder.ToTable("HierarchyOfControls");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).HasMaxLength(1000);

        builder.HasIndex(e => e.Code).IsUnique().HasDatabaseName("UX_HierarchyOfControls_Code");

        builder.ConfigureAuditFields();
    }
}

public class AssessmentControlConfiguration : IEntityTypeConfiguration<AssessmentControl>
{
    public void Configure(EntityTypeBuilder<AssessmentControl> builder)
    {
        builder.ToTable("AssessmentControls");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Description).IsRequired().HasMaxLength(2000);

        builder.HasOne(e => e.ControlMeasure)
            .WithMany(e => e.AssessmentControls)
            .HasForeignKey(e => e.ControlMeasureId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.HierarchyOfControl)
            .WithMany(e => e.AssessmentControls)
            .HasForeignKey(e => e.HierarchyOfControlId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditFields();
    }
}

public class GoldenRuleConfiguration : IEntityTypeConfiguration<GoldenRule>
{
    public void Configure(EntityTypeBuilder<GoldenRule> builder)
    {
        builder.ToTable("GoldenRules");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Description).IsRequired().HasMaxLength(4000);
        builder.Property(e => e.IsActive).HasDefaultValue(true);

        builder.HasIndex(e => new { e.CompanyId, e.PlantId, e.IsActive })
            .HasDatabaseName("IX_GoldenRules_CompanyId_PlantId_IsActive");

        builder.HasOne(e => e.Company)
            .WithMany(e => e.GoldenRules)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Plant)
            .WithMany(e => e.GoldenRules)
            .HasForeignKey(e => e.PlantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditFields();
    }
}
