using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!; // e.g., "Project.Create", "Project.Read"
        public string? Description { get; set; }
    }

    public class PermissionEntityTypeConfiguration : IEntityTypeConfiguration<Permission>
    {
        public void Configure(EntityTypeBuilder<Permission> builder)
        {
            builder.ToTable("Permissions");

            builder.HasKey(p => p.Id);
            builder.Property(p => p.Id).ValueGeneratedOnAdd();

            builder.Property(p => p.Name)
                   .HasColumnType("nvarchar(200)")
                   .IsRequired();

            builder.HasIndex(p => p.Name)
                   .IsUnique()
                   .HasDatabaseName("UQ_Permissions_Name");

            builder.Property(p => p.Description)
                   .HasColumnType("nvarchar(1000)");
        }
    }
}
