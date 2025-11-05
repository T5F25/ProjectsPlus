using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models.Comunication
{
    // Membership records for ChatRoom
    public class ChatRoomMember
    {
        public long ChatRoomId { get; set; }
        public ChatRoom ChatRoom { get; set; } = null!;

        public long UserId { get; set; }
        public User User { get; set; } = null!;

        // Role in room: 0=Member,1=Moderator,2=Admin
        public byte Role { get; set; }

        // Timestamps
        public DateTimeOffset JoinedAt { get; set; }
        public DateTimeOffset? LeftAt { get; set; }
    }

    public class ChatRoomMemberEntityTypeConfiguration : IEntityTypeConfiguration<ChatRoomMember>
    {
        public void Configure(EntityTypeBuilder<ChatRoomMember> builder)
        {
            builder.ToTable("ChatRoomMembers");

            builder.HasKey(m => new { m.ChatRoomId, m.UserId });

            builder.Property(m => m.ChatRoomId).HasColumnType("bigint").IsRequired();
            builder.HasOne(m => m.ChatRoom)
                   .WithMany(r => r.Members)
                   .HasForeignKey(m => m.ChatRoomId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(m => m.UserId).HasColumnType("bigint").IsRequired();
            builder.HasOne(m => m.User)
                   .WithMany()
                   .HasForeignKey(m => m.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(m => m.Role).HasColumnType("tinyint").IsRequired();

            builder.Property(m => m.JoinedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(m => m.LeftAt).HasColumnType("datetime2");

            builder.HasIndex(m => new { m.UserId }).HasDatabaseName("IX_ChatRoomMembers_UserId");
        }
    }
}
