using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models
{
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }

    public class RoleEntityTypeConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");

            builder.HasKey(r => r.Id);
            builder.Property(r => r.Id).ValueGeneratedOnAdd();

            builder.Property(r => r.Name)
                   .HasColumnType("nvarchar(100)")
                   .IsRequired();

            builder.HasIndex(r => r.Name)
                   .IsUnique()
                   .HasDatabaseName("UQ_Roles_Name");

            builder.Property(r => r.Description)
                   .HasColumnType("nvarchar(1000)");
        }
    }
}
