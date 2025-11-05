using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models
{
    public class FileReference
    {
        public long Id { get; set; }
        public string S3Key { get; set; } = null!;
        public string? ContentType { get; set; }
        public long LengthBytes { get; set; }
        public long? UploadedBy { get; set; }
        public User? UploadedByUser { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class FileReferenceEntityTypeConfiguration : IEntityTypeConfiguration<FileReference>
    {
        public void Configure(EntityTypeBuilder<FileReference> builder)
        {
            builder.ToTable("FileReferences");

            builder.HasKey(f => f.Id);
            builder.Property(f => f.Id).ValueGeneratedOnAdd();

            builder.Property(f => f.S3Key)
                   .HasColumnType("nvarchar(1000)")
                   .IsRequired();

            builder.Property(f => f.ContentType)
                   .HasColumnType("nvarchar(200)");

            builder.Property(f => f.LengthBytes)
                   .HasColumnType("bigint")
                   .IsRequired();

            builder.Property(f => f.UploadedBy)
                   .HasColumnType("bigint");
            builder.HasOne(f => f.UploadedByUser)
                   .WithMany()
                   .HasForeignKey(f => f.UploadedBy)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.Property(f => f.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.HasIndex(f => f.S3Key).HasDatabaseName("IX_FileReferences_S3Key");
        }
    }
}
