using System.Collections.Concurrent;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;

namespace DeskAppKit.Infrastructure.Notifications;

/// <summary>
/// 通知センター実装
/// </summary>
public class NotificationCenter : INotificationCenter
{
    private readonly ConcurrentBag<Notification> _notifications = new();
    private readonly ILogger? _logger;

    public event EventHandler<NotificationEventArgs>? NotificationAdded;

    public NotificationCenter(ILogger? logger = null)
    {
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

        _notifications.Add(notification);
        _logger?.Info("NotificationCenter", $"通知追加: {notification.Title}");

        NotificationAdded?.Invoke(this, new NotificationEventArgs(notification));
    }

    /// <summary>
    /// 通知を取得
    /// </summary>
    public Task<IEnumerable<Notification>> GetNotificationsAsync(NotificationFilter? filter = null)
    {
        var query = _notifications.AsEnumerable();

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

        var result = query.OrderByDescending(n => n.Timestamp).ToList();
        return Task.FromResult<IEnumerable<Notification>>(result);
    }

    /// <summary>
    /// 通知を既読にする
    /// </summary>
    public Task MarkAsReadAsync(Guid notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification != null)
        {
            notification.IsRead = true;
            _logger?.Debug("NotificationCenter", $"通知既読: {notificationId}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// すべての通知を既読にする
    /// </summary>
    public Task MarkAllAsReadAsync()
    {
        foreach (var notification in _notifications)
        {
            notification.IsRead = true;
        }

        _logger?.Info("NotificationCenter", "すべての通知を既読にしました");
        return Task.CompletedTask;
    }

    /// <summary>
    /// 通知を削除
    /// </summary>
    public Task DeleteNotificationAsync(Guid notificationId)
    {
        var notification = _notifications.FirstOrDefault(n => n.Id == notificationId);
        if (notification != null)
        {
            // ConcurrentBagから削除する代わりに、新しいリストを作成
            var remaining = _notifications.Where(n => n.Id != notificationId).ToList();
            _notifications.Clear();
            foreach (var n in remaining)
            {
                _notifications.Add(n);
            }

            _logger?.Info("NotificationCenter", $"通知削除: {notificationId}");
        }

        return Task.CompletedTask;
    }

    /// <summary>
    /// 古い通知をクリア
    /// </summary>
    public Task ClearOldNotificationsAsync(TimeSpan maxAge)
    {
        var threshold = DateTime.UtcNow - maxAge;
        var toDelete = _notifications.Where(n => n.Timestamp < threshold).Select(n => n.Id).ToList();

        foreach (var id in toDelete)
        {
            DeleteNotificationAsync(id).Wait();
        }

        _logger?.Info("NotificationCenter", $"古い通知を削除しました: {toDelete.Count}件");
        return Task.CompletedTask;
    }

    /// <summary>
    /// 未読件数を取得
    /// </summary>
    public Task<int> GetUnreadCountAsync()
    {
        var count = _notifications.Count(n => !n.IsRead);
        return Task.FromResult(count);
    }
}
