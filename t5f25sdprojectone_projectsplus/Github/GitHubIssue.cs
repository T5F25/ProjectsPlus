using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Github
{
    public class GitHubIssue
    {
        public long Id { get; set; }
        public long GitHubRepositoryId { get; set; }
        public GitHubRepository GitHubRepository { get; set; } = null!;

        public long ExternalIssueId { get; set; }         // GitHub issue number
        public string Title { get; set; } = null!;
        public string? Body { get; set; }
        public string State { get; set; } = null!;        // open, closed
        public long? UserId { get; set; }                 // optional mapped local user
        public string? Url { get; set; }
        public string? MetadataJson { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ClosedAt { get; set; }
        public DateTimeOffset FetchedAt { get; set; }
    }

    public class GitHubIssueEntityTypeConfiguration : IEntityTypeConfiguration<GitHubIssue>
    {
        public void Configure(EntityTypeBuilder<GitHubIssue> builder)
        {
            builder.ToTable("GitHubIssues");

            builder.HasKey(i => i.Id);
            builder.Property(i => i.Id).ValueGeneratedOnAdd();

            builder.Property(i => i.GitHubRepositoryId).HasColumnType("bigint").IsRequired();
            builder.HasOne(i => i.GitHubRepository)
                   .WithMany(r => r.Issues)
                   .HasForeignKey(i => i.GitHubRepositoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(i => i.ExternalIssueId).HasColumnType("bigint").IsRequired();
            builder.Property(i => i.Title).HasColumnType("nvarchar(1000)").IsRequired();
            builder.Property(i => i.Body).HasColumnType("nvarchar(max)");
            builder.Property(i => i.State).HasColumnType("nvarchar(50)").IsRequired();
            builder.Property(i => i.Url).HasColumnType("nvarchar(1000)");
            builder.Property(i => i.MetadataJson).HasColumnType("nvarchar(max)");

            builder.Property(i => i.CreatedAt).HasColumnType("datetime2").IsRequired();
            builder.Property(i => i.ClosedAt).HasColumnType("datetime2");
            builder.Property(i => i.FetchedAt).HasColumnType("datetime2").IsRequired();

            builder.HasIndex(i => new { i.GitHubRepositoryId, i.ExternalIssueId }).IsUnique().HasDatabaseName("UQ_GHIssues_Repo_IssueId");
            builder.HasIndex(i => new { i.GitHubRepositoryId, i.State }).HasDatabaseName("IX_GHIssues_Repo_State");
        }
    }
}
