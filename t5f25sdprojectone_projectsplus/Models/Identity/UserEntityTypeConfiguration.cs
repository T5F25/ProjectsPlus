// Configurations/Identity/UserEntityTypeConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using t5f25sdprojectone_projectsplus.Models.SystemTypeID;


namespace t5f25sdprojectone_projectsplus.Models.Identity
{
    /// <summary>
    /// EF Core configuration for User entity.
    /// - Maps to canonical "users" table with explicit column names and indexes.
    /// - Declares unique index on email and concurrency token mapping for Version.
    /// - Does not cascade-delete any navigation; keep ownership explicit in services.
    /// </summary>
    public class UserEntityTypeConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("users");

            builder.HasKey(u => u.Id);
            builder.Property(u => u.Id)
                   .HasColumnName("id")
                   .ValueGeneratedOnAdd();

            builder.Property(f => f.SystemTypeId).HasColumnName("system_type_id").IsRequired();
            builder.HasOne<SystemType>()
                   .WithMany()
                   .HasForeignKey(nameof(User.SystemTypeId))
                   .OnDelete(DeleteBehavior.Restrict)
                   .HasConstraintName("fk_user_system_type_id_system_types");

            builder.Property(u => u.Email)
                   .HasColumnName("email")
                   .HasMaxLength(320)
                   .IsRequired();

            builder.Property(u => u.DisplayName)
                   .HasColumnName("display_name")
                   .HasMaxLength(200)
                   .IsRequired(false);

            builder.Property(u => u.Verified)
                   .HasColumnName("verified")
                   .IsRequired()
                   .HasDefaultValue(false);

            builder.Property(u => u.GithubId)
                   .HasColumnName("github_id")
                   .IsRequired(false);

            builder.Property(u => u.ProfileJson)
                   .HasColumnName("profile_json")
                   .HasMaxLength(4000)
                   .IsRequired(false);

            builder.Property(u => u.CreatedAt)
                   .HasColumnName("created_at")
                   .IsRequired();

            builder.Property(u => u.UpdatedAt)
                   .HasColumnName("updated_at")
                   .IsRequired();

            builder.Property(u => u.Version)
                   .HasColumnName("version")
                   .IsRequired()
                   .IsConcurrencyToken();

            builder.HasIndex(u => u.Email)
                   .IsUnique()
                   .HasDatabaseName("ix_users_email");

            builder.HasIndex(u => u.GithubId)
                   .HasDatabaseName("ix_users_github_id");           

        }
    }
}
