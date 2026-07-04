using JSP.Domain.Entities.Identity;
using JSP.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSP.Infrastructure.Persistence.Configurations.Identity;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Email).IsRequired().HasMaxLength(256);
        builder.Property(e => e.PasswordHash).IsRequired().HasMaxLength(512);
        builder.Property(e => e.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.LastName).IsRequired().HasMaxLength(100);
        builder.Property(e => e.EmployeeNumber).HasMaxLength(50);
        builder.Property(e => e.IsActive).HasDefaultValue(true);

        builder.HasIndex(e => new { e.CompanyId, e.Email })
            .IsUnique()
            .HasDatabaseName("UX_Users_CompanyId_Email");

        builder.HasIndex(e => e.PlantId).HasDatabaseName("IX_Users_PlantId");

        builder.HasOne(e => e.Company)
            .WithMany(e => e.Users)
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Plant)
            .WithMany(e => e.Users)
            .HasForeignKey(e => e.PlantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Department)
            .WithMany(e => e.Users)
            .HasForeignKey(e => e.DepartmentId)
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
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasIndex(e => new { e.CompanyId, e.Name })
            .IsUnique()
            .HasDatabaseName("UX_Roles_CompanyId_Name");

        builder.HasOne(e => e.Company)
            .WithMany()
            .HasForeignKey(e => e.CompanyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditFields();
    }
}

public class PermissionConfiguration : IEntityTypeConfiguration<Permission>
{
    public void Configure(EntityTypeBuilder<Permission> builder)
    {
        builder.ToTable("Permissions");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Code).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Module).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Description).HasMaxLength(500);

        builder.HasIndex(e => e.Code).IsUnique().HasDatabaseName("UX_Permissions_Code");

        builder.ConfigureAuditFields();
    }
}

public class UserRoleConfiguration : IEntityTypeConfiguration<UserRole>
{
    public void Configure(EntityTypeBuilder<UserRole> builder)
    {
        builder.ToTable("UserRoles");
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.UserId, e.RoleId, e.PlantId })
            .IsUnique()
            .HasDatabaseName("UX_UserRoles_UserId_RoleId_PlantId");

        builder.HasOne(e => e.User)
            .WithMany(e => e.UserRoles)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Role)
            .WithMany(e => e.UserRoles)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(e => e.Plant)
            .WithMany()
            .HasForeignKey(e => e.PlantId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditFields();
    }
}

public class RolePermissionConfiguration : IEntityTypeConfiguration<RolePermission>
{
    public void Configure(EntityTypeBuilder<RolePermission> builder)
    {
        builder.ToTable("RolePermissions");
        builder.HasKey(e => e.Id);

        builder.HasIndex(e => new { e.RoleId, e.PermissionId })
            .IsUnique()
            .HasDatabaseName("UX_RolePermissions_RoleId_PermissionId");

        builder.HasOne(e => e.Role)
            .WithMany(e => e.RolePermissions)
            .HasForeignKey(e => e.RoleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(e => e.Permission)
            .WithMany(e => e.RolePermissions)
            .HasForeignKey(e => e.PermissionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ConfigureAuditFields();
    }
}

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Title).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Body).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.RelatedEntityType).HasMaxLength(100);

        builder.HasIndex(e => new { e.UserId, e.IsRead, e.CreatedDate })
            .HasDatabaseName("IX_Notifications_UserId_IsRead_CreatedDate");

        builder.HasOne(e => e.User)
            .WithMany(e => e.Notifications)
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ConfigureAuditFields();
    }
}
