// DTOs/Audit/AuthorizationAuditPayload.cs
using System;

namespace t5f25sdprojectone_projectsplus.Models.Audit
{
    /// <summary>
    /// Strongly-typed payload for authorization audit events.
    /// Serialized into AuditEvent.PayloadJson by AuditWriter.
    /// </summary>
    public class AuthorizationAuditPayload
    {
        public string Action { get; set; } = null!;
        public string ResourceType { get; set; } = null!;
        public long? ResourceId { get; set; }
        public string Decision { get; set; } = null!; // e.g., "Allow" or "Deny"
        public string? Reason { get; set; } // optional explanation for decision
        public Guid CorrelationId { get; set; }
        public DateTimeOffset RecordedAt { get; set; } = DateTimeOffset.UtcNow;
        public long? ActorId { get; set; }
        public long? SystemTypeId { get; set; }
        public object? Context { get; set; } // free-form additional context (e.g., role list, policy flags)
    }
}
