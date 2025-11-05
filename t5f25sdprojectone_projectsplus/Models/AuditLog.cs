using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models
{
    public class AuditLog
    {
        public long Id { get; set; }

        // What kind of entity was acted on (e.g., "Project", "Grant", "User")
        public string EntityType { get; set; } = null!;

        // Optional numeric id of the entity instance
        public long? EntityId { get; set; }

        // Action performed (e.g., "Create", "Update", "Delete", "PermissionChange")
        public string Action { get; set; } = null!;

        // Who performed it (nullable for system actions)
        public long? PerformedBy { get; set; }
        public User? PerformedByUser { get; set; }

        // Large context / payload pointer stored in S3 to avoid DB bloat
        public string? PayloadS3Key { get; set; }

        // When the action occurred
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class AuditLogEntityTypeConfiguration : IEntityTypeConfiguration<AuditLog>
    {
        public void Configure(EntityTypeBuilder<AuditLog> builder)
        {
            builder.ToTable("AuditLogs");

            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).ValueGeneratedOnAdd();

            builder.Property(a => a.EntityType)
                   .HasColumnType("nvarchar(100)")
                   .IsRequired();

            builder.Property(a => a.EntityId)
                   .HasColumnType("bigint");

            builder.Property(a => a.Action)
                   .HasColumnType("nvarchar(100)")
                   .IsRequired();

            builder.Property(a => a.PerformedBy)
                   .HasColumnType("bigint");

            builder.HasOne(a => a.PerformedByUser)
                   .WithMany()
                   .HasForeignKey(a => a.PerformedBy)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.Property(a => a.PayloadS3Key)
                   .HasColumnType("nvarchar(1000)");

            builder.Property(a => a.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.HasIndex(a => new { a.EntityType, a.EntityId })
                   .HasDatabaseName("IX_AuditLogs_Entity");
            builder.HasIndex(a => a.CreatedAt)
                   .HasDatabaseName("IX_AuditLogs_CreatedAt");
        }
    }
}
