using JobSafetyPro.Domain.Entities.Ppe;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JobSafetyPro.Infrastructure.Persistence.Configurations;

public class PpeCatalogueItemConfiguration : IEntityTypeConfiguration<PpeCatalogueItem>
{
    public void Configure(EntityTypeBuilder<PpeCatalogueItem> builder)
    {
        builder.ToTable("PpeCatalogueItems");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.ItemName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Category).HasConversion<string>().HasMaxLength(50);
        builder.Property(e => e.Description).HasMaxLength(2000);
        builder.HasIndex(e => e.ItemName);
        builder.ConfigureAuditFields();
    }
}

public class PpeRequestConfiguration : IEntityTypeConfiguration<PpeRequest>
{
    public void Configure(EntityTypeBuilder<PpeRequest> builder)
    {
        builder.ToTable("PpeRequests");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.RequestNumber).IsRequired().HasMaxLength(30);
        builder.HasIndex(e => e.RequestNumber).IsUnique();
        builder.Property(e => e.EmployeeName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Department).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Location).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Section).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Reason).IsRequired().HasMaxLength(2000);
        builder.Property(e => e.Comments).HasMaxLength(2000);
        builder.Property(e => e.RejectionReason).HasMaxLength(2000);
        builder.Property(e => e.CollectedByEmployee).HasMaxLength(200);
        builder.Property(e => e.EmployeeSignature).HasMaxLength(500);
        builder.Property(e => e.SafetyOfficerSignature).HasMaxLength(500);
        builder.Property(e => e.DispatchNotes).HasMaxLength(2000);
        builder.Property(e => e.Priority).HasConversion<string>().HasMaxLength(20);
        builder.Property(e => e.Status).HasConversion<string>().HasMaxLength(30);
        builder.HasIndex(e => e.Status);
        builder.HasIndex(e => e.RequiredByDate);
        builder.HasIndex(e => e.EmployeeUserId);
        builder.HasOne(e => e.EmployeeUser).WithMany().HasForeignKey(e => e.EmployeeUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.RequestedByUser).WithMany().HasForeignKey(e => e.RequestedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.IssuedByUser).WithMany().HasForeignKey(e => e.IssuedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.ApprovedByUser).WithMany().HasForeignKey(e => e.ApprovedByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.WorkDepartment).WithMany().HasForeignKey(e => e.WorkDepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(e => e.CatalogueItem).WithMany(e => e.Requests).HasForeignKey(e => e.PpeCatalogueItemId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}

public class PpeRequestStatusHistoryConfiguration : IEntityTypeConfiguration<PpeRequestStatusHistory>
{
    public void Configure(EntityTypeBuilder<PpeRequestStatusHistory> builder)
    {
        builder.ToTable("PpeRequestStatusHistories");
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Action).IsRequired().HasMaxLength(100);
        builder.Property(e => e.ActionByUserName).IsRequired().HasMaxLength(200);
        builder.Property(e => e.Comments).HasMaxLength(2000);
        builder.Property(e => e.OldStatus).HasConversion<string>().HasMaxLength(30);
        builder.Property(e => e.NewStatus).HasConversion<string>().HasMaxLength(30);
        builder.HasOne(e => e.Request).WithMany(e => e.StatusHistory).HasForeignKey(e => e.PpeRequestId).OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(e => e.ActionByUser).WithMany().HasForeignKey(e => e.ActionByUserId).OnDelete(DeleteBehavior.Restrict);
        builder.ConfigureAuditFields();
    }
}
