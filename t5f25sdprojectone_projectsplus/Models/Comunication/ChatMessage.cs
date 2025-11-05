using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models.Comunication
{
    public class ChatMessage
    {
        public long Id { get; set; }

        // Relation to ChatRoom (primary conversation surface)
        public long ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; } = null!;

        // Optional link to Project for quick scoping/search (denormalized)
        public long? ProjectId { get; set; }
        public Project? Project { get; set; }

        // Sender (required)
        public long SenderId { get; set; }
        public User Sender { get; set; } = null!;

        // Optional explicit receiver (for DMs or when addressed to one user)
        public long? ReceiverId { get; set; }
        public User? Receiver { get; set; }

        // Threading (reply to another message)
        public long? ParentMessageId { get; set; }
        public ChatMessage? ParentMessage { get; set; }

        // Content object (text + attachments)
        public long MessageContentId { get; set; }
        public MessageContent MessageContent { get; set; } = null!;

        // Delivery/read flags (basic)
        public bool IsEdited { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? EditedAt { get; set; }

        // Notes: keep your comments in code where helpful
        // e.g., this model separates metadata (sender/room) from message payload (MessageContent)
    }

    public class ChatMessageEntityTypeConfiguration : IEntityTypeConfiguration<ChatMessage>
    {
        public void Configure(EntityTypeBuilder<ChatMessage> builder)
        {
            builder.ToTable("ChatMessages");

            builder.HasKey(m => m.Id);
            builder.Property(m => m.Id).ValueGeneratedOnAdd();

            // ChatRoom link
            builder.Property(m => m.ChatRoomId).HasColumnType("bigint").IsRequired();
            builder.HasOne(m => m.ChatRoom)
                   .WithMany(r => r.Messages)
                   .HasForeignKey(m => m.ChatRoomId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Optional denormalized Project link
            builder.Property(m => m.ProjectId).HasColumnType("bigint");
            builder.HasOne(m => m.Project)
                   .WithMany(p => p.ChatMessages)
                   .HasForeignKey(m => m.ProjectId)
                   .OnDelete(DeleteBehavior.SetNull);

            // Sender
            builder.Property(m => m.SenderId).HasColumnType("bigint").IsRequired();
            builder.HasOne(m => m.Sender)
                   .WithMany()
                   .HasForeignKey(m => m.SenderId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Receiver (optional)
            builder.Property(m => m.ReceiverId).HasColumnType("bigint");
            builder.HasOne(m => m.Receiver)
                   .WithMany()
                   .HasForeignKey(m => m.ReceiverId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Threading
            builder.Property(m => m.ParentMessageId).HasColumnType("bigint");
            builder.HasOne(m => m.ParentMessage)
                   .WithMany()
                   .HasForeignKey(m => m.ParentMessageId)
                   .OnDelete(DeleteBehavior.Restrict);

            // MessageContent (payload)
            builder.Property(m => m.MessageContentId).HasColumnType("bigint").IsRequired();
            builder.HasOne(m => m.MessageContent)
                   .WithMany()
                   .HasForeignKey(m => m.MessageContentId)
                   .OnDelete(DeleteBehavior.Cascade);

            // Flags and timestamps
            builder.Property(m => m.IsEdited).HasColumnType("bit").IsRequired().HasDefaultValue(false);
            builder.Property(m => m.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();
            builder.Property(m => m.EditedAt).HasColumnType("datetime2");

            // Indexes
            builder.HasIndex(m => new { m.ChatRoomId, m.CreatedAt }).HasDatabaseName("IX_ChatMessages_Room_CreatedAt");
            builder.HasIndex(m => new { m.ProjectId, m.CreatedAt }).HasDatabaseName("IX_ChatMessages_Project_CreatedAt");
        }
    }
}
