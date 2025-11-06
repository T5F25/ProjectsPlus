using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Github
{
    // References a file in a repo (blob/path). Not necessarily stored in our blobstore; stores pointers.
    public class GitHubFileReference
    {
        public long Id { get; set; }
        public long GitHubRepositoryId { get; set; }
        public GitHubRepository GitHubRepository { get; set; } = null!;

        public string Path { get; set; } = null!;      // path within repo
        public string Sha { get; set; } = null!;       // blob SHA
        public long Size { get; set; }                 // bytes
        public string? Url { get; set; }               // raw url or API url
        public string? MetadataJson { get; set; }

        public DateTimeOffset FetchedAt { get; set; }
    }

    public class GitHubFileReferenceEntityTypeConfiguration : IEntityTypeConfiguration<GitHubFileReference>
    {
        public void Configure(EntityTypeBuilder<GitHubFileReference> builder)
        {
            builder.ToTable("GitHubFileReferences");

            builder.HasKey(f => f.Id);
            builder.Property(f => f.Id).ValueGeneratedOnAdd();

            builder.Property(f => f.GitHubRepositoryId).HasColumnType("bigint").IsRequired();
            builder.HasOne(f => f.GitHubRepository)
                   .WithMany(r => r.Files)
                   .HasForeignKey(f => f.GitHubRepositoryId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(f => f.Path).HasColumnType("nvarchar(1000)").IsRequired();
            builder.Property(f => f.Sha).HasColumnType("nvarchar(100)").IsRequired();
            builder.Property(f => f.Size).HasColumnType("bigint").IsRequired();
            builder.Property(f => f.Url).HasColumnType("nvarchar(1000)");
            builder.Property(f => f.MetadataJson).HasColumnType("nvarchar(max)");

            builder.Property(f => f.FetchedAt).HasColumnType("datetime2").IsRequired();

            builder.HasIndex(f => new { f.GitHubRepositoryId, f.Path }).IsUnique().HasDatabaseName("UQ_GHFiles_Repo_Path");
        }
    }
}
