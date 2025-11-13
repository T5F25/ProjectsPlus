// Services/Audit/IAuditWriter.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage;

namespace t5f25sdprojectone_projectsplus.Models.Audit
{
    /// <summary>
    /// Small, explicit contract for writing audit events.
    /// Implementations must support transactional writes (when a Db transaction is provided)
    /// and non-transactional best-effort writes for high-throughput scenarios.
    /// </summary>
    public interface IAuditWriter
    {
        /// <summary>
        /// Write an audit event outside of any ambient transaction.
        /// Best-effort: should retry transient failures but must not block domain-critical flows indefinitely.
        /// </summary>
        Task WriteAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default);

        /// <summary>
        /// Write an audit event using the supplied database transaction / ambient context.
        /// When provided, the implementation must enlist the insert in the same transactional unit
        /// so domain state and audit row persist atomically.
        /// </summary>
        Task WriteInTransactionAsync(AuditEvent auditEvent, IDbContextTransaction dbContextTransaction, CancellationToken cancellationToken = default);

        /// <summary>
        /// Convenience overload for common audits: Authorization events.
        /// Implementations should produce a small structured payload in PayloadJson.
        /// </summary>
        Task WriteAuthorizationAsync(
            string action,
            string resourceType,
            long? resourceId,
            long? actorId,
            long? systemTypeId,
            Guid correlationId,
            object payload,
            IDbContextTransaction? dbContextTransaction = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Convenience overload for authentication (sign-in/out) audits.
        /// </summary>
        Task WriteAuthenticationAsync(
            long? actorId,
            bool success,
            string reason,
            string? ipAddress,
            string? userAgent,
            Guid correlationId,
            object? payload = null,
            IDbContextTransaction? dbContextTransaction = null,
            CancellationToken cancellationToken = default);
    }
}
