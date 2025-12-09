using DeskAppKit.Core.Enums;
using DeskAppKit.Core.Models;

namespace DeskAppKit.Core.Interfaces;

/// <summary>
/// 通知センターインターフェース
/// </summary>
public interface INotificationCenter
{
    /// <summary>
    /// 通知を追加
    /// </summary>
    void AddNotification(Notification notification);

    /// <summary>
    /// 通知を取得
    /// </summary>
    Task<IEnumerable<Notification>> GetNotificationsAsync(NotificationFilter? filter = null);

    /// <summary>
    /// 通知を既読にする
    /// </summary>
    Task MarkAsReadAsync(Guid notificationId);

    /// <summary>
    /// すべての通知を既読にする
    /// </summary>
    Task MarkAllAsReadAsync();

    /// <summary>
    /// 通知を削除
    /// </summary>
    Task DeleteNotificationAsync(Guid notificationId);

    /// <summary>
    /// 古い通知をクリア
    /// </summary>
    Task ClearOldNotificationsAsync(TimeSpan maxAge);

    /// <summary>
    /// 未読件数を取得
    /// </summary>
    Task<int> GetUnreadCountAsync();

    /// <summary>
    /// 通知追加イベント
    /// </summary>
    event EventHandler<NotificationEventArgs>? NotificationAdded;
}

/// <summary>
/// 通知フィルター
/// </summary>
public class NotificationFilter
{
    public NotificationLevel? Level { get; set; }
    public bool? IsRead { get; set; }
    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public string? SearchText { get; set; }
    public string? Category { get; set; }
}

/// <summary>
/// 通知イベント引数
/// </summary>
public class NotificationEventArgs : EventArgs
{
    public Notification Notification { get; set; }

    public NotificationEventArgs(Notification notification)
    {
        Notification = notification;
    }
}
