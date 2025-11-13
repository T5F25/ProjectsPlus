// Repositories/Audit/AuditQueryRepository.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using t5f25sdprojectone_projectsplus.Data;
using t5f25sdprojectone_projectsplus.Models.Audit;
using t5f25sdprojectone_projectsplus.Repositories.Interfaces;

namespace t5f25sdprojectone_projectsplus.Repositories
{
    /// <summary>
    /// EF Core implementation of IAuditQueryRepository.
    /// - Uses projections for list queries to avoid loading large JSON blobs.
    /// - Designed for read-replica friendly usage (no tracked queries by default).
    /// - Filtering logic is pushed to the database to avoid client-side evaluation.
    /// </summary>
    public sealed class AuditQueryRepository : IAuditQueryRepository
    {
        private readonly ProjectsPlusDbContext _db;

        public AuditQueryRepository(ProjectsPlusDbContext db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public async Task<PagedResult<AuditEventSummary>> QueryAsync(
            AuditQueryFilter? filter,
            PagingOptions paging,
            CancellationToken cancellationToken = default)
        {
            paging ??= new PagingOptions();
            filter ??= new AuditQueryFilter();

            var q = BuildFilter(_db.AuditEvents.AsNoTracking(), filter);

            var total = await q.LongCountAsync(cancellationToken).ConfigureAwait(false);

            var ordered = paging.SortDescending
                ? q.OrderByDescending(a => a.OccurredAt)
                : q.OrderBy(a => a.OccurredAt);

            var items = await ordered
                .Skip((paging.Page - 1) * paging.PageSize)
                .Take(paging.PageSize)
                .Select(a => new AuditEventSummary
                {
                    Id = a.Id,
                    EventType = a.EventType,
                    CorrelationId = a.CorrelationId,
                    ActorId = a.ActorId,
                    TargetType = a.TargetType,
                    TargetId = a.TargetId,
                    SystemTypeId = a.SystemTypeId,
                    OccurredAt = a.OccurredAt,
                    CreatedAt = a.CreatedAt,
                    Version = a.Version
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return new PagedResult<AuditEventSummary>
            {
                Items = items,
                TotalCount = total,
                Page = paging.Page,
                PageSize = paging.PageSize
            };
        }

        public async Task<AuditEvent?> GetByIdAsync(long id, bool includePayload = false, CancellationToken cancellationToken = default)
        {
            if (includePayload)
            {
                return await _db.AuditEvents
                    .AsNoTracking()
                    .FirstOrDefaultAsync(a => a.Id == id, cancellationToken)
                    .ConfigureAwait(false);
            }

            return await _db.AuditEvents
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AuditEvent
                {
                    Id = a.Id,
                    EventType = a.EventType,
                    CorrelationId = a.CorrelationId,
                    ActorId = a.ActorId,
                    TargetId = a.TargetId,
                    TargetType = a.TargetType,
                    SystemTypeId = a.SystemTypeId,
                    OccurredAt = a.OccurredAt,
                    Version = a.Version,
                    CreatedAt = a.CreatedAt,
                    // omit PayloadJson and MetadataJson to reduce IO when includePayload == false
                })
                .FirstOrDefaultAsync(cancellationToken)
                .ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<AuditEventSummary>> QueryByCorrelationIdAsync(Guid correlationId, CancellationToken cancellationToken = default)
        {
            var list = await _db.AuditEvents
                .AsNoTracking()
                .Where(a => a.CorrelationId == correlationId)
                .OrderBy(a => a.OccurredAt)
                .Select(a => new AuditEventSummary
                {
                    Id = a.Id,
                    EventType = a.EventType,
                    CorrelationId = a.CorrelationId,
                    ActorId = a.ActorId,
                    TargetType = a.TargetType,
                    TargetId = a.TargetId,
                    SystemTypeId = a.SystemTypeId,
                    OccurredAt = a.OccurredAt,
                    CreatedAt = a.CreatedAt,
                    Version = a.Version
                })
                .ToListAsync(cancellationToken)
                .ConfigureAwait(false);

            return list;
        }

        public async Task<PagedResult<AuditEventSummary>> QueryByActorAsync(
            long actorId,
            AuditQueryFilter? filter,
            PagingOptions paging,
            CancellationToken cancellationToken = default)
        {
            filter ??= new AuditQueryFilter();
            filter = filter with { ActorId = actorId };

            return await QueryAsync(filter, paging ?? new PagingOptions(), cancellationToken).ConfigureAwait(false);
        }

        public async Task<long> CountAsync(AuditQueryFilter? filter, CancellationToken cancellationToken = default)
        {
            filter ??= new AuditQueryFilter();
            var q = BuildFilter(_db.AuditEvents.AsNoTracking(), filter);
            return await q.LongCountAsync(cancellationToken).ConfigureAwait(false);
        }

        #region Private helpers

        private static IQueryable<AuditEvent> BuildFilter(IQueryable<AuditEvent> q, AuditQueryFilter filter)
        {
            if (filter.CorrelationId.HasValue)
            {
                q = q.Where(a => a.CorrelationId == filter.CorrelationId.Value);
            }

            if (filter.ActorId.HasValue)
            {
                q = q.Where(a => a.ActorId == filter.ActorId.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.EventType))
            {
                q = q.Where(a => a.EventType == filter.EventType);
            }

            if (!string.IsNullOrWhiteSpace(filter.TargetType))
            {
                q = q.Where(a => a.TargetType == filter.TargetType);
            }

            if (filter.TargetId.HasValue)
            {
                q = q.Where(a => a.TargetId == filter.TargetId.Value);
            }

            if (filter.SystemTypeId.HasValue)
            {
                q = q.Where(a => a.SystemTypeId == filter.SystemTypeId.Value);
            }

            if (filter.OccurredAfter.HasValue)
            {
                q = q.Where(a => a.OccurredAt >= filter.OccurredAfter.Value);
            }

            if (filter.OccurredBefore.HasValue)
            {
                q = q.Where(a => a.OccurredAt <= filter.OccurredBefore.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.TextSearch))
            {
                // Text search is provider-specific and potentially expensive; apply a conservative, index-friendly approach:
                // - Match against event_type and target_type first
                // - Leave JSON full text search to specialized endpoints or DB functions (not implemented here)
                var ts = filter.TextSearch.Trim();
                q = q.Where(a => a.EventType.Contains(ts) || a.TargetType != null && a.TargetType.Contains(ts));
            }

            return q;
        }

        #endregion
    }
}
