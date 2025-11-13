// Services/Audit/AuditWriter.cs
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;

namespace t5f25sdprojectone_projectsplus.Models.Audit
{
    /// <summary>
    /// Durable audit writer that writes AuditEvent rows and emits structured logs.
    /// - Supports transactional writes when an IDbContextTransaction is provided.
    /// - Offers a best-effort non-transactional path for high-throughput scenarios.
    /// - Keeps implementation small and explicit so domain services can call it inside their DbContext transaction.
    /// </summary>
    public class AuditWriter : IAuditWriter
    {
        private readonly DbContext _db;
        private readonly ILogger<AuditWriter> _logger;
        private readonly JsonSerializerOptions _jsonOptions;

        // Non-blocking write retries for best-effort writes
        private const int BestEffortRetryCount = 3;
        private static readonly TimeSpan BestEffortRetryDelay = TimeSpan.FromMilliseconds(200);

        public AuditWriter(DbContext dbContext, ILogger<AuditWriter> logger)
        {
            _db = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = false,
                DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull
            };
        }

        public async Task WriteAsync(AuditEvent auditEvent, CancellationToken cancellationToken = default)
        {
            if (auditEvent is null) throw new ArgumentNullException(nameof(auditEvent));

            // Best-effort, small retry loop to avoid transient failures blocking critical flows.
            for (int attempt = 1; attempt <= BestEffortRetryCount; attempt++)
            {
                try
                {
                    // Use a fresh DbContext instance if caller supplies one via DI scope; we rely on injected _db.
                    _db.Set<AuditEvent>().Add(auditEvent);
                    await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                    // Structured log for downstream sinks
                    _logger.LogInformation("AuditEvent written: {EventType} Correlation={CorrelationId} Actor={ActorId} Target={Target}",
                        auditEvent.EventType, auditEvent.CorrelationId, auditEvent.ActorId, $"{auditEvent.TargetType}:{auditEvent.TargetId}");

                    return;
                }
                catch (Exception ex) when (attempt < BestEffortRetryCount)
                {
                    _logger.LogWarning(ex, "Transient error writing audit event (attempt {Attempt}), will retry", attempt);
                    await Task.Delay(BestEffortRetryDelay, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // Last-resort: log error and do not throw, preserving best-effort contract for non-transactional path.
                    _logger.LogError(ex, "Failed to write audit event after {Attempts} attempts. EventType={EventType} Correlation={CorrelationId}",
                        attempt, auditEvent.EventType, auditEvent.CorrelationId);
                    return;
                }
            }
        }

        public async Task WriteInTransactionAsync(AuditEvent auditEvent, IDbContextTransaction dbContextTransaction, CancellationToken cancellationToken = default)
        {
            if (auditEvent is null) throw new ArgumentNullException(nameof(auditEvent));
            if (dbContextTransaction is null) throw new ArgumentNullException(nameof(dbContextTransaction));

            // We expect that the provided DbContext (injected) is the same context that created the transaction.
            // Enlist the audit insert into the same transaction by using the same DbContext instance.
            try
            {
                _db.Set<AuditEvent>().Add(auditEvent);
                // SaveChanges will perform the insert and remain part of the ambient transaction.
                await _db.SaveChangesAsync(cancellationToken).ConfigureAwait(false);

                _logger.LogInformation("AuditEvent written in transaction: {EventType} Correlation={CorrelationId} Actor={ActorId}",
                    auditEvent.EventType, auditEvent.CorrelationId, auditEvent.ActorId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to write audit event inside transaction. EventType={EventType} Correlation={CorrelationId}",
                    auditEvent.EventType, auditEvent.CorrelationId);
                // Bubble up to let caller decide transaction rollback semantics.
                throw;
            }
        }

        public Task WriteAuthorizationAsync(
            string action,
            string resourceType,
            long? resourceId,
            long? actorId,
            long? systemTypeId,
            Guid correlationId,
            object payload,
            IDbContextTransaction? dbContextTransaction = null,
            CancellationToken cancellationToken = default)
        {
            var evt = new AuditEvent
            {
                EventType = "AuthorizationAudit",
                CorrelationId = correlationId == Guid.Empty ? Guid.NewGuid() : correlationId,
                ActorId = actorId,
                TargetId = resourceId,
                TargetType = resourceType,
                SystemTypeId = systemTypeId,
                OccurredAt = DateTimeOffset.UtcNow,
                PayloadJson = payload is null ? null : JsonSerializer.Serialize(payload, _jsonOptions),
                MetadataJson = JsonSerializer.Serialize(new
                {
                    action,
                    recordedAt = DateTimeOffset.UtcNow
                }, _jsonOptions),
                Version = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            if (dbContextTransaction != null)
            {
                return WriteInTransactionAsync(evt, dbContextTransaction, cancellationToken);
            }

            return WriteAsync(evt, cancellationToken);
        }

        public Task WriteAuthenticationAsync(
            long? actorId,
            bool success,
            string reason,
            string? ipAddress,
            string? userAgent,
            Guid correlationId,
            object? payload = null,
            IDbContextTransaction? dbContextTransaction = null,
            CancellationToken cancellationToken = default)
        {
            var evt = new AuditEvent
            {
                EventType = "AuthenticationAudit",
                CorrelationId = correlationId == Guid.Empty ? Guid.NewGuid() : correlationId,
                ActorId = actorId,
                TargetId = actorId,
                TargetType = "User",
                SystemTypeId = null,
                OccurredAt = DateTimeOffset.UtcNow,
                PayloadJson = payload is null ? JsonSerializer.Serialize(new { success, reason, ipAddress, userAgent }, _jsonOptions) : JsonSerializer.Serialize(payload, _jsonOptions),
                MetadataJson = JsonSerializer.Serialize(new
                {
                    recordedAt = DateTimeOffset.UtcNow
                }, _jsonOptions),
                Version = 1,
                CreatedAt = DateTimeOffset.UtcNow
            };

            if (dbContextTransaction != null)
            {
                return WriteInTransactionAsync(evt, dbContextTransaction, cancellationToken);
            }

            return WriteAsync(evt, cancellationToken);
        }
    }
}
