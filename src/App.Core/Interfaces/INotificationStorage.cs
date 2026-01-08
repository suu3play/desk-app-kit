using DeskAppKit.Core.Models;

namespace DeskAppKit.Core.Interfaces;

/// <summary>
/// 通知ストレージインターフェース
/// </summary>
public interface INotificationStorage
{
    /// <summary>
    /// 通知を保存
    /// </summary>
    Task SaveNotificationAsync(Notification notification);

    /// <summary>
    /// すべての通知を取得
    /// </summary>
    Task<IEnumerable<Notification>> GetAllNotificationsAsync();

    /// <summary>
    /// 通知を更新
    /// </summary>
    Task UpdateNotificationAsync(Notification notification);

    /// <summary>
    /// 通知を削除
    /// </summary>
    Task DeleteNotificationAsync(Guid notificationId);

    /// <summary>
    /// 古い通知を削除
    /// </summary>
    Task DeleteOldNotificationsAsync(DateTime threshold);

    /// <summary>
    /// すべての通知をクリア
    /// </summary>
    Task ClearAllAsync();
}
