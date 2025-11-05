using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models.Comunication
{
    // Payload for messages: text + attachments. Immutable-ish payload record.
    public class MessageContent
    {
        public long Id { get; set; }

        // Plain text (nullable for attachments-only messages)
        public string? Text { get; set; }

        // Metadata (e.g., markdown, parsing hints)
        public string? MetadataJson { get; set; }

        // Attachments stored via join table to FileReference
        public ICollection<FileReference> Attachments { get; set; } = new List<FileReference>();

        public DateTimeOffset CreatedAt { get; set; }
    }

    public class MessageContentEntityTypeConfiguration : IEntityTypeConfiguration<MessageContent>
    {
        public void Configure(EntityTypeBuilder<MessageContent> builder)
        {
            builder.ToTable("MessageContents");

            builder.HasKey(c => c.Id);
            builder.Property(c => c.Id).ValueGeneratedOnAdd();

            builder.Property(c => c.Text).HasColumnType("nvarchar(max)");
            builder.Property(c => c.MetadataJson).HasColumnType("nvarchar(max)");

            builder.Property(c => c.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder
                .HasMany(c => c.Attachments)
                .WithMany()
                .UsingEntity<Dictionary<string, object>>(
                    "MessageContentAttachment",
                    right => right.HasOne<FileReference>().WithMany().HasForeignKey("FileReferenceId").OnDelete(DeleteBehavior.Cascade),
                    left => left.HasOne<MessageContent>().WithMany().HasForeignKey("MessageContentId").OnDelete(DeleteBehavior.Cascade),
                    je =>
                    {
                        je.ToTable("MessageContentAttachments");
                        je.HasKey("MessageContentId", "FileReferenceId");
                        je.Property<long>("MessageContentId").HasColumnType("bigint");
                        je.Property<long>("FileReferenceId").HasColumnType("bigint");
                    });
        }
    }
}
