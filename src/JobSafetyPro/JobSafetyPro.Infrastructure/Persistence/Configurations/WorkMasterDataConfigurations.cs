using JobSafetyPro.Domain.Entities.MasterData;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSafetyPro.Infrastructure.Persistence.Configurations;

public class WorkDepartmentConfiguration : IEntityTypeConfiguration<WorkDepartment>
{
    public void Configure(EntityTypeBuilder<WorkDepartment> builder)
    {
        builder.ToTable("WorkDepartments");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.ConfigureAuditFields();
    }
}

public class WorkLocationConfiguration : IEntityTypeConfiguration<WorkLocation>
{
    public void Configure(EntityTypeBuilder<WorkLocation> builder)
    {
        builder.ToTable("WorkLocations");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.ConfigureAuditFields();
    }
}

public class WorkSectionConfiguration : IEntityTypeConfiguration<WorkSection>
{
    public void Configure(EntityTypeBuilder<WorkSection> builder)
    {
        builder.ToTable("WorkSections");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.HasIndex(e => e.Name).IsUnique();
        builder.ConfigureAuditFields();
    }
}
