// Middleware/Audit/AuditMiddleware.cs
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace t5f25sdprojectone_projectsplus.Models.Audit
{
    /// <summary>
    /// Ensures a CorrelationId is present on each request, enriches logging scope, and exposes AuditScope
    /// values via IHttpContextAccessor so downstream services can read actor and system_type context.
    /// Adds X-Correlation-Id response header for client observability.
    /// </summary>
    public class AuditMiddleware
    {
        private const string CorrelationHeader = "X-Correlation-Id";
        private readonly RequestDelegate _next;
        private readonly ILogger<AuditMiddleware> _logger;

        public AuditMiddleware(RequestDelegate next, ILogger<AuditMiddleware> logger)
        {
            _next = next ?? throw new ArgumentNullException(nameof(next));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context is null) throw new ArgumentNullException(nameof(context));

            // Ensure correlation id: prefer incoming header, otherwise create a new GUID
            var correlationId = GetOrCreateCorrelationId(context);

            // Expose correlation id and default empty actor/system type in AuditScope
            var auditScope = AuditScope.CreateDefault(correlationId);
            context.Items[AuditScope.HttpContextItemKey] = auditScope;

            // Enrich logging scope with correlation id and optionally actor/system-type if populated later
            using (_logger.BeginScope(new
            {
                CorrelationId = correlationId,
                ActorId = (long?)null,
                SystemTypeId = (long?)null,
                RequestPath = context.Request.Path,
                context.Request.Method
            }))
            {
                // Add header to response for client tracing
                context.Response.OnStarting(() =>
                {
                    if (!context.Response.Headers.ContainsKey(CorrelationHeader))
                    {
                        context.Response.Headers[CorrelationHeader] = correlationId.ToString();
                    }
                    return Task.CompletedTask;
                });

                try
                {
                    await _next(context).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    // Log the exception with correlation id in scope, then rethrow to preserve middleware pipeline semantics
                    _logger.LogError(ex, "Unhandled exception for request {Method} {Path} Correlation={CorrelationId}", context.Request.Method, context.Request.Path, correlationId);
                    throw;
                }
            }
        }

        private static Guid GetOrCreateCorrelationId(HttpContext context)
        {
            if (context.Request.Headers.TryGetValue(CorrelationHeader, out var values) &&
                Guid.TryParse(values.ToString(), out var parsed) &&
                parsed != Guid.Empty)
            {
                return parsed;
            }

            // No valid incoming header — create a new id
            return Guid.NewGuid();
        }
    }
}
