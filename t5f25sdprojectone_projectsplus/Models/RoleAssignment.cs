using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models
{
    public class RoleAssignment
    {
        public long Id { get; set; }

        // Foreign keys
        public long ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        public long UserId { get; set; }
        public User User { get; set; } = null!;

        // Role details
        public string RoleName { get; set; } = null!;
        public byte Status { get; set; } // 0=Applied,1=Approved,2=Active,3=Completed

        public DateTimeOffset? AssignedAt { get; set; }
        public DateTimeOffset? CompletedAt { get; set; }
    }

    public class RoleAssignmentEntityTypeConfiguration : IEntityTypeConfiguration<RoleAssignment>
    {
        public void Configure(EntityTypeBuilder<RoleAssignment> builder)
        {
            builder.ToTable("RoleAssignments");

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedOnAdd();

            builder.Property(r => r.ProjectId).HasColumnType("bigint").IsRequired();
            builder.HasOne(r => r.Project)
                   .WithMany(p => p.RoleAssignments)
                   .HasForeignKey(r => r.ProjectId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(r => r.UserId).HasColumnType("bigint").IsRequired();
            builder.HasOne(r => r.User)
                   .WithMany(u => u.RoleAssignments)
                   .HasForeignKey(r => r.UserId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(r => r.RoleName)
                   .HasColumnType("nvarchar(100)")
                   .IsRequired();

            builder.Property(r => r.Status)
                   .HasColumnType("tinyint")
                   .IsRequired();

            builder.Property(r => r.AssignedAt).HasColumnType("datetime2");
            builder.Property(r => r.CompletedAt).HasColumnType("datetime2");

            builder.HasIndex(r => r.UserId).HasDatabaseName("IX_RoleAssignments_UserId");

            builder.HasAlternateKey("ProjectId", "UserId", "RoleName"); // prevents duplicate same-role assignments
        }
    }
}

