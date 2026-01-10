using DeskAppKit.Core.Enums;
using DeskAppKit.Core.Models;

namespace DeskAppKit.Core.Interfaces;

/// <summary>
/// 監査ログサービスインターフェース
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// 監査ログを記録
    /// </summary>
    Task LogAsync(AuditLog log);

    /// <summary>
    /// 作成操作を記録
    /// </summary>
    Task LogCreateAsync<T>(T entity, Guid userId, string? reason = null);

    /// <summary>
    /// 更新操作を記録
    /// </summary>
    Task LogUpdateAsync<T>(T oldEntity, T newEntity, Guid userId, string? reason = null);

    /// <summary>
    /// 削除操作を記録
    /// </summary>
    Task LogDeleteAsync<T>(T entity, Guid userId, string? reason = null);

    /// <summary>
    /// ログイン操作を記録
    /// </summary>
    Task LogLoginAsync(Guid userId, string userName, bool isSuccess, string? errorMessage = null);

    /// <summary>
    /// ログアウト操作を記録
    /// </summary>
    Task LogLogoutAsync(Guid userId, string userName);

    /// <summary>
    /// 監査ログを取得
    /// </summary>
    Task<IEnumerable<AuditLog>> GetLogsAsync(AuditLogFilter? filter = null);

    /// <summary>
    /// エンティティの履歴を取得
    /// </summary>
    Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(string entityType, string entityId);
}

/// <summary>
/// 監査ログフィルター
/// </summary>
public class AuditLogFilter
{
    public Guid? UserId { get; set; }
    public AuditAction? Action { get; set; }
    public string? EntityType { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? SearchText { get; set; }
    public bool? IsSuccess { get; set; }
}
