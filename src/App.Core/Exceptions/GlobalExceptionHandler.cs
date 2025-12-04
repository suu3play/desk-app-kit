using System;
using System.Threading.Tasks;

namespace DeskAppKit.Core.Exceptions;

/// <summary>
/// グローバル例外ハンドラー
/// アプリケーション全体の未処理例外をキャッチして処理します
/// </summary>
public class GlobalExceptionHandler
{
    private readonly IExceptionNotifier _notifier;
    private readonly IExceptionLogger _logger;

    public GlobalExceptionHandler(IExceptionNotifier notifier, IExceptionLogger logger)
    {
        _notifier = notifier ?? throw new ArgumentNullException(nameof(notifier));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// 例外を処理します
    /// </summary>
    public async Task HandleExceptionAsync(Exception exception, string context = "")
    {
        try
        {
            // ログに記録
            await _logger.LogExceptionAsync(exception, context);

            // ユーザーに通知
            var message = GetUserFriendlyMessage(exception);
            await _notifier.NotifyAsync(message, ExceptionSeverity.Error);
        }
        catch (Exception ex)
        {
            // ハンドラー自体で例外が発生した場合の最後の砦
            System.Diagnostics.Debug.WriteLine($"GlobalExceptionHandler failed: {ex.Message}");
        }
    }

    /// <summary>
    /// ユーザーフレンドリーなエラーメッセージを生成します
    /// </summary>
    private string GetUserFriendlyMessage(Exception exception)
    {
        return exception switch
        {
            InvalidOperationException => "操作を実行できませんでした。",
            UnauthorizedAccessException => "この操作を実行する権限がありません。",
            TimeoutException => "操作がタイムアウトしました。もう一度お試しください。",
            ArgumentException => "入力値が正しくありません。",
            System.IO.IOException => "ファイル操作中にエラーが発生しました。",
            System.Net.Http.HttpRequestException => "ネットワーク接続に問題があります。",
            _ => "予期しないエラーが発生しました。"
        };
    }
}

/// <summary>
/// 例外通知インターフェース
/// </summary>
public interface IExceptionNotifier
{
    Task NotifyAsync(string message, ExceptionSeverity severity);
}

/// <summary>
/// 例外ログ記録インターフェース
/// </summary>
public interface IExceptionLogger
{
    Task LogExceptionAsync(Exception exception, string context);
}

/// <summary>
/// 例外の重要度
/// </summary>
public enum ExceptionSeverity
{
    Info,
    Warning,
    Error,
    Critical
}
