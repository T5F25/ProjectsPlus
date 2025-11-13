// Program.cs
using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
// CorrelationEnricher (adjust if you moved this type)
using Serilog;
using Serilog.Events;
using t5f25sdprojectone_projectsplus.Models.Audit;

namespace t5f25sdprojectone_projectsplus
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Register framework services
            builder.Services.AddRazorPages();
            builder.Services.AddHttpContextAccessor();

            // Configure Serilog BEFORE building the app; basic bootstrap logger to capture startup issues
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .WriteTo.File("logs/projectsplus-.log", rollingInterval: Serilog.RollingInterval.Day)
                .CreateLogger();

            // Use Serilog with access to DI so we can resolve IHttpContextAccessor for the enricher
            builder.Host.UseSerilog((hostContext, services, loggerConfiguration) =>
            {
                var httpContextAccessor = services.GetRequiredService<IHttpContextAccessor>();

                loggerConfiguration
                    .MinimumLevel.Is(LogEventLevel.Information)
                    .Enrich.FromLogContext()
                    .Enrich.With(new CorrelationEnricher(httpContextAccessor))
                    .WriteTo.Console()
                    .WriteTo.File("logs/projectsplus-.log", rollingInterval: Serilog.RollingInterval.Day);
            });

            var app = builder.Build();

            // Ensure AuditMiddleware runs early so AuditScope is available for logging enrichment
            app.UseMiddleware<AuditMiddleware>();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthorization();
            app.MapRazorPages();

            app.Run();
        }
    }
}
