// Tests/Audit/AuditQueryRepositoryTests.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using t5f25sdprojectone_projectsplus.Data;
using t5f25sdprojectone_projectsplus.Models.Audit;
using t5f25sdprojectone_projectsplus.Repositories;
using t5f25sdprojectone_projectsplus.Repositories.Interfaces;
using Xunit;
using Assert = Xunit.Assert;

namespace t5f25sdprojectone_projectsplus.Tests.AuditTests
{
    /// <summary>
    /// Unit tests for AuditQueryRepository using EF InMemory provider.
    /// Focus: correctness of filtering, paging and projections (AuditEventSummary).
    /// These are fast, deterministic tests that do not rely on provider-specific SQL behavior.
    /// </summary>
    public class AuditQueryRepositoryTests : IDisposable
    {
        private readonly ProjectsPlusDbContext _db;
        private readonly AuditQueryRepository _repo;

        public AuditQueryRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<ProjectsPlusDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            _db = new ProjectsPlusDbContext(options, loggerFactory: null);

            // Seed deterministic data for tests
            Seed().GetAwaiter().GetResult();

            _repo = new AuditQueryRepository(_db);
        }

        public void Dispose()
        {
            _db.Dispose();
        }

        private async Task Seed()
        {
            var now = DateTimeOffset.UtcNow;
            var events = new List<AuditEvent>
            {
                new AuditEvent { EventType = "Authn", CorrelationId = Guid.Parse("11111111-1111-1111-1111-111111111111"), ActorId = 1, TargetType = "User", TargetId = 1, SystemTypeId = 10, OccurredAt = now.AddMinutes(-30), PayloadJson = "{}", MetadataJson = "{}" },
                new AuditEvent { EventType = "Authz", CorrelationId = Guid.Parse("22222222-2222-2222-2222-222222222222"), ActorId = 2, TargetType = "Project", TargetId = 5, SystemTypeId = 11, OccurredAt = now.AddMinutes(-20), PayloadJson = "{}", MetadataJson = "{}" },
                new AuditEvent { EventType = "Authn", CorrelationId = Guid.Parse("33333333-3333-3333-3333-333333333333"), ActorId = 1, TargetType = "Project", TargetId = 6, SystemTypeId = 10, OccurredAt = now.AddMinutes(-10), PayloadJson = "{}", MetadataJson = "{}" },
                // extra events to verify paging/order
                new AuditEvent { EventType = "Authn", CorrelationId = Guid.NewGuid(), ActorId = 3, TargetType = "File", TargetId = 9, SystemTypeId = 12, OccurredAt = now.AddMinutes(-5), PayloadJson = "{}", MetadataJson = "{}" },
            };

            _db.AddRange(events);
            await _db.SaveChangesAsync();
        }

        [Fact]
        public async Task QueryAsync_NoFilter_ReturnsPagedRecentEvents()
        {
            var result = await _repo.QueryAsync(null, new PagingOptions { Page = 1, PageSize = 2 }, CancellationToken.None);

            Assert.Equal(2, result.Items.Count);
            Assert.True(result.TotalCount >= 4);
            Assert.Equal(1, result.Page);
            Assert.Equal(2, result.PageSize);
        }

        [Fact]
        public async Task QueryByCorrelationIdAsync_ReturnsAllEventsForCorrelation()
        {
            var correlation = Guid.Parse("11111111-1111-1111-1111-111111111111");

            var list = await _repo.QueryByCorrelationIdAsync(correlation, CancellationToken.None);

            Assert.Single(list);
            Assert.Equal("Authn", list.First().EventType);
            Assert.Equal(correlation, list.First().CorrelationId);
        }

        [Fact]
        public async Task QueryByActorAsync_FiltersByActorAndRespectsPaging()
        {
            var paging = new PagingOptions { Page = 1, PageSize = 10, SortDescending = false };

            var result = await _repo.QueryByActorAsync(1, null, paging, CancellationToken.None);

            Assert.All(result.Items, s => Assert.Equal(1, s.ActorId));
            Assert.True(result.TotalCount >= 2);
        }

        [Fact]
        public async Task GetByIdAsync_WithoutPayload_OmitsPayloadFields()
        {
            // pick an existing id
            var id = await _db.AuditEvents.Select(a => a.Id).FirstAsync();
            var evt = await _repo.GetByIdAsync(id, includePayload: false, cancellationToken: CancellationToken.None);

            Assert.NotNull(evt);
            Assert.Equal(id, evt!.Id);
            Assert.Null(evt.PayloadJson);
            Assert.Null(evt.MetadataJson);
        }

        [Fact]
        public async Task GetByIdAsync_WithPayload_ReturnsPayload()
        {
            var id = await _db.AuditEvents.Select(a => a.Id).FirstAsync();
            var evt = await _repo.GetByIdAsync(id, includePayload: true, cancellationToken: CancellationToken.None);

            Assert.NotNull(evt);
            Assert.Equal(id, evt!.Id);
            Assert.NotNull(evt.PayloadJson);
        }
    }
}
