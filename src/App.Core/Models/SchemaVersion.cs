namespace DeskAppKit.Core.Models;

/// <summary>
/// スキーマバージョン情報
/// </summary>
public class SchemaVersion
{
    public Guid SchemaVersionId { get; set; }
    public string Version { get; set; } = string.Empty;
    public bool IsCurrent { get; set; }
    public bool IsAvailableForUpgrade { get; set; }
    public bool IsAvailableForDowngrade { get; set; }
    public bool DevApproved { get; set; }
    public DateTime? DevApprovedAt { get; set; }
    public Guid? DevApprovedBy { get; set; }
    public bool UserApproved { get; set; }
    public DateTime? UserApprovedAt { get; set; }
    public Guid? UserApprovedBy { get; set; }
    public string MigrationScriptUpPath { get; set; } = string.Empty;
    public string MigrationScriptDownPath { get; set; } = string.Empty;
    public DateTime ReleaseDate { get; set; }
    public string? Notes { get; set; }
    public DateTime UpdatedAt { get; set; }
}
