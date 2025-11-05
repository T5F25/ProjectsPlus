using System.Reflection;
using Microsoft.EntityFrameworkCore;
using t5f25sdprojectone_projectsplus.Models;
using t5f25sdprojectone_projectsplus.Models.Comunication;

namespace t5f25sdprojectone_projectsplus.Data
{
    public class ProjectsPlusDbContext : DbContext
    {
        public ProjectsPlusDbContext(DbContextOptions<ProjectsPlusDbContext> options) : base(options)
        {
        }

        // Core entity sets
        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Project> Projects { get; set; } = null!;
        public DbSet<ProjectTask> ProjectTasks { get; set; } = null!;
        public DbSet<RoleAssignment> RoleAssignments { get; set; } = null!;
        public DbSet<Workspace> Workspaces { get; set; } = null!;
        public DbSet<Contribution> Contributions { get; set; } = null!;
        public DbSet<Attestation> Attestations { get; set; } = null!;
        public DbSet<ChatMessage> ChatMessages { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<Role> Roles { get; set; } = null!;
        public DbSet<Permission> Permissions { get; set; } = null!;
        public DbSet<RolePermission> RolePermissions { get; set; } = null!;
        public DbSet<UserRole> UserRoles { get; set; } = null!;
        public DbSet<Grant> Grants { get; set; } = null!;
        public DbSet<FileReference> FileReferences { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Pick up all IEntityTypeConfiguration implementations in this assembly
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

            // Ensure Project -> ProjectTask navigation is correct
            // If Project currently exposes Tasks as ICollection<ProjectTask>, nothing further is required.
            // If older classes still reference Task, map it explicitly here to avoid ambiguity:
            // modelBuilder.Entity<Project>().HasMany<ProjectTask>("Tasks").WithOne(p => p.Project).HasForeignKey("ProjectId");

            base.OnModelCreating(modelBuilder);
        }
    }
}
