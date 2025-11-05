using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using t5f25sdprojectone_projectsplus.Models.Comunication;

namespace t5f25sdprojectone_projectsplus.Models
{
    public class Project
    {
        public long Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string? Description { get; set; }
        public byte SkillLevel { get; set; }      // 1=Beginner,2=Intermediate,3=Advanced
        public byte Status { get; set; }          // 0=Pending,1=Approved,2=Open,3=InProgress,4=Completed
        public string? GithubRepoUrl { get; set; }
        public long? FacultyId { get; set; }
        public User? Faculty { get; set; }
        public byte Visibility { get; set; }      // 0=Private,1=Public,2=Org
        public string? MetadataJson { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation
        // Keep your notes in comments where helpful.
        public ICollection<RoleAssignment> RoleAssignments { get; set; } = new List<RoleAssignment>();
        public ICollection<ProjectTask> Tasks { get; set; } = new List<ProjectTask>();
        public ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();
        public ICollection<Attestation> Attestations { get; set; } = new List<Attestation>();
        public ICollection<Workspace> Workspaces { get; set; } = new List<Workspace>();

        // Chat-related navigations
        // Projects can have one or more chat rooms (project channel + optional feature rooms)
        public ICollection<ChatRoom> ChatRooms { get; set; } = new List<ChatRoom>();
        // Project-scoped messages (denormalized messages that reference the project directly)
        public ICollection<ChatMessage> ChatMessages { get; set; } = new List<ChatMessage>();
    }

    public class ProjectEntityTypeConfiguration : IEntityTypeConfiguration<Project>
    {
        public void Configure(EntityTypeBuilder<Project> builder)
        {
            builder.ToTable("Projects");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();

            builder.Property(p => p.Title)
                   .HasColumnType("nvarchar(500)")
                   .IsRequired();

            builder.Property(p => p.Slug)
                   .HasColumnType("nvarchar(255)")
                   .IsRequired();

            builder.HasIndex(p => p.Slug)
                   .IsUnique()
                   .HasDatabaseName("IX_Projects_Slug");

            builder.Property(p => p.Description)
                   .HasColumnType("nvarchar(max)");

            builder.Property(p => p.SkillLevel)
                   .HasColumnType("tinyint")
                   .IsRequired();

            builder.Property(p => p.Status)
                   .HasColumnType("tinyint")
                   .IsRequired();

            builder.Property(p => p.GithubRepoUrl)
                   .HasColumnType("nvarchar(1000)");

            builder.Property(p => p.FacultyId)
                   .HasColumnType("bigint");

            builder.HasOne(p => p.Faculty)
                   .WithMany()
                   .HasForeignKey(p => p.FacultyId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.Property(p => p.Visibility)
                   .HasColumnType("tinyint")
                   .IsRequired();

            builder.Property(p => p.MetadataJson)
                   .HasColumnType("nvarchar(max)");

            builder.Property(p => p.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(p => p.UpdatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            // Indexes for common queries
            builder.HasIndex(p => new { p.Status, p.Visibility })
                   .HasDatabaseName("IX_Projects_Status_Visibility");

            // No explicit EF configuration required here for ChatRooms/ChatMessages
            // as those are configured on ChatRoomEntityTypeConfiguration and ChatMessageEntityTypeConfiguration.
        }
    }
}
