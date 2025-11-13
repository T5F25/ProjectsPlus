// Configurations/System/SystemTypeEntityTypeConfiguration.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace t5f25sdprojectone_projectsplus.Models.SystemTypeID
{
    public class SystemTypeEntityTypeConfiguration : IEntityTypeConfiguration<SystemType>
    {
        public void Configure(EntityTypeBuilder<SystemType> builder)
        {
            builder.ToTable("system_types");

            builder.HasKey(t => t.Id);
            builder.Property(t => t.Id).HasColumnName("id").ValueGeneratedOnAdd();

            builder.Property(t => t.Code).HasColumnName("code").HasMaxLength(200).IsRequired();
            builder.Property(t => t.DisplayName).HasColumnName("display_name").HasMaxLength(400).IsRequired();
            builder.Property(t => t.MetadataJson).HasColumnName("metadata_json").IsRequired(false);
            builder.Property(t => t.IsActive).HasColumnName("is_active").IsRequired();
            builder.Property(t => t.CreatedAt).HasColumnName("created_at").IsRequired();
            builder.Property(t => t.UpdatedAt).HasColumnName("updated_at").IsRequired();
            builder.Property(t => t.Version).HasColumnName("version").IsRequired().IsConcurrencyToken();

            builder.HasIndex(t => t.Code).IsUnique().HasDatabaseName("ux_system_types_code");

            // Seed canonical types referenced by domain models created so far.
            // NOTE: adjust timestamps/ids if you already seeded or use migrations to set ids deterministically.
            builder.HasData(
                // Projects
                new SystemType { Id = 1, Code = "project:standard", DisplayName = "Project — standard", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },
                new SystemType { Id = 2, Code = "project:template", DisplayName = "Project — template", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },

                // Files
                new SystemType { Id = 10, Code = "file:generic", DisplayName = "File — generic", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },
                new SystemType { Id = 11, Code = "file:pdf", DisplayName = "File — PDF", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },
                new SystemType { Id = 12, Code = "file:zip", DisplayName = "File — ZIP", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },
                new SystemType { Id = 13, Code = "file:evidence", DisplayName = "File — legal evidence", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },
                new SystemType { Id = 14, Code = "file:image", DisplayName = "File — image", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },
                new SystemType { Id = 15, Code = "file:thumbnail", DisplayName = "File — thumbnail", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },

                // Attachments / Card attachments
                new SystemType { Id = 20, Code = "attachment:generic", DisplayName = "Attachment — generic", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },
                new SystemType { Id = 60, Code = "card_attachment", DisplayName = "Card Attachment", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },

                // Boards and columns
                new SystemType { Id = 30, Code = "board:user", DisplayName = "Board — user", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },
                new SystemType { Id = 31, Code = "board:system", DisplayName = "Board — system", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },
                new SystemType { Id = 40, Code = "column:standard", DisplayName = "Kanban Column — standard", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },

                // Cards and card types
                new SystemType { Id = 50, Code = "card:standard", DisplayName = "Kanban Card — standard", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },
                new SystemType { Id = 51, Code = "card:blocker", DisplayName = "Kanban Card — blocker", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },
                new SystemType { Id = 61, Code = "card_assignee", DisplayName = "Card Assignee", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },

                // Project membership and roles
                new SystemType { Id = 70, Code = "project:member", DisplayName = "Project Member", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },

                // Integrations / links
                new SystemType { Id = 80, Code = "github:project-link", DisplayName = "GitHub Project Link", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },
                new SystemType { Id = 81, Code = "github:user-link", DisplayName = "GitHub User Link", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },

                // Release/build artifacts and exports
                new SystemType { Id = 16, Code = "file:release", DisplayName = "File — release artifact", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },

                // Workspace and user
                new SystemType { Id = 90, Code = "workspace:standard", DisplayName = "Workspace — standard", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },
                new SystemType { Id = 100, Code = "user:internal", DisplayName = "User — internal", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },

                // System/admin artifacts
                new SystemType { Id = 110, Code = "system:notification", DisplayName = "System Notification", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 },
                new SystemType { Id = 111, Code = "system:audit", DisplayName = "System Audit Log", IsActive = true, CreatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), UpdatedAt = new DateTimeOffset(2025, 11, 13, 00, 00, 00, TimeSpan.Zero), Version = 1 }
            );


        }
    }
}
