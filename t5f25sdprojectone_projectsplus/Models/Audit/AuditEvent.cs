// Models/Audit/AuditEvent.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace t5f25sdprojectone_projectsplus.Models.Audit
{
    /// <summary>
    /// Append-only audit event envelope. Designed to be compact, queryable and immutable.
    /// </summary>
    [Table("audit_events")]
    public class AuditEvent
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        // e.g. AuthorizationAudit, AuthenticationAudit, ProvisioningStep, ProjectAudit
        [Required]
        [MaxLength(200)]
        [Column("event_type")]
        public string EventType { get; set; } = null!;

        // Correlation id shared across request/workflow
        [Required]
        [Column("correlation_id")]
        public Guid CorrelationId { get; set; } = Guid.Empty;

        // Actor who triggered the event (nullable for system/anonymous events)
        [Column("actor_id")]
        public long? ActorId { get; set; }

        // Optional target resource id and a simple typed name for quick filtering
        [Column("target_id")]
        public long? TargetId { get; set; }

        [MaxLength(200)]
        [Column("target_type")]
        public string? TargetType { get; set; }

        // If applicable, the SystemTypeId for the resource or actor (helps policy queries)
        [Column("system_type_id")]
        public long? SystemTypeId { get; set; }

        // Event timestamp in UTC
        [Required]
        [Column("occurred_at")]
        public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;

        // Typed payload for the event (jsonb)
        [Column("payload_json", TypeName = "jsonb")]
        public string? PayloadJson { get; set; }

        // Freeform metadata (jsonb) for indexing hints, versioning, or envelope data
        [Column("metadata_json", TypeName = "jsonb")]
        public string? MetadataJson { get; set; }

        // Immutable version for schema evolution of payloads
        [Required]
        [Column("version")]
        public int Version { get; set; } = 1;

        // Insertion timestamp (write-time)
        [Required]
        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        public override string ToString()
        {
            // single-line, traceable representation suitable for logs
            return $"AuditEvent:{Id}:type:{EventType}:corr:{CorrelationId}:actor:{ActorId?.ToString() ?? "n/a"}:target:{TargetType ?? "n/a"}:{TargetId?.ToString() ?? "n/a"}:at:{OccurredAt:u}:v{Version}";
        }
    }
}
