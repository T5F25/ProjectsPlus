// Repositories/Audit/IAuditQueryRepository.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using t5f25sdprojectone_projectsplus.Models.Audit;

namespace t5f25sdprojectone_projectsplus.Repositories.Interfaces
{
    /// <summary>
    /// Read-only repository surface for querying AuditEvent rows for admin UIs and investigative tools.
    /// - Designed for pagination, light-weight projections, and efficient filtering by common attributes.
    /// - Implementations should favor projections (SELECT specific columns) and avoid loading large JSON blobs
    ///   unless explicitly requested by callers (e.g., GetByIdAsync with includePayload = true).
    /// - Implementations must be safe for use in read replicas and should accept CancellationToken.
    /// </summary>
    public interface IAuditQueryRepository
    {
        /// <summary>
        /// Query audit events with flexible filters and pagination.
        /// - Filters are optional; if all are null, returns most recent events.
        /// - Prefer projection to AuditEventSummary for list views; includePayload toggles loading PayloadJson/MetadataJson.
        /// </summary>
        Task<PagedResult<AuditEventSummary>> QueryAsync(
            AuditQueryFilter? filter,
            PagingOptions paging,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a single audit event by id. If includePayload is false, implementer may omit PayloadJson/MetadataJson to reduce IO.
        /// Returns null when no event found.
        /// </summary>
        Task<AuditEvent?> GetByIdAsync(long id, bool includePayload = false, CancellationToken cancellationToken = default);

        /// <summary>
        /// Query all events associated with a correlation id (common for tracing a request flow).
        /// Results are ordered by OccurredAt ascending by default unless paging.SortDescending is true.
        /// </summary>
        Task<IReadOnlyList<AuditEventSummary>> QueryByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Query recent events for a given actor (user) optionally filtered by eventType and time range.
        /// </summary>
        Task<PagedResult<AuditEventSummary>> QueryByActorAsync(
            long actorId,
            AuditQueryFilter? filter,
            PagingOptions paging,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Count events matching a filter. Useful for admin dashboards and pagination total counts.
        /// </summary>
        Task<long> CountAsync(AuditQueryFilter? filter, CancellationToken cancellationToken = default);
    }

    #region Supporting types (lightweight, copy-friendly)

    /// <summary>
    /// Simple paged result container. Keeps a minimal surface so implementers can return consistent shape.
    /// </summary>
    public sealed class PagedResult<T>
    {
        public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();
        public long TotalCount { get; init; }
        public int Page { get; init; }
        public int PageSize { get; init; }

        public static PagedResult<T> Empty(int page = 1, int pageSize = 25) =>
            new PagedResult<T> { Items = Array.Empty<T>(), TotalCount = 0, Page = page, PageSize = pageSize };
    }

    /// <summary>
    /// Paging options used by Query methods. Keep simple to avoid over-optimizing prematurely.
    /// </summary>
    public sealed class PagingOptions
    {
        public int Page { get; init; } = 1;
        public int PageSize { get; init; } = 25;
        public bool SortDescending { get; init; } = true; // default newest-first
    }

    /// <summary>
    /// Filter object supporting the most common query dimensions for audits.
    /// Add properties only when needed to avoid explosion of permutations.
    /// </summary>
    // Repositories/Audit/IAuditQueryRepository.cs (supporting types section)
    public sealed record AuditQueryFilter
    {
        public Guid? CorrelationId { get; init; }
        public long? ActorId { get; init; }
        public string? EventType { get; init; }
        public string? TargetType { get; init; }
        public long? TargetId { get; init; }
        public long? SystemTypeId { get; init; }

        // Time range filters: inclusive semantics
        public DateTimeOffset? OccurredAfter { get; init; }
        public DateTimeOffset? OccurredBefore { get; init; }

        // Optional free-text match against MetadataJson or PayloadJson (implementer decides how to apply safely)
        public string? TextSearch { get; init; }
    }


    /// <summary>
    /// Lightweight projection for list views. Implementations should populate this from efficient SELECTs.
    /// PayloadJson/MetadataJson omitted to reduce IO unless explicitly requested.
    /// </summary>
    public sealed class AuditEventSummary
    {
        public long Id { get; init; }
        public string EventType { get; init; } = null!;
        public Guid CorrelationId { get; init; }
        public long? ActorId { get; init; }
        public string? TargetType { get; init; }
        public long? TargetId { get; init; }
        public long? SystemTypeId { get; init; }
        public DateTimeOffset OccurredAt { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        public int Version { get; init; }
    }

    #endregion

}
