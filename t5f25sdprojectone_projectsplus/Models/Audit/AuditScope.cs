// Helpers/Audit/AuditScope.cs
using System;
using Microsoft.AspNetCore.Http;

namespace t5f25sdprojectone_projectsplus.Models.Audit
{
    /// <summary>
    /// Lightweight request-scoped audit context holder.
    /// Stored in HttpContext.Items using HttpContextItemKey.
    /// Services should read values from AuditScope.Current (via IHttpContextAccessor) to enrich audits.
    /// </summary>
    public sealed class AuditScope
    {
        public const string HttpContextItemKey = "ProjectsPlus.AuditScope";

        public Guid CorrelationId { get; }
        public long? ActorId { get; private set; }
        public long? SystemTypeId { get; private set; }
        public string? ActorIpAddress { get; private set; }
        public string? UserAgent { get; private set; }

        private AuditScope(Guid correlationId, long? actorId = null, long? systemTypeId = null, string? ip = null, string? ua = null)
        {
            CorrelationId = correlationId;
            ActorId = actorId;
            SystemTypeId = systemTypeId;
            ActorIpAddress = ip;
            UserAgent = ua;
        }

        /// <summary>
        /// Creates a default scope with correlation id populated.
        /// </summary>
        public static AuditScope CreateDefault(Guid correlationId) => new AuditScope(correlationId);

        /// <summary>
        /// Read the current AuditScope from HttpContext via IHttpContextAccessor.
        /// Returns null if no HttpContext or no AuditScope present.
        /// </summary>
        public static AuditScope? Current(IHttpContextAccessor httpContextAccessor)
        {
            var ctx = httpContextAccessor?.HttpContext;
            if (ctx == null) return null;
            if (ctx.Items.TryGetValue(HttpContextItemKey, out var value) && value is AuditScope scope)
            {
                return scope;
            }
            return null;
        }

        /// <summary>
        /// Mutating setters are intentionally limited to internal use by middleware/authorization components.
        /// Callers outside that flow should treat AuditScope as immutable.
        /// </summary>
        public void SetActor(long actorId)
        {
            ActorId = actorId;
        }

        public void SetSystemType(long systemTypeId)
        {
            SystemTypeId = systemTypeId;
        }

        public void SetRequestMetadata(string? ipAddress, string? userAgent)
        {
            ActorIpAddress = ipAddress;
            UserAgent = userAgent;
        }

        public override string ToString()
        {
            return $"AuditScope:Corr={CorrelationId}:Actor={ActorId?.ToString() ?? "n/a"}:SysType={SystemTypeId?.ToString() ?? "n/a"}";
        }
    }
}
