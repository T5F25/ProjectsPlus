using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models
{
    public class Attestation
    {
        public long Id { get; set; }

        // Subject of the attestation
        public long UserId { get; set; }
        public User User { get; set; } = null!;

        // Related project
        public long ProjectId { get; set; }
        public Project Project { get; set; } = null!;

        // What role/claim is being attested
        public string RoleName { get; set; } = null!;

        // Who issued the attestation (faculty/admin)
        public long IssuedById { get; set; }
        public User IssuedBy { get; set; } = null!;

        // When and immutable pointer to the rendered artifact
        public DateTimeOffset IssuedAt { get; set; }
        public string? PdfS3Key { get; set; }

        // Audit timestamps
        public DateTimeOffset CreatedAt { get; set; }
    }

    public class AttestationEntityTypeConfiguration : IEntityTypeConfiguration<Attestation>
    {
        public void Configure(EntityTypeBuilder<Attestation> builder)
        {
            builder.ToTable("Attestations");

            builder.HasKey(a => a.Id);
            builder.Property(a => a.Id).ValueGeneratedOnAdd();

            builder.Property(a => a.UserId)
                   .HasColumnType("bigint")
                   .IsRequired();
            builder.HasOne(a => a.User)
                   .WithMany(u => u.AttestationsIssued) // reuse collection; Accept that Issued vs Received nav may be adjusted later
                   .HasForeignKey(a => a.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(a => a.ProjectId)
                   .HasColumnType("bigint")
                   .IsRequired();
            builder.HasOne(a => a.Project)
                   .WithMany(p => p.Attestations)
                   .HasForeignKey(a => a.ProjectId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(a => a.RoleName)
                   .HasColumnType("nvarchar(100)")
                   .IsRequired();

            builder.Property(a => a.IssuedById)
                   .HasColumnType("bigint")
                   .IsRequired();
            builder.HasOne(a => a.IssuedBy)
                   .WithMany()
                   .HasForeignKey(a => a.IssuedById)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.Property(a => a.IssuedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.Property(a => a.PdfS3Key)
                   .HasColumnType("nvarchar(1000)");

            builder.Property(a => a.CreatedAt)
                   .HasColumnType("datetime2")
                   .HasDefaultValueSql("SYSUTCDATETIME()")
                   .IsRequired();

            builder.HasIndex(a => a.UserId).HasDatabaseName("IX_Attestations_UserId");
            builder.HasIndex(a => a.ProjectId).HasDatabaseName("IX_Attestations_ProjectId");
        }
    }
}
