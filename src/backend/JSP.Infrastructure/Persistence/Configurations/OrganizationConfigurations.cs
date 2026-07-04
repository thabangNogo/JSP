using JSP.Domain.Common;
using JSP.Domain.Entities.Organization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSP.Infrastructure.Persistence.Configurations;

public static class EntityConfigurationExtensions
{
    public static void ConfigureAuditFields<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder.Property(e => e.CreatedDate).IsRequired();
        builder.Property(e => e.CreatedBy).IsRequired().HasMaxLength(256);
        builder.Property(e => e.ModifiedBy).HasMaxLength(256);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
        builder.HasQueryFilter(e => !e.IsDeleted);
    }
}

public class CompanyConfiguration : IEntityTypeConfiguration<Company>
{
    public void Configure(EntityTypeBuilder<Company> builder)
    {
        builder.ToTable("Companies");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Name).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Code).IsRequired().HasMaxLength(50);
        builder.Property(e => e.IsActive).HasDefaultValue(true);

        builder.HasIndex(e => e.Code).IsUnique().HasDatabaseName("UX_Companies_Code");

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
        builder.Property(e => e.TimeZone).IsRequired().HasMaxLength(100);
        builder.Property(e => e.IsActive).HasDefaultValue(true);

        builder.HasIndex(e => new { e.CompanyId, e.Code })
            .IsUnique()
            .HasDatabaseName("UX_Plants_CompanyId_Code");

        builder.HasOne(e => e.Company)
            .WithMany(e => e.Plants)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

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
        builder.Property(e => e.IsActive).HasDefaultValue(true);

        builder.HasIndex(e => new { e.PlantId, e.Code })
            .IsUnique()
            .HasDatabaseName("UX_Departments_PlantId_Code");

        builder.HasOne(e => e.Plant)
            .WithMany(e => e.Departments)
            .HasForeignKey(e => e.PlantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditFields();
    }
}
