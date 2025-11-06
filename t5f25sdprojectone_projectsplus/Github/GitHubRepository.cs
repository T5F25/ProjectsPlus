using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using t5f25sdprojectone_projectsplus.Github;
using t5f25sdprojectone_projectsplus.Models;

namespace t5f25sdprojectone_projectsplus.Github
{
    // Cached GitHub repository metadata. Records are canonical in our DB for history/audit.
    public class GitHubRepository
    {
        public long Id { get; set; }                      // PK (local)
        public long ProjectId { get; set; }               // optional link to local Project
        public Project? Project { get; set; }

        public long ExternalRepoId { get; set; }          // GitHub numeric repo id
        public string Owner { get; set; } = null!;        // "org-or-user"
        public string Name { get; set; } = null!;         // repo name
        public string FullName { get; set; } = null!;     // owner/name
        public string Url { get; set; } = null!;          // html_url
        public string? Description { get; set; }
        public bool Private { get; set; }
        public string? DefaultBranch { get; set; }
        public string? MetadataJson { get; set; }         // raw GitHub JSON snapshot if needed

        // Stats from GitHub (snapshot)
        public int StargazersCount { get; set; }
        public int ForksCount { get; set; }
        public int OpenIssuesCount { get; set; }

        // Audit
        public DateTimeOffset FetchedAt { get; set; }     // when we last refreshed from GitHub
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset UpdatedAt { get; set; }

        // Related cached items
        public ICollection<GitHubCommit> Commits { get; set; } = new List<GitHubCommit>();
        public ICollection<GitHubIssue> Issues { get; set; } = new List<GitHubIssue>();
        public ICollection<GitHubPullRequest> PullRequests { get; set; } = new List<GitHubPullRequest>();
        public ICollection<GitHubFileReference> Files { get; set; } = new List<GitHubFileReference>();
    }

    public class GitHubRepositoryEntityTypeConfiguration : IEntityTypeConfiguration<GitHubRepository>
    {
        public void Configure(EntityTypeBuilder<GitHubRepository> builder)
        {
            builder.ToTable("GitHubRepositories");

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedOnAdd();

            builder.Property(r => r.ProjectId).HasColumnType("bigint");
            builder.HasOne(r => r.Project)
                   .WithMany(p => p.Contributions) // optional: change if you prefer p.GitHubRepositories
                   .HasForeignKey(r => r.ProjectId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.Property(r => r.ExternalRepoId).HasColumnType("bigint").IsRequired();
            builder.Property(r => r.Owner).HasColumnType("nvarchar(200)").IsRequired();
            builder.Property(r => r.Name).HasColumnType("nvarchar(200)").IsRequired();
            builder.Property(r => r.FullName).HasColumnType("nvarchar(400)").IsRequired();
            builder.Property(r => r.Url).HasColumnType("nvarchar(1000)").IsRequired();
            builder.Property(r => r.Description).HasColumnType("nvarchar(max)");
            builder.Property(r => r.Private).HasColumnType("bit").IsRequired();
            builder.Property(r => r.DefaultBranch).HasColumnType("nvarchar(200)");
            builder.Property(r => r.MetadataJson).HasColumnType("nvarchar(max)");

            builder.Property(r => r.StargazersCount).HasColumnType("int").IsRequired().HasDefaultValue(0);
            builder.Property(r => r.ForksCount).HasColumnType("int").IsRequired().HasDefaultValue(0);
            builder.Property(r => r.OpenIssuesCount).HasColumnType("int").IsRequired().HasDefaultValue(0);

            builder.Property(r => r.FetchedAt).HasColumnType("datetime2").IsRequired();
            builder.Property(r => r.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();
            builder.Property(r => r.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

            builder.HasIndex(r => r.ExternalRepoId).HasDatabaseName("IX_GHRepos_ExternalRepoId");
            builder.HasIndex(r => r.FullName).IsUnique().HasDatabaseName("UQ_GHRepos_FullName");
        }
    }
}
