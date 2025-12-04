using System;
using System.Threading.Tasks;
using DeskAppKit.Core.Exceptions;
using DeskAppKit.Core.Notifications;

namespace DeskAppKit.Infrastructure.Notifications;

/// <summary>
/// 例外通知実装クラス
/// </summary>
public class ExceptionNotifier : IExceptionNotifier
{
    private readonly NotificationService _notificationService;

    public ExceptionNotifier(NotificationService notificationService)
    {
        _notificationService = notificationService ?? throw new ArgumentNullException(nameof(notificationService));
    }

    public async Task NotifyAsync(string message, ExceptionSeverity severity)
    {
        switch (severity)
        {
            case ExceptionSeverity.Info:
                await _notificationService.NotifyInfoAsync("情報", message);
                break;

            case ExceptionSeverity.Warning:
                await _notificationService.NotifyWarningAsync("警告", message);
                break;

            case ExceptionSeverity.Error:
                await _notificationService.NotifyErrorAsync("エラー", message);
                break;

            case ExceptionSeverity.Critical:
                await _notificationService.NotifyErrorAsync("重大なエラー", message);
                break;

            default:
                await _notificationService.NotifyInfoAsync("通知", message);
                break;
        }
    }
}
