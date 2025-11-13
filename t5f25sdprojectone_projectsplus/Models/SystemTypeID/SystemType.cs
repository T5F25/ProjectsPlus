// Models/System/SystemType.cs
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace t5f25sdprojectone_projectsplus.Models.SystemTypeID
{
    [Table("system_types")]
    public class SystemType
    {
        [Key][Column("id")] public long Id { get; set; }
        [Required][Column("code")][MaxLength(200)] public string Code { get; set; } = null!;
        [Required][Column("display_name")][MaxLength(400)] public string DisplayName { get; set; } = null!;
        [Column("metadata_json")] public string? MetadataJson { get; set; }
        [Required][Column("is_active")] public bool IsActive { get; set; } = true;
        [Required][Column("created_at")] public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
        [Required][Column("updated_at")] public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
        [Required][Column("version")][ConcurrencyCheck] public int Version { get; set; } = 1;
        public override string ToString() => $"SystemType:{Id}:Code:{Code}:v{Version}";
    }
}
