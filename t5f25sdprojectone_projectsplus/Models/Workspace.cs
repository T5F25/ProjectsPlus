using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using t5f25sdprojectone_projectsplus.Models.Comunication;

namespace t5f25sdprojectone_projectsplus.Models
{
    public class Workspace
    {
        public long Id { get; set; }

        // Link back to the vetted Project that spawned this workspace
        public long ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        // Owner (faculty or team lead) responsible for workspace governance
        public long? OwnerId { get; set; }
        public User? Owner { get; set; }

        // Provisioning status for infra/resources
        public byte Status { get; set; } // 0=PendingProvision,1=Provisioning,2=Ready,3=Degraded,4=Decommissioned,5=Failed

        // Optional infra/resource pointers
        public string? DeploymentEndpoint { get; set; }   // e.g., https://workspace-xxx.example.com
        public string? StorageBucket { get; set; }        // S3 bucket name or logical key
        public string? DatabaseInstance { get; set; }     // RDS identifier or connection alias
        public string? NotesJson { get; set; }            // small metadata / settings

        // Lifecycle timestamps
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ProvisionedAt { get; set; }
        public DateTimeOffset? DecommissionedAt { get; set; }

        // Optional relations
        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();

        // Chat surfaces for this workspace
        // Keep your notes in comments; workspace-level chatrooms and messages are supported
        public ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();
        public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }

    public class WorkspaceEntityTypeConfiguration : IEntityTypeConfiguration<Workspace>
    {
        public void Configure(EntityTypeBuilder<Workspace> builder)
        {
            builder.ToTable("Workspaces");

            builder.HasKey(w => w.Id);
            builder.Property(w => w.Id).ValueGeneratedOnAdd();

            builder.Property(w => w.ProjectId)
                   .HasColumnType("bigint")
                   .IsRequired();

            // Keep relationship minimal and correctly typed:
            // Project -> Workspaces (Project may own multiple workspaces)
            builder.HasOne(w => w.Project)
                   .WithMany(p => p.Workspaces)
                   .HasForeignKey(w => w.ProjectId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(w => w.OwnerId)
                   .HasColumnType("bigint");

            builder.HasOne(w => w.Owner)
                   .WithMany()
                   .HasForeignKey(w => w.OwnerId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.Property(w => w.Status)
                   .HasColumnType("tinyint")
                   .IsRequired();

            builder.Property(w => w.DeploymentEndpoint)
                   .HasColumnType("nvarchar(1000)");

            builder.Property(w => w.StorageBucket)
                   .HasColumnType("nvarchar(500)");

            builder.Property(w => w.DatabaseInstance)
                   .HasColumnType("nvarchar(500)");

            builder.Property(w => w.NotesJson)
                   .HasColumnType("nvarchar(max)");

            builder.Property(w => w.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(w => w.ProvisionedAt)
                   .HasColumnType("datetime2");

            builder.Property(w => w.DecommissionedAt)
                   .HasColumnType("datetime2");

            builder.HasIndex(w => w.ProjectId)
                   .HasDatabaseName("IX_Workspaces_ProjectId");

            builder.HasIndex(w => w.OwnerId)
                   .HasDatabaseName("IX_Workspaces_OwnerId");

            // Consider a uniqueness constraint so a Project maps to at most one active Workspace
            builder.HasIndex(w => new { w.ProjectId, w.Status })
                   .HasDatabaseName("IX_Workspaces_Project_Status");
        }
    }
}
