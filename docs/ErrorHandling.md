# エラーハンドリング機能

## 概要

DeskAppKitは、アプリケーション全体で統一されたエラーハンドリング機能を提供します。

## 主な機能

### 1. グローバル例外ハンドラー

アプリケーション全体の未処理例外をキャッチして適切に処理します。

```csharp
using DeskAppKit.Core.Exceptions;
using DeskAppKit.Core.Notifications;
using DeskAppKit.Infrastructure.Logging;
using DeskAppKit.Infrastructure.Notifications;

// セットアップ
var notificationService = new NotificationService();
var logger = new FileExceptionLogger();
var notifier = new ExceptionNotifier(notificationService);
var handler = new GlobalExceptionHandler(notifier, logger);

// 例外処理
try
{
    // 何か処理
}
catch (Exception ex)
{
    await handler.HandleExceptionAsync(ex, "データ読み込み処理");
}
```

### 2. 自動エラー通知

エラーが発生すると、自動的にユーザーに通知されます。

```csharp
// 情報通知
await notificationService.NotifyInfoAsync("処理完了", "データの保存が完了しました。");

// 警告通知
await notificationService.NotifyWarningAsync("注意", "一部のデータが古い可能性があります。");

// エラー通知
await notificationService.NotifyErrorAsync("エラー", "ファイルの読み込みに失敗しました。", exception);

// 成功通知
await notificationService.NotifySuccessAsync("成功", "操作が正常に完了しました。");
```

### 3. ファイルログ記録

すべての例外は自動的にファイルに記録されます。

- ログ保存先: `%AppData%\DeskAppKit\Logs\`
- ファイル名形式: `exceptions_yyyyMMdd.log`
- 自動ローテーション: 30日以上前のログは自動削除

```csharp
var logger = new FileExceptionLogger();

// 古いログのクリーンアップ（30日以上前）
logger.CleanupOldLogs(30);
```

## カスタム通知ハンドラーの実装

独自の通知方法を実装できます。

```csharp
public class CustomNotificationHandler : INotificationHandler
{
    public bool CanHandle(Notification notification)
    {
        // この通知を処理するか判定
        return notification.Level == NotificationLevel.Error;
    }

    public async Task HandleAsync(Notification notification)
    {
        // カスタム通知処理
        // 例: メール送信、Slack通知、データベース記録など
    }
}

// 登録
notificationService.RegisterHandler(new CustomNotificationHandler());
```

## ユーザーフレンドリーなメッセージ

グローバルハンドラーは、例外の種類に応じて適切なメッセージを自動生成します。

| 例外の種類 | ユーザーメッセージ |
|-----------|-------------------|
| InvalidOperationException | 操作を実行できませんでした。 |
| UnauthorizedAccessException | この操作を実行する権限がありません。 |
| TimeoutException | 操作がタイムアウトしました。もう一度お試しください。 |
| ArgumentException | 入力値が正しくありません。 |
| IOException | ファイル操作中にエラーが発生しました。 |
| HttpRequestException | ネットワーク接続に問題があります。 |
| その他 | 予期しないエラーが発生しました。 |

## WPFアプリケーションでの統合

App.xaml.csでグローバルハンドラーを設定します。

```csharp
public partial class App : Application
{
    private GlobalExceptionHandler? _exceptionHandler;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // エラーハンドリングのセットアップ
        var notificationService = new NotificationService();
        notificationService.RegisterHandler(new ToastNotificationHandler(this));

        var logger = new FileExceptionLogger();
        var notifier = new ExceptionNotifier(notificationService);
        _exceptionHandler = new GlobalExceptionHandler(notifier, logger);

        // グローバル例外ハンドラーの登録
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
        DispatcherUnhandledException += OnDispatcherUnhandledException;
        TaskScheduler.UnobservedTaskException += OnUnobservedTaskException;
    }

    private void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception exception)
        {
            _exceptionHandler?.HandleExceptionAsync(exception, "AppDomain").Wait();
        }
    }

    private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        _exceptionHandler?.HandleExceptionAsync(e.Exception, "Dispatcher").Wait();
        e.Handled = true;
    }

    private void OnUnobservedTaskException(object? sender, UnobservedTaskExceptionEventArgs e)
    {
        _exceptionHandler?.HandleExceptionAsync(e.Exception, "Task").Wait();
        e.SetObserved();
    }
}
```

## ベストプラクティス

1. **例外は適切にキャッチする**: 予期される例外は事前にキャッチして処理する
2. **コンテキスト情報を提供**: `HandleExceptionAsync`の第2引数に処理の文脈を渡す
3. **ログを定期的に確認**: 本番環境でのエラー傾向を把握する
4. **カスタムハンドラーで拡張**: プロジェクト固有の通知方法を実装する
5. **ユーザーに適切な情報を提供**: 技術的な詳細ではなく、解決方法を示す
