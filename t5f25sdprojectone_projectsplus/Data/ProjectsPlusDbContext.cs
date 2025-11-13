// Data/ProjectsPlusDbContext.cs
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using t5f25sdprojectone_projectsplus.Models.Audit;
using t5f25sdprojectone_projectsplus.Models.SystemTypeID;

namespace t5f25sdprojectone_projectsplus.Data
{
    /// <summary>
    /// Central EF Core DbContext for ProjectsPlus.
    /// - Keep DbSet additions strictly synchronized with corresponding entity and configuration files.
    /// - Commented DbSets are placeholders until the model classes and configurations are implemented.
    /// - ApplyConfigurationsFromAssembly is the single source of mapping truth for the project.
    /// </summary>
    public class ProjectsPlusDbContext : DbContext
    {
        private readonly ILoggerFactory? _loggerFactory;

        public ProjectsPlusDbContext(DbContextOptions<ProjectsPlusDbContext> options, ILoggerFactory? loggerFactory = null)
            : base(options)
        {
            _loggerFactory = loggerFactory;
        }

        // Core audit + reference data
        public DbSet<AuditEvent> AuditEvents { get; set; } = null!;
        public DbSet<SystemType> SystemTypes { get; set; } = null!; // seeds provided in SystemType configuration

        // Auth / identity related
        // TODO: Uncomment when the RefreshToken model and its configuration exist
        // public DbSet<RefreshToken> RefreshTokens { get; set; } = null!; // persistent refresh tokens

        // File / attachment domain
        // TODO: Uncomment when FileRecord and ProjectAttachment models/configs exist
        // public DbSet<FileRecord> FileRecords { get; set; } = null!;   // file metadata, versioned and auditable
        // public DbSet<ProjectAttachment> ProjectAttachments { get; set; } = null!; // join/attachment entity

        // Domain tables to add as features land
        // public DbSet<Project> Projects { get; set; } = null!;
        // public DbSet<Episode> Episodes { get; set; } = null!;
        // public DbSet<Role> Roles { get; set; } = null!;

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Do not overwrite options when the caller has configured them (e.g., in Program.cs).
            if (!optionsBuilder.IsConfigured)
            {
                // sensible local dev default for SQL Server Express; callers should override in DI registration.
                optionsBuilder.UseSqlServer("Server=.\\SQLEXPRESS;Database=ProjectsPlus;Trusted_Connection=True;");
            }

            if (_loggerFactory != null)
            {
                optionsBuilder.UseLoggerFactory(_loggerFactory);
                // keep sensitive data logging off by default; enable only for short-lived local debugging
                optionsBuilder.EnableSensitiveDataLogging(false);
            }

            base.OnConfiguring(optionsBuilder);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Apply modular IEntityTypeConfiguration implementations from this assembly.
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(ProjectsPlusDbContext).Assembly);

            // Important: keep seed data inside each IEntityTypeConfiguration to avoid duplication.
            // SystemType seeds are already provided in its configuration file.

            // Conventions / safety notes:
            // - Numeric Id PK/FK conventions should be enforced in entity configurations.
            // - Use DeleteBehavior.Restrict for cross-domain references unless domain logic requires cascade.
            // - Use explicit HasConstraintName for all FKs to stabilize migration DDL and make teardown predictable.
            // - Register this DbContext as Scoped in DI when you add registrations (we'll do that later).

            base.OnModelCreating(modelBuilder);
        }
    }
}
