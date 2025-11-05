using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models
{
    public class User
    {
        public long Id { get; set; }
        public string Email { get; set; } = null!;
        public string DisplayName { get; set; } = null!;
        public bool Verified { get; set; }
        public long? GithubId { get; set; }
        public string? ProfileJson { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Navigation properties
        public ICollection<RoleAssignment> RoleAssignments { get; set; } = new List<RoleAssignment>();
        public ICollection<Contribution> Contributions { get; set; } = new List<Contribution>();
        public ICollection<Attestation> AttestationsIssued { get; set; } = new List<Attestation>();
    }

    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");

            builder.HasKey(u => u.Id);

            builder.Property(u => u.Id)
                   .ValueGeneratedOnAdd();

            builder.Property(u => u.Email)
                   .HasColumnType("nvarchar(256)")
                   .IsRequired();

            builder.HasIndex(u => u.Email)
                   .IsUnique()
                   .HasDatabaseName("IX_Users_Email");

            builder.Property(u => u.DisplayName)
                   .HasColumnType("nvarchar(200)")
                   .IsRequired();

            builder.Property(u => u.Verified)
                   .HasDefaultValue(false)
                   .IsRequired();

            builder.Property(u => u.GithubId)
                   .HasColumnType("bigint");

            builder.HasIndex(u => u.GithubId)
                   .IsUnique()
                   .HasFilter("[GithubId] IS NOT NULL")
                   .HasDatabaseName("IX_Users_GithubId");

            builder.Property(u => u.ProfileJson)
                   .HasColumnType("nvarchar(max)");

            builder.Property(u => u.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(u => u.UpdatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();
            // Navigation constraints (optional cascade behaviour left default)
        }
    }
}
