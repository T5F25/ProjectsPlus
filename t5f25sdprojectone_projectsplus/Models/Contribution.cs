using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models
{
    public class Contribution
    {
        public long Id { get; set; }

        // Relation to Project
        public long ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        // Author (may be null if external)
        public long? UserId { get; set; }
        public User? User { get; set; }

        // Contribution details
        public byte Type { get; set; } // 0=Commit,1=PullRequest,2=Issue,3=Review
        public string GithubExternalId { get; set; } = null!; // unique external id (PR number, commit sha, issue id)
        public string? MetadataS3Key { get; set; } // raw webhook payload or large metadata stored in S3

        public DateTimeOffset CreatedAt { get; set; }
    }

    public class ContributionEntityTypeConfiguration : IEntityTypeConfiguration<Contribution>
    {
        public void Configure(EntityTypeBuilder<Contribution> builder)
        {
            builder.ToTable("Contributions");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).ValueGeneratedOnAdd();

            builder.Property(c => c.ProjectId)
                   .HasColumnType("bigint")
                   .IsRequired();

            builder.HasOne(c => c.Project)
                   .WithMany(p => p.Contributions)
                   .HasForeignKey(c => c.ProjectId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(c => c.UserId)
                   .HasColumnType("bigint");

            builder.HasOne(c => c.User)
                   .WithMany(u => u.Contributions)
                   .HasForeignKey(c => c.UserId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.Property(c => c.Type)
                   .HasColumnType("tinyint")
                   .IsRequired();

            builder.Property(c => c.GithubExternalId)
                   .HasColumnType("nvarchar(100)")
                   .IsRequired();

            builder.HasIndex(c => c.GithubExternalId)
                   .IsUnique()
                   .HasDatabaseName("UQ_Contributions_GithubExternalId");

            builder.Property(c => c.MetadataS3Key)
                   .HasColumnType("nvarchar(1000)");

            builder.Property(c => c.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.HasIndex(c => new { c.ProjectId, c.UserId })
                   .HasDatabaseName("IX_Contributions_Project_User");
        }
    }
}
