using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models
{
    public class RolePermission
    {
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public int PermissionId { get; set; }
        public Permission Permission { get; set; } = null!;

        public bool Allowed { get; set; } = false;
    }

    public class RolePermissionEntityTypeConfiguration : IEntityTypeConfiguration<RolePermission>
    {
        public void Configure(EntityTypeBuilder<RolePermission> builder)
        {
            builder.ToTable("RolePermissions");

            builder.HasKey(rp => new { rp.RoleId, rp.PermissionId });

            builder.Property(rp => rp.RoleId).HasColumnType("int").IsRequired();
            builder.HasOne(rp => rp.Role)
                   .WithMany()
                   .HasForeignKey(rp => rp.RoleId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(rp => rp.PermissionId).HasColumnType("int").IsRequired();
            builder.HasOne(rp => rp.Permission)
                   .WithMany()
                   .HasForeignKey(rp => rp.PermissionId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(rp => rp.Allowed)
                   .HasColumnType("bit")
                   .IsRequired();
        }
    }
}
