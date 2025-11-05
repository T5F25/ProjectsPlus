using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models
{
    public class UserRole
    {
        public long UserId { get; set; }
        public User User { get; set; } = null!;

        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public DateTimeOffset AssignedAt { get; set; }
    }

    public class UserRoleEntityTypeConfiguration : IEntityTypeConfiguration<UserRole>
    {
        public void Configure(EntityTypeBuilder<UserRole> builder)
        {
            builder.ToTable("UserRoles");

            builder.HasKey(ur => new { ur.UserId, ur.RoleId });

            builder.Property(ur => ur.UserId).HasColumnType("bigint").IsRequired();
            builder.HasOne(ur => ur.User)
                   .WithMany()
                   .HasForeignKey(ur => ur.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(ur => ur.RoleId).HasColumnType("int").IsRequired();
            builder.HasOne(ur => ur.Role)
                   .WithMany()
                   .HasForeignKey(ur => ur.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(ur => ur.AssignedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();
        }
    }
}
