// Models/Identity/User.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace t5f25sdprojectone_projectsplus.Models.Identity
{
    /// <summary>
    /// Canonical local user account.
    /// - Numeric long Id PK.
    /// - Email is required and globally unique.
    /// - DisplayName is optional for UI.
    /// - GithubId is optional numeric provider id (kept for quick joins).
    /// - ProfileJson is small JSON blob for UI/display preferences; avoid storing provider full payloads here.
    /// - CreatedAt/UpdatedAt maintained by repository logic (UtcNow).
    /// - Version used for optimistic concurrency and cache invalidation.
    /// - Deterministic ToString used for logs and audits:
    ///   "User:{Id}:E:{Email}:v{Version}"
    /// </summary>
    [Table("users")]
    public class User
    {
        [Key]
        [Column("id")]
        public long Id { get; set; }

        // inside FileRecord POCO
        [Required]
        [Column("system_type_id")]
        public long SystemTypeId { get; set; }


        [Required]
        [MaxLength(320)]
        [Column("email")]
        public string Email { get; set; } = null!;

        [MaxLength(200)]
        [Column("display_name")]
        public string? DisplayName { get; set; }

        [Required]
        [Column("verified")]
        public bool Verified { get; set; } = false;

        [Column("github_id")]
        public long? GithubId { get; set; }

        [MaxLength(4000)]
        [Column("profile_json")]
        public string? ProfileJson { get; set; }

        [Required]
        [Column("created_at")]
        public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Column("updated_at")]
        public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

        [Required]
        [Column("version")]
        [ConcurrencyCheck]
        public int Version { get; set; } = 1;

        /// <summary>
        /// Deterministic ToString for logs/audits.
        /// Format: "User:{Id}:E:{Email}:v{Version}"
        /// </summary>
        public override string ToString()
        {
            return $"User:{Id}:E:{Email}:v{Version}";
        }
    }
}
