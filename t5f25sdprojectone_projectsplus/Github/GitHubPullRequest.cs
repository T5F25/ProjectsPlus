using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Github
{
    public class GitHubPullRequest
    {
        public long Id { get; set; }
        public long GitHubRepositoryId { get; set; }
        public GitHubRepository GitHubRepository { get; set; } = null!;

        public long ExternalPrId { get; set; }            // pull request number
        public string Title { get; set; } = null!;
        public string? Body { get; set; }
        public string State { get; set; } = null!;        // open, closed, merged
        public string? HeadRef { get; set; }
        public string? BaseRef { get; set; }
        public string? Url { get; set; }
        public string? MetadataJson { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? MergedAt { get; set; }
        public DateTimeOffset FetchedAt { get; set; }
    }

    public class GitHubPullRequestEntityTypeConfiguration : IEntityTypeConfiguration<GitHubPullRequest>
    {
        public void Configure(EntityTypeBuilder<GitHubPullRequest> builder)
        {
            builder.ToTable("GitHubPullRequests");

            builder.HasKey(pr => pr.Id);
            builder.Property(pr => pr.Id).ValueGeneratedOnAdd();

            builder.Property(pr => pr.GitHubRepositoryId).HasColumnType("bigint").IsRequired();
            builder.HasOne(pr => pr.GitHubRepository)
                   .WithMany(r => r.PullRequests)
                   .HasForeignKey(pr => pr.GitHubRepositoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(pr => pr.ExternalPrId).HasColumnType("bigint").IsRequired();
            builder.Property(pr => pr.Title).HasColumnType("nvarchar(1000)").IsRequired();
            builder.Property(pr => pr.Body).HasColumnType("nvarchar(max)");
            builder.Property(pr => pr.State).HasColumnType("nvarchar(50)").IsRequired();
            builder.Property(pr => pr.HeadRef).HasColumnType("nvarchar(500)");
            builder.Property(pr => pr.BaseRef).HasColumnType("nvarchar(500)");
            builder.Property(pr => pr.Url).HasColumnType("nvarchar(1000)");
            builder.Property(pr => pr.MetadataJson).HasColumnType("nvarchar(max)");

            builder.Property(pr => pr.CreatedAt).HasColumnType("datetime2").IsRequired();
            builder.Property(pr => pr.MergedAt).HasColumnType("datetime2");
            builder.Property(pr => pr.FetchedAt).HasColumnType("datetime2").IsRequired();

            builder.HasIndex(pr => new { pr.GitHubRepositoryId, pr.ExternalPrId }).IsUnique().HasDatabaseName("UQ_GHPRs_Repo_PrId");
            builder.HasIndex(pr => new { pr.GitHubRepositoryId, pr.State }).HasDatabaseName("IX_GHPRs_Repo_State");
        }
    }
}
