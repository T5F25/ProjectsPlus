using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Github
{
    public class GitHubCommit
    {
        public long Id { get; set; }
        public long GitHubRepositoryId { get; set; }
        public GitHubRepository GitHubRepository { get; set; } = null!;

        public string Sha { get; set; } = null!;           // commit SHA
        public string AuthorName { get; set; } = null!;
        public string AuthorEmail { get; set; } = null!;
        public DateTimeOffset CommitDate { get; set; }
        public string Message { get; set; } = null!;
        public string? Url { get; set; }
        public string? MetadataJson { get; set; }          // raw commit JSON

        public DateTimeOffset CreatedAt { get; set; }
    }

    public class GitHubCommitEntityTypeConfiguration : IEntityTypeConfiguration<GitHubCommit>
    {
        public void Configure(EntityTypeBuilder<GitHubCommit> builder)
        {
            builder.ToTable("GitHubCommits");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).ValueGeneratedOnAdd();

            builder.Property(c => c.GitHubRepositoryId).HasColumnType("bigint").IsRequired();
            builder.HasOne(c => c.GitHubRepository)
                   .WithMany(r => r.Commits)
                   .HasForeignKey(c => c.GitHubRepositoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(c => c.Sha).HasColumnType("nvarchar(100)").IsRequired();
            builder.Property(c => c.AuthorName).HasColumnType("nvarchar(200)").IsRequired();
            builder.Property(c => c.AuthorEmail).HasColumnType("nvarchar(500)").IsRequired();
            builder.Property(c => c.CommitDate).HasColumnType("datetime2").IsRequired();
            builder.Property(c => c.Message).HasColumnType("nvarchar(max)").IsRequired();
            builder.Property(c => c.Url).HasColumnType("nvarchar(1000)");
            builder.Property(c => c.MetadataJson).HasColumnType("nvarchar(max)");

            builder.Property(c => c.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("SYSUTCDATETIME()").IsRequired();

            builder.HasIndex(c => new { c.GitHubRepositoryId, c.Sha }).IsUnique().HasDatabaseName("UQ_GHCommits_Repo_Sha");
            builder.HasIndex(c => new { c.GitHubRepositoryId, c.CommitDate }).HasDatabaseName("IX_GHCommits_Repo_CommitDate");
        }
    }
}
