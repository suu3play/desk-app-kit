using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;
using DeskAppKit.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DeskAppKit.Infrastructure.Notifications;

/// <summary>
/// データベース形式の通知ストレージ
/// </summary>
public class DatabaseNotificationStorage : INotificationStorage
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger? _logger;

    public DatabaseNotificationStorage(AppDbContext dbContext, ILogger? logger = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger;
    }

    public async Task SaveNotificationAsync(Notification notification)
    {
        try
        {
            await _dbContext.Notifications.AddAsync(notification).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.Error("DatabaseNotificationStorage", "通知保存エラー", ex);
            throw;
        }
    }

    public async Task<IEnumerable<Notification>> GetAllNotificationsAsync()
    {
        try
        {
            return await _dbContext.Notifications
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.Error("DatabaseNotificationStorage", "通知取得エラー", ex);
            return new List<Notification>();
        }
    }

    public async Task UpdateNotificationAsync(Notification notification)
    {
        try
        {
            _dbContext.Notifications.Update(notification);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.Error("DatabaseNotificationStorage", "通知更新エラー", ex);
            throw;
        }
    }

    public async Task DeleteNotificationAsync(Guid notificationId)
    {
        try
        {
            var notification = await _dbContext.Notifications
                .FirstOrDefaultAsync(n => n.Id == notificationId)
                .ConfigureAwait(false);

            if (notification != null)
            {
                _dbContext.Notifications.Remove(notification);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger?.Error("DatabaseNotificationStorage", "通知削除エラー", ex);
            throw;
        }
    }

    public async Task DeleteOldNotificationsAsync(DateTime threshold)
    {
        try
        {
            var oldNotifications = await _dbContext.Notifications
                .Where(n => n.Timestamp < threshold)
                .ToListAsync()
                .ConfigureAwait(false);

            if (oldNotifications.Any())
            {
                _dbContext.Notifications.RemoveRange(oldNotifications);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
                _logger?.Info("DatabaseNotificationStorage", $"古い通知を削除しました: {oldNotifications.Count}件");
            }
        }
        catch (Exception ex)
        {
            _logger?.Error("DatabaseNotificationStorage", "古い通知削除エラー", ex);
            throw;
        }
    }

    public async Task ClearAllAsync()
    {
        try
        {
            var allNotifications = await _dbContext.Notifications.ToListAsync().ConfigureAwait(false);
            _dbContext.Notifications.RemoveRange(allNotifications);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
            _logger?.Info("DatabaseNotificationStorage", "すべての通知を削除しました");
        }
        catch (Exception ex)
        {
            _logger?.Error("DatabaseNotificationStorage", "全通知削除エラー", ex);
            throw;
        }
    }
}
