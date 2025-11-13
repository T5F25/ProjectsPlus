// Logging/Serilog/CorrelationEnricher.cs
using System;
using Microsoft.AspNetCore.Http;
using Serilog.Core;
using Serilog.Events;

namespace t5f25sdprojectone_projectsplus.Models.Audit
{
    /// <summary>
    /// Enriches Serilog log events with CorrelationId, ActorId and SystemTypeId from IHttpContextAccessor (when available).
    /// Register with DI and include in LoggerConfiguration.Enrich.With(new CorrelationEnricher(...))
    /// </summary>
    public class CorrelationEnricher : ILogEventEnricher
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _correlationPropertyName;
        private readonly string _actorPropertyName;
        private readonly string _systemTypePropertyName;

        public CorrelationEnricher(IHttpContextAccessor httpContextAccessor,
                                  string correlationPropertyName = "CorrelationId",
                                  string actorPropertyName = "ActorId",
                                  string systemTypePropertyName = "SystemTypeId")
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _correlationPropertyName = correlationPropertyName;
            _actorPropertyName = actorPropertyName;
            _systemTypePropertyName = systemTypePropertyName;
        }

        public void Enrich(LogEvent logEvent, ILogEventPropertyFactory propertyFactory)
        {
            try
            {
                var ctx = _httpContextAccessor.HttpContext;
                if (ctx == null) return;

                if (ctx.Items.TryGetValue(AuditScope.HttpContextItemKey, out var scopeObj) && scopeObj is AuditScope scope)
                {
                    if (scope.CorrelationId != Guid.Empty)
                    {
                        var prop = propertyFactory.CreateProperty(_correlationPropertyName, scope.CorrelationId);
                        logEvent.AddPropertyIfAbsent(prop);
                    }

                    if (scope.ActorId.HasValue)
                    {
                        var prop = propertyFactory.CreateProperty(_actorPropertyName, scope.ActorId.Value);
                        logEvent.AddPropertyIfAbsent(prop);
                    }

                    if (scope.SystemTypeId.HasValue)
                    {
                        var prop = propertyFactory.CreateProperty(_systemTypePropertyName, scope.SystemTypeId.Value);
                        logEvent.AddPropertyIfAbsent(prop);
                    }
                }
            }
            catch
            {
                // Enrichment must not throw; swallow any unexpected errors.
            }
        }
    }
}
