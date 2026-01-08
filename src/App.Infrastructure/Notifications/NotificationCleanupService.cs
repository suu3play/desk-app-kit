using DeskAppKit.Core.Interfaces;

namespace DeskAppKit.Infrastructure.Notifications;

/// <summary>
/// 通知自動クリーンアップサービス
/// </summary>
public class NotificationCleanupService : IDisposable
{
    private readonly INotificationCenter _notificationCenter;
    private readonly ILogger? _logger;
    private readonly TimeSpan _cleanupInterval;
    private readonly TimeSpan _maxAge;
    private Timer? _timer;
    private bool _disposed;

    /// <summary>
    /// コンストラクタ
    /// </summary>
    /// <param name="notificationCenter">通知センター</param>
    /// <param name="logger">ロガー</param>
    /// <param name="cleanupInterval">クリーンアップ実行間隔（デフォルト: 1日）</param>
    /// <param name="maxAge">通知保持期間（デフォルト: 30日）</param>
    public NotificationCleanupService(
        INotificationCenter notificationCenter,
        ILogger? logger = null,
        TimeSpan? cleanupInterval = null,
        TimeSpan? maxAge = null)
    {
        _notificationCenter = notificationCenter ?? throw new ArgumentNullException(nameof(notificationCenter));
        _logger = logger;
        _cleanupInterval = cleanupInterval ?? TimeSpan.FromDays(1);
        _maxAge = maxAge ?? TimeSpan.FromDays(30);
    }

    /// <summary>
    /// 自動クリーンアップを開始
    /// </summary>
    public void Start()
    {
        if (_disposed)
        {
            throw new ObjectDisposedException(nameof(NotificationCleanupService));
        }

        _logger?.Info("NotificationCleanupService", $"自動クリーンアップ開始（間隔: {_cleanupInterval.TotalHours}時間、保持期間: {_maxAge.TotalDays}日）");

        _timer = new Timer(
            callback: async _ => await CleanupAsync().ConfigureAwait(false),
            state: null,
            dueTime: _cleanupInterval,
            period: _cleanupInterval);
    }

    /// <summary>
    /// 自動クリーンアップを停止
    /// </summary>
    public void Stop()
    {
        _timer?.Dispose();
        _timer = null;
        _logger?.Info("NotificationCleanupService", "自動クリーンアップ停止");
    }

    /// <summary>
    /// 手動でクリーンアップを実行
    /// </summary>
    public async Task CleanupAsync()
    {
        try
        {
            _logger?.Info("NotificationCleanupService", "古い通知をクリーンアップ中...");
            await _notificationCenter.ClearOldNotificationsAsync(_maxAge).ConfigureAwait(false);
            _logger?.Info("NotificationCleanupService", "クリーンアップ完了");
        }
        catch (Exception ex)
        {
            _logger?.Error("NotificationCleanupService", "クリーンアップエラー", ex);
        }
    }

    /// <summary>
    /// リソースを解放
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            Stop();
            _disposed = true;
        }
    }
}
