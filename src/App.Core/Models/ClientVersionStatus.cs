using DeskAppKit.Core.Enums;

namespace DeskAppKit.Core.Models;

/// <summary>
/// クライアントバージョン状態
/// </summary>
public class ClientVersionStatus
{
    public Guid ClientVersionStatusId { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public string CurrentAppVersion { get; set; } = string.Empty;
    public string CurrentSchemaVersion { get; set; } = string.Empty;
    public bool AutoUpdateEnabled { get; set; }
    public DateTime? LastUpdateCheckAt { get; set; }
    public UpdateResult? LastUpdateResult { get; set; }
    public string? LastUpdateErrorMessage { get; set; }
    public bool IsActive { get; set; }
}
