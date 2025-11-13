// Configurations/Audit/AuditEventEntityTypeConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using t5f25sdprojectone_projectsplus.Models.Storage;    // adjust or remove if SystemType lives elsewhere
using t5f25sdprojectone_projectsplus.Models.SystemTypeID; // adjust or remove depending on actual SystemType type location

namespace t5f25sdprojectone_projectsplus.Models.Audit
{
    /// <summary>
    /// EF Core configuration for AuditEvent.
    /// - Table: audit_events
    /// - Explicit column names and constraint/index names to stabilize migrations.
    /// - Uses jsonb column type (Postgres). If you use SQL Server, change HasColumnType("jsonb") to nvarchar(max)
    ///   or remove HasColumnType so EF defaults are applied by provider.
    /// - Keeps indexes narrow and adds a composite actor+occurred_at index for common queries.
    /// </summary>
    public class AuditEventEntityTypeConfiguration : IEntityTypeConfiguration<AuditEvent>
    {
        public void Configure(EntityTypeBuilder<AuditEvent> builder)
        {
            builder.ToTable("audit_events");

            // Primary key
            builder.HasKey(a => a.Id).HasName("pk_audit_events");

            // Columns
            builder.Property(a => a.Id).HasColumnName("id").ValueGeneratedOnAdd();

            builder.Property(a => a.EventType)
                   .HasColumnName("event_type")
                   .HasMaxLength(200)
                   .IsRequired();

            builder.Property(a => a.CorrelationId)
                   .HasColumnName("correlation_id")
                   .IsRequired();

            builder.Property(a => a.ActorId)
                   .HasColumnName("actor_id");

            builder.Property(a => a.TargetId)
                   .HasColumnName("target_id");

            builder.Property(a => a.TargetType)
                   .HasColumnName("target_type")
                   .HasMaxLength(200);

            builder.Property(a => a.SystemTypeId)
                   .HasColumnName("system_type_id");

            // Foreign key to SystemType (restrict deletes to keep audit rows immutable)
            // Ensure the SystemType CLR type and namespace in the using directives above match your project layout.
            builder.HasOne<SystemType>()
                   .WithMany()
                   .HasForeignKey(nameof(AuditEvent.SystemTypeId))
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("fk_audit_event_system_type_id_system_types");

            builder.Property(a => a.OccurredAt)
                   .HasColumnName("occurred_at")
                   .IsRequired();

            // Payload / metadata JSON columns
            // NOTE: HasColumnType("jsonb") is Postgres-specific. If your project targets SQL Server use "nvarchar(max)".
            builder.Property(a => a.PayloadJson)
                   .HasColumnName("payload_json")
                   .HasColumnType("jsonb");

            builder.Property(a => a.MetadataJson)
                   .HasColumnName("metadata_json")
                   .HasColumnType("jsonb");

            builder.Property(a => a.Version)
                   .HasColumnName("version")
                   .IsRequired();

            builder.Property(a => a.CreatedAt)
                   .HasColumnName("created_at")
                   .IsRequired();

            // Indexes - narrow, common access patterns
            builder.HasIndex(a => a.EventType).HasDatabaseName("ix_audit_events_event_type");
            builder.HasIndex(a => a.ActorId).HasDatabaseName("ix_audit_events_actor_id");
            builder.HasIndex(a => a.CorrelationId).HasDatabaseName("ix_audit_events_correlation_id");
            builder.HasIndex(a => a.OccurredAt).HasDatabaseName("ix_audit_events_occurred_at");
            builder.HasIndex(a => a.TargetId).HasDatabaseName("ix_audit_events_target_id");

            // Composite index for common filter: actor + time range
            builder.HasIndex(a => new { a.ActorId, a.OccurredAt })
                   .HasDatabaseName("ix_audit_events_actor_id_occurred_at");

            // Keep the entity append-only from the application perspective.
            // Optional: If you want to enforce a foreign key to Users (ActorId), enable the following when User entity exists:
            // builder.HasOne<User>()
            //        .WithMany()
            //        .HasForeignKey(nameof(AuditEvent.ActorId))
            //        .OnDelete(DeleteBehavior.Restrict)
            //        .HasConstraintName("fk_audit_events_actor_id_users");

            // Considerations:
            // - Keep payload_json and metadata_json off the default SELECT for list queries to reduce IO.
            // - For very large retention volumes, consider partitioning by occurred_at in DB migrations/ops.
        }
    }
}
