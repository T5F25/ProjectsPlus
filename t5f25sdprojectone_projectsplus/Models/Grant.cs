using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models
{
    public class Grant
    {
        public long Id { get; set; }

        // Resource identity; ResourceId = null means resource-type level (global for that type)
        public string ResourceType { get; set; } = null!;
        public long? ResourceId { get; set; }

        // Subject: either a user or a role
        public string SubjectType { get; set; } = null!; // "User" or "Role"
        public long SubjectId { get; set; }

        // CRUD bits
        public bool CanCreate { get; set; }
        public bool CanRead { get; set; }
        public bool CanUpdate { get; set; }
        public bool CanDelete { get; set; }

        public bool DenyWins { get; set; } = true;

        public long? GrantedBy { get; set; }
        public User? GrantedByUser { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
    }

    public class GrantEntityTypeConfiguration : IEntityTypeConfiguration<Grant>
    {
        public void Configure(EntityTypeBuilder<Grant> builder)
        {
            builder.ToTable("Grants");

            builder.HasKey(g => g.Id);
            builder.Property(g => g.Id).ValueGeneratedOnAdd();

            builder.Property(g => g.ResourceType)
                   .HasColumnType("nvarchar(200)")
                   .IsRequired();

            builder.Property(g => g.ResourceId)
                   .HasColumnType("bigint");

            builder.Property(g => g.SubjectType)
                   .HasColumnType("nvarchar(16)")
                   .IsRequired();

            builder.Property(g => g.SubjectId)
                   .HasColumnType("bigint")
                   .IsRequired();

            builder.Property(g => g.CanCreate).HasColumnType("bit").IsRequired();
            builder.Property(g => g.CanRead).HasColumnType("bit").IsRequired();
            builder.Property(g => g.CanUpdate).HasColumnType("bit").IsRequired();
            builder.Property(g => g.CanDelete).HasColumnType("bit").IsRequired();

            builder.Property(g => g.DenyWins).HasColumnType("bit").IsRequired().HasDefaultValue(true);

            builder.Property(g => g.GrantedBy).HasColumnType("bigint");
            builder.HasOne(g => g.GrantedByUser)
                   .WithMany()
                   .HasForeignKey(g => g.GrantedBy)
                   .OnDelete(DeleteBehavior.SetNull);

            builder.Property(g => g.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.HasIndex(g => new { g.ResourceType, g.ResourceId, g.SubjectType, g.SubjectId })
                   .HasDatabaseName("IX_Grants_Resource_Subject");

            builder.HasIndex(g => new { g.SubjectType, g.SubjectId })
                   .HasDatabaseName("IX_Grants_Subject");
        }
    }
}
