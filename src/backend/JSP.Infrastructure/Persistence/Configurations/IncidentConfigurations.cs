using JSP.Domain.Entities.Incidents;
using JSP.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSP.Infrastructure.Persistence.Configurations.Incidents;

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

        builder.HasIndex(e => new { e.PlantId, e.Status, e.OccurredAt })
            .HasDatabaseName("IX_Incidents_PlantId_Status_OccurredAt");

        builder.HasOne(e => e.Company)
            .WithMany(e => e.Incidents)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Plant)
            .WithMany(e => e.Incidents)
            .HasForeignKey(e => e.PlantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
            .WithMany(e => e.Incidents)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReportedByUser)
            .WithMany()
            .HasForeignKey(e => e.ReportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.InvestigatedByUser)
            .WithMany()
            .HasForeignKey(e => e.InvestigatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.CorrectiveActions)
            .WithOne(e => e.Incident)
            .HasForeignKey(e => e.IncidentId)
            .OnDelete(DeleteBehavior.Cascade);

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

        builder.HasIndex(e => new { e.PlantId, e.OccurredAt })
            .HasDatabaseName("IX_NearMisses_PlantId_OccurredAt");

        builder.HasOne(e => e.Company)
            .WithMany(e => e.NearMisses)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Plant)
            .WithMany(e => e.NearMisses)
            .HasForeignKey(e => e.PlantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
            .WithMany(e => e.NearMisses)
            .HasForeignKey(e => e.DepartmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.ReportedByUser)
            .WithMany()
            .HasForeignKey(e => e.ReportedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.PotentialRiskLevel)
            .WithMany()
            .HasForeignKey(e => e.PotentialRiskLevelId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(e => e.CorrectiveActions)
            .WithOne(e => e.NearMiss)
            .HasForeignKey(e => e.NearMissId)
            .OnDelete(DeleteBehavior.Cascade);

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

        builder.HasIndex(e => new { e.AssignedToUserId, e.Status, e.DueDate })
            .HasDatabaseName("IX_CorrectiveActions_AssignedToUserId_Status_DueDate");

        builder.HasOne(e => e.AssignedToUser)
            .WithMany()
            .HasForeignKey(e => e.AssignedToUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.VerifiedByUser)
            .WithMany()
            .HasForeignKey(e => e.VerifiedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasCheckConstraint(
            "CK_CorrectiveActions_IncidentOrNearMiss",
            "([IncidentId] IS NOT NULL AND [NearMissId] IS NULL) OR ([IncidentId] IS NULL AND [NearMissId] IS NOT NULL)");

        builder.ConfigureAuditFields();
    }
}
