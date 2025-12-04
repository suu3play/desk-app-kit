using DeskAppKit.Core.Enums;

namespace DeskAppKit.Core.Models;

/// <summary>
/// 設定情報
/// </summary>
public class Setting
{
    public Guid SettingId { get; set; }
    public SettingScopeType ScopeType { get; set; }
    public Guid? UserId { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Key { get; set; } = string.Empty;
    public string ValueEncrypted { get; set; } = string.Empty;
    public string DataType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public Guid? UpdatedByUserId { get; set; }
    public DateTime UpdatedAt { get; set; }
}
