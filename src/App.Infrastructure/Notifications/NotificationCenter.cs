using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;

namespace DeskAppKit.Infrastructure.Notifications;

/// <summary>
/// 通知センター実装
/// </summary>
public class NotificationCenter : INotificationCenter
{
    private readonly INotificationStorage _storage;
    private readonly ILogger? _logger;

    public event EventHandler<NotificationEventArgs>? NotificationAdded;

    public NotificationCenter(INotificationStorage storage, ILogger? logger = null)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _logger = logger;
    }

    /// <summary>
    /// 通知を追加
    /// </summary>
    public void AddNotification(Notification notification)
    {
        if (notification == null)
        {
            throw new ArgumentNullException(nameof(notification));
        }

        // 非同期保存を同期的に実行（UIスレッドからの呼び出しを想定）
        _storage.SaveNotificationAsync(notification).GetAwaiter().GetResult();
        _logger?.Info("NotificationCenter", $"通知追加: {notification.Title}");

        NotificationAdded?.Invoke(this, new NotificationEventArgs(notification));
    }

    /// <summary>
    /// 通知を取得
    /// </summary>
    public async Task<IEnumerable<Notification>> GetNotificationsAsync(NotificationFilter? filter = null)
    {
        var allNotifications = await _storage.GetAllNotificationsAsync().ConfigureAwait(false);
        var query = allNotifications.AsEnumerable();

        if (filter != null)
        {
            if (filter.Level.HasValue)
            {
                query = query.Where(n => n.Level == filter.Level.Value);
            }

            if (filter.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == filter.IsRead.Value);
            }

            if (filter.From.HasValue)
            {
                query = query.Where(n => n.Timestamp >= filter.From.Value);
            }

            if (filter.To.HasValue)
            {
                query = query.Where(n => n.Timestamp <= filter.To.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var searchText = filter.SearchText.ToLower();
                query = query.Where(n =>
                    n.Title.ToLower().Contains(searchText) ||
                    n.Message.ToLower().Contains(searchText));
            }

            if (!string.IsNullOrWhiteSpace(filter.Category))
            {
                query = query.Where(n => n.Category == filter.Category);
            }
        }

        return query.OrderByDescending(n => n.Timestamp).ToList();
    }

    /// <summary>
    /// 通知を既読にする
    /// </summary>
    public async Task MarkAsReadAsync(Guid notificationId)
    {
        var allNotifications = await _storage.GetAllNotificationsAsync().ConfigureAwait(false);
        var notification = allNotifications.FirstOrDefault(n => n.Id == notificationId);

        if (notification != null)
        {
            notification.IsRead = true;
            await _storage.UpdateNotificationAsync(notification).ConfigureAwait(false);
            _logger?.Debug("NotificationCenter", $"通知既読: {notificationId}");
        }
    }

    /// <summary>
    /// すべての通知を既読にする
    /// </summary>
    public async Task MarkAllAsReadAsync()
    {
        var allNotifications = await _storage.GetAllNotificationsAsync().ConfigureAwait(false);

        foreach (var notification in allNotifications)
        {
            if (!notification.IsRead)
            {
                notification.IsRead = true;
                await _storage.UpdateNotificationAsync(notification).ConfigureAwait(false);
            }
        }

        _logger?.Info("NotificationCenter", "すべての通知を既読にしました");
    }

    /// <summary>
    /// 通知を削除
    /// </summary>
    public async Task DeleteNotificationAsync(Guid notificationId)
    {
        await _storage.DeleteNotificationAsync(notificationId).ConfigureAwait(false);
        _logger?.Info("NotificationCenter", $"通知削除: {notificationId}");
    }

    /// <summary>
    /// 古い通知をクリア
    /// </summary>
    public async Task ClearOldNotificationsAsync(TimeSpan maxAge)
    {
        var threshold = DateTime.UtcNow - maxAge;
        await _storage.DeleteOldNotificationsAsync(threshold).ConfigureAwait(false);
        _logger?.Info("NotificationCenter", "古い通知を削除しました");
    }

    /// <summary>
    /// 未読件数を取得
    /// </summary>
    public async Task<int> GetUnreadCountAsync()
    {
        var allNotifications = await _storage.GetAllNotificationsAsync().ConfigureAwait(false);
        var count = allNotifications.Count(n => !n.IsRead);
        return count;
    }
}
