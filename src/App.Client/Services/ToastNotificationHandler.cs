using System;
using System.Threading.Tasks;
using System.Windows;
using DeskAppKit.Core.Notifications;

namespace DeskAppKit.Client.Services;

/// <summary>
/// WPF用トースト通知ハンドラー
/// </summary>
public class ToastNotificationHandler : INotificationHandler
{
    private readonly Application _application;

    public ToastNotificationHandler(Application application)
    {
        _application = application ?? throw new ArgumentNullException(nameof(application));
    }

    public bool CanHandle(Notification notification)
    {
        // すべての通知を処理可能
        return true;
    }

    public Task HandleAsync(Notification notification)
    {
        // UIスレッドで実行
        _application.Dispatcher.Invoke(() =>
        {
            var icon = GetIcon(notification.Level);
            var color = GetColor(notification.Level);

            MessageBox.Show(
                _application.MainWindow,
                $"{icon} {notification.Message}",
                notification.Title,
                MessageBoxButton.OK,
                GetMessageBoxImage(notification.Level));
        });

        return Task.CompletedTask;
    }

    private string GetIcon(NotificationLevel level)
    {
        return level switch
        {
            NotificationLevel.Info => "ℹ️",
            NotificationLevel.Success => "✅",
            NotificationLevel.Warning => "⚠️",
            NotificationLevel.Error => "❌",
            _ => "ℹ️"
        };
    }

    private string GetColor(NotificationLevel level)
    {
        return level switch
        {
            NotificationLevel.Info => "#2196F3",
            NotificationLevel.Success => "#4CAF50",
            NotificationLevel.Warning => "#FF9800",
            NotificationLevel.Error => "#F44336",
            _ => "#2196F3"
        };
    }

    private MessageBoxImage GetMessageBoxImage(NotificationLevel level)
    {
        return level switch
        {
            NotificationLevel.Info => MessageBoxImage.Information,
            NotificationLevel.Success => MessageBoxImage.Information,
            NotificationLevel.Warning => MessageBoxImage.Warning,
            NotificationLevel.Error => MessageBoxImage.Error,
            _ => MessageBoxImage.Information
        };
    }
}
