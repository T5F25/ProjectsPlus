using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models.Comunication
{
    // ChatRoom represents a conversation surface.
    // Types:
    // 0 = Global (application-wide; admins send system notifications)
    // 1 = ProjectChannel (one per Project)
    // 2 = WorkspaceChannel (one per Workspace)
    // 3 = DirectMessage (between users)
    // 4 = Group (ad-hoc group)
    // 5 = System (reserved)
    public class ChatRoom
    {
        public long Id { get; set; }

        // Optional linkage to Project or Workspace depending on Type
        public long? ProjectId { get; set; }
        public Project? Project { get; set; }

        public long? WorkspaceId { get; set; }
        public Workspace? Workspace { get; set; }

        // For DirectMessage and Group rooms we store a canonical display name
        public string Name { get; set; } = null!;

        // See type enum above
        public byte Type { get; set; }

        // Optional owner/creator (user who created the room or null for system/global)
        public long? OwnerId { get; set; }
        public User? Owner { get; set; }

        // Members of the room (user membership, roles, join date)
        public ICollection<ChatRoomMember> Members { get; set; } = new List<ChatRoomMember>();

        // Messages in the room
        public ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();

        // Audit
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ArchivedAt { get; set; }
    }

    public class ChatRoomEntityTypeConfiguration : IEntityTypeConfiguration<ChatRoom>
    {
        public void Configure(EntityTypeBuilder<ChatRoom> builder)
        {
            builder.ToTable("ChatRooms");

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedOnAdd();

            builder.Property(r => r.ProjectId).HasColumnType("bigint");
            builder.HasOne(r => r.Project)
                   .WithMany(p => p.ChatRooms)
                   .HasForeignKey(r => r.ProjectId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(r => r.WorkspaceId).HasColumnType("bigint");
            builder.HasOne(r => r.Workspace)
                   .WithMany(w => w.ChatRooms)
                   .HasForeignKey(r => r.WorkspaceId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(r => r.Name).HasColumnType("nvarchar(500)").IsRequired();
            builder.Property(r => r.Type).HasColumnType("tinyint").IsRequired();

            builder.Property(r => r.OwnerId).HasColumnType("bigint");
            builder.HasOne(r => r.Owner)
                   .WithMany()
                   .HasForeignKey(r => r.OwnerId)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.Property(r => r.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(r => r.ArchivedAt).HasColumnType("datetime2");

            // Indexes: allow one project channel per project by convention (enforce in seed/provisioning)
            builder.HasIndex(r => new { r.ProjectId, r.Type }).HasDatabaseName("IX_ChatRooms_Project_Type");
            builder.HasIndex(r => new { r.WorkspaceId, r.Type }).HasDatabaseName("IX_ChatRooms_Workspace_Type");
        }
    }
}
