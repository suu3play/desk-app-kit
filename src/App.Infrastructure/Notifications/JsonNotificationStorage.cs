using System.Text.Json;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;

namespace DeskAppKit.Infrastructure.Notifications;

/// <summary>
/// JSON形式の通知ストレージ
/// </summary>
public class JsonNotificationStorage : INotificationStorage
{
    private readonly string _filePath;
    private readonly ILogger? _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public JsonNotificationStorage(string dataDirectory, ILogger? logger = null)
    {
        Directory.CreateDirectory(dataDirectory);
        _filePath = Path.Combine(dataDirectory, "notifications.json");
        _logger = logger;
    }

    public async Task SaveNotificationAsync(Notification notification)
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            var notifications = await LoadNotificationsFromFileAsync().ConfigureAwait(false);
            notifications.Add(notification);
            await SaveNotificationsToFileAsync(notifications).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<Notification>> GetAllNotificationsAsync()
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            return await LoadNotificationsFromFileAsync().ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task UpdateNotificationAsync(Notification notification)
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            var notifications = await LoadNotificationsFromFileAsync().ConfigureAwait(false);
            var index = notifications.FindIndex(n => n.Id == notification.Id);
            if (index >= 0)
            {
                notifications[index] = notification;
                await SaveNotificationsToFileAsync(notifications).ConfigureAwait(false);
            }
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeleteNotificationAsync(Guid notificationId)
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            var notifications = await LoadNotificationsFromFileAsync().ConfigureAwait(false);
            notifications.RemoveAll(n => n.Id == notificationId);
            await SaveNotificationsToFileAsync(notifications).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeleteOldNotificationsAsync(DateTime threshold)
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            var notifications = await LoadNotificationsFromFileAsync().ConfigureAwait(false);
            var count = notifications.RemoveAll(n => n.Timestamp < threshold);
            await SaveNotificationsToFileAsync(notifications).ConfigureAwait(false);
            _logger?.Info("JsonNotificationStorage", $"古い通知を削除しました: {count}件");
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task ClearAllAsync()
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            await SaveNotificationsToFileAsync(new List<Notification>()).ConfigureAwait(false);
            _logger?.Info("JsonNotificationStorage", "すべての通知を削除しました");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<Notification>> LoadNotificationsFromFileAsync()
    {
        if (!File.Exists(_filePath))
        {
            return new List<Notification>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(_filePath).ConfigureAwait(false);
            var notifications = JsonSerializer.Deserialize<List<Notification>>(json);
            return notifications ?? new List<Notification>();
        }
        catch (Exception ex)
        {
            _logger?.Error("JsonNotificationStorage", "通知読み込みエラー", ex);
            return new List<Notification>();
        }
    }

    private async Task SaveNotificationsToFileAsync(List<Notification> notifications)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var json = JsonSerializer.Serialize(notifications, options);
            await File.WriteAllTextAsync(_filePath, json).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.Error("JsonNotificationStorage", "通知保存エラー", ex);
            throw;
        }
    }
}
