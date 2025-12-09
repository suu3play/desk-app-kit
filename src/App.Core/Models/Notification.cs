using DeskAppKit.Core.Enums;

namespace DeskAppKit.Core.Models;

/// <summary>
/// 通知情報
/// </summary>
public class Notification
{
    /// <summary>
    /// 通知ID
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// タイトル
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// メッセージ
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// 通知レベル
    /// </summary>
    public NotificationLevel Level { get; set; }

    /// <summary>
    /// タイムスタンプ
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 既読フラグ
    /// </summary>
    public bool IsRead { get; set; }

    /// <summary>
    /// カテゴリ
    /// </summary>
    public string? Category { get; set; }

    /// <summary>
    /// メタデータ（JSON形式）
    /// </summary>
    public string? MetadataJson { get; set; }

    /// <summary>
    /// ユーザーID
    /// </summary>
    public Guid? UserId { get; set; }
}
