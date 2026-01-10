using DeskAppKit.Core.Models;

namespace DeskAppKit.Core.Interfaces;

/// <summary>
/// 監査ログストレージインターフェース
/// </summary>
public interface IAuditLogStorage
{
    /// <summary>
    /// 監査ログを保存
    /// </summary>
    Task SaveAsync(AuditLog log);

    /// <summary>
    /// すべての監査ログを取得
    /// </summary>
    Task<IEnumerable<AuditLog>> GetAllAsync();

    /// <summary>
    /// 古い監査ログを削除
    /// </summary>
    Task DeleteOldLogsAsync(DateTime threshold);
}
