using System.IO;
using System.Windows;
using System.Windows.Threading;
using DeskAppKit.Core.Interfaces;

namespace DeskAppKit.Client.Services;

/// <summary>
/// グローバル例外ハンドラー
/// </summary>
public class GlobalExceptionHandler
{
    private readonly ILogger _logger;
    private readonly string _errorLogDirectory;

    public GlobalExceptionHandler(ILogger logger, string errorLogDirectory)
    {
        _logger = logger;
        _errorLogDirectory = errorLogDirectory;
    }

    /// <summary>
    /// 例外ハンドラーを登録
    /// </summary>
    public void Register()
    {
        // UIスレッドの未処理例外
        Application.Current.DispatcherUnhandledException += OnDispatcherUnhandledException;

        // 非UIスレッドの未処理例外
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

        // タスクの未処理例外
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    /// <summary>
    /// UIスレッド未処理例外
    /// </summary>
    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _logger.Error("GlobalExceptionHandler", "UIスレッドで未処理例外が発生しました", e.Exception);

        var errorMessage = FormatErrorMessage(e.Exception);
        var errorId = SaveErrorReport(e.Exception);

        var result = MessageBox.Show(
            $"予期しないエラーが発生しました。\n\n{errorMessage}\n\nエラーID: {errorId}\n\nアプリケーションを続行しますか?",
            "エラー",
            MessageBoxButton.YesNo,
            MessageBoxImage.Error);

        if (result == MessageBoxResult.No)
        {
            Application.Current.Shutdown();
        }

        e.Handled = true;
    }

    /// <summary>
    /// 非UIスレッド未処理例外
    /// </summary>
    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            _logger.Error("GlobalExceptionHandler", "非UIスレッドで未処理例外が発生しました", ex);

            var errorMessage = FormatErrorMessage(ex);
            var errorId = SaveErrorReport(ex);

            MessageBox.Show(
                $"重大なエラーが発生しました。アプリケーションを終了します。\n\n{errorMessage}\n\nエラーID: {errorId}",
                "致命的なエラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            Application.Current.Shutdown();
        }
    }

    /// <summary>
    /// タスク未処理例外
    /// </summary>
    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _logger.Error("GlobalExceptionHandler", "タスクで未処理例外が発生しました", e.Exception);

        // ログに記録して続行
        e.SetObserved();
    }

    /// <summary>
    /// エラーメッセージをフォーマット
    /// </summary>
    private string FormatErrorMessage(Exception ex)
    {
        return ex.InnerException != null
            ? $"{ex.Message}\n\n詳細: {ex.InnerException.Message}"
            : ex.Message;
    }

    /// <summary>
    /// エラーレポートを保存
    /// </summary>
    private string SaveErrorReport(Exception ex)
    {
        try
        {
            Directory.CreateDirectory(_errorLogDirectory);

            var errorId = Guid.NewGuid().ToString("N").Substring(0, 8);
            var fileName = $"error_{DateTime.Now:yyyyMMdd_HHmmss}_{errorId}.txt";
            var filePath = Path.Combine(_errorLogDirectory, fileName);

            var errorReport = $"""
                エラーID: {errorId}
                発生日時: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

                【例外情報】
                タイプ: {ex.GetType().FullName}
                メッセージ: {ex.Message}

                【スタックトレース】
                {ex.StackTrace}

                【内部例外】
                {(ex.InnerException != null ? ex.InnerException.ToString() : "なし")}

                【環境情報】
                OS: {Environment.OSVersion}
                .NET: {Environment.Version}
                マシン名: {Environment.MachineName}
                ユーザー名: {Environment.UserName}
                作業ディレクトリ: {Environment.CurrentDirectory}
                """;

            File.WriteAllText(filePath, errorReport);

            return errorId;
        }
        catch (Exception saveEx)
        {
            _logger.Error("GlobalExceptionHandler", "エラーレポートの保存に失敗しました", saveEx);
            return "UNKNOWN";
        }
    }
}
