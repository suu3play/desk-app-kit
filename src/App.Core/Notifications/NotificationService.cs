using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DeskAppKit.Core.Notifications;

/// <summary>
/// 通知サービス
/// アプリケーション全体で使用する通知機能を提供します
/// </summary>
public class NotificationService
{
    private readonly List<INotificationHandler> _handlers = new();

    /// <summary>
    /// 通知ハンドラーを登録します
    /// </summary>
    public void RegisterHandler(INotificationHandler handler)
    {
        if (handler == null) throw new ArgumentNullException(nameof(handler));
        _handlers.Add(handler);
    }

    /// <summary>
    /// 通知を送信します
    /// </summary>
    public async Task NotifyAsync(Notification notification)
    {
        if (notification == null) throw new ArgumentNullException(nameof(notification));

        var tasks = new List<Task>();

        foreach (var handler in _handlers)
        {
            if (handler.CanHandle(notification))
            {
                tasks.Add(handler.HandleAsync(notification));
            }
        }

        await Task.WhenAll(tasks);
    }

    /// <summary>
    /// 情報通知を送信します
    /// </summary>
    public Task NotifyInfoAsync(string title, string message)
    {
        return NotifyAsync(new Notification
        {
            Title = title,
            Message = message,
            Level = NotificationLevel.Info,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// 警告通知を送信します
    /// </summary>
    public Task NotifyWarningAsync(string title, string message)
    {
        return NotifyAsync(new Notification
        {
            Title = title,
            Message = message,
            Level = NotificationLevel.Warning,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// エラー通知を送信します
    /// </summary>
    public Task NotifyErrorAsync(string title, string message, Exception? exception = null)
    {
        return NotifyAsync(new Notification
        {
            Title = title,
            Message = message,
            Level = NotificationLevel.Error,
            Exception = exception,
            Timestamp = DateTime.Now
        });
    }

    /// <summary>
    /// 成功通知を送信します
    /// </summary>
    public Task NotifySuccessAsync(string title, string message)
    {
        return NotifyAsync(new Notification
        {
            Title = title,
            Message = message,
            Level = NotificationLevel.Success,
            Timestamp = DateTime.Now
        });
    }
}

/// <summary>
/// 通知
/// </summary>
public class Notification
{
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
    public DateTime Timestamp { get; set; }

    /// <summary>
    /// 例外情報（エラーの場合）
    /// </summary>
    public Exception? Exception { get; set; }

    /// <summary>
    /// 追加データ
    /// </summary>
    public Dictionary<string, object> Data { get; set; } = new();
}

/// <summary>
/// 通知レベル
/// </summary>
public enum NotificationLevel
{
    Info,
    Success,
    Warning,
    Error
}

/// <summary>
/// 通知ハンドラーインターフェース
/// </summary>
public interface INotificationHandler
{
    /// <summary>
    /// この通知を処理できるか判定します
    /// </summary>
    bool CanHandle(Notification notification);

    /// <summary>
    /// 通知を処理します
    /// </summary>
    Task HandleAsync(Notification notification);
}
