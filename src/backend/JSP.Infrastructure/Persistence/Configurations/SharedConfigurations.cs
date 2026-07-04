using JSP.Domain.Entities.Shared;
using JSP.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JSP.Infrastructure.Persistence.Configurations.Shared;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.RelatedEntityType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.FileName).IsRequired().HasMaxLength(255);
        builder.Property(e => e.ContentType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.StoragePath).IsRequired().HasMaxLength(1000);

        builder.HasIndex(e => new { e.RelatedEntityType, e.RelatedEntityId })
            .HasDatabaseName("IX_Attachments_RelatedEntity");

        builder.ConfigureAuditFields();
    }
}

public class PhotoConfiguration : IEntityTypeConfiguration<Photo>
{
    public void Configure(EntityTypeBuilder<Photo> builder)
    {
        builder.ToTable("Photos");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.RelatedEntityType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.StoragePath).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.Caption).HasMaxLength(500);

        builder.HasIndex(e => new { e.RelatedEntityType, e.RelatedEntityId })
            .HasDatabaseName("IX_Photos_RelatedEntity");

        builder.ConfigureAuditFields();
    }
}

public class SignatureConfiguration : IEntityTypeConfiguration<Signature>
{
    public void Configure(EntityTypeBuilder<Signature> builder)
    {
        builder.ToTable("Signatures");
        builder.HasKey(e => e.Id);

        builder.Property(e => e.RelatedEntityType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.SignatureDataPath).IsRequired().HasMaxLength(1000);
        builder.Property(e => e.IpAddress).HasMaxLength(45);

        builder.HasIndex(e => new { e.RelatedEntityType, e.RelatedEntityId })
            .HasDatabaseName("IX_Signatures_RelatedEntity");

        builder.HasOne(e => e.SignedByUser)
            .WithMany()
            .HasForeignKey(e => e.SignedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

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
        builder.Property(e => e.IpAddress).HasMaxLength(45);

        builder.HasIndex(e => new { e.EntityType, e.EntityId, e.Timestamp })
            .HasDatabaseName("IX_AuditLogs_EntityType_EntityId_Timestamp");

        builder.HasIndex(e => new { e.CompanyId, e.Timestamp })
            .HasDatabaseName("IX_AuditLogs_CompanyId_Timestamp");

        builder.HasOne(e => e.User)
            .WithMany()
            .HasForeignKey(e => e.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Audit logs are append-only; no soft-delete filter.
        builder.Property(e => e.CreatedDate).IsRequired();
        builder.Property(e => e.CreatedBy).IsRequired().HasMaxLength(256);
        builder.Property(e => e.ModifiedBy).HasMaxLength(256);
        builder.Property(e => e.IsDeleted).IsRequired().HasDefaultValue(false);
    }
}
