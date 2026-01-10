using DeskAppKit.Core.Enums;

namespace DeskAppKit.Core.Models;

/// <summary>
/// 監査ログ
/// </summary>
public class AuditLog
{
    /// <summary>
    /// ID
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// タイムスタンプ
    /// </summary>
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// ユーザーID
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// ユーザー名
    /// </summary>
    public string UserName { get; set; } = string.Empty;

    /// <summary>
    /// 操作種別
    /// </summary>
    public AuditAction Action { get; set; }

    /// <summary>
    /// エンティティ型
    /// </summary>
    public string EntityType { get; set; } = string.Empty;

    /// <summary>
    /// エンティティID
    /// </summary>
    public string? EntityId { get; set; }

    /// <summary>
    /// 変更前の値（JSON）
    /// </summary>
    public string? OldValue { get; set; }

    /// <summary>
    /// 変更後の値（JSON）
    /// </summary>
    public string? NewValue { get; set; }

    /// <summary>
    /// IPアドレス
    /// </summary>
    public string? IpAddress { get; set; }

    /// <summary>
    /// マシン名
    /// </summary>
    public string MachineName { get; set; } = Environment.MachineName;

    /// <summary>
    /// 変更理由
    /// </summary>
    public string? Reason { get; set; }

    /// <summary>
    /// 成功フラグ
    /// </summary>
    public bool IsSuccess { get; set; } = true;

    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string? ErrorMessage { get; set; }
}
