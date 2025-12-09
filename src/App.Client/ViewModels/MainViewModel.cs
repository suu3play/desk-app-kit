using System.Windows.Input;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;
using DeskAppKit.Infrastructure.Diagnostics;
using DeskAppKit.Infrastructure.Settings;

namespace DeskAppKit.Client.ViewModels;

/// <summary>
/// メインウィンドウViewModel
/// </summary>
public class MainViewModel : ViewModelBase
{
    private User _currentUser;
    private string _currentViewTitle = "ホーム";
    private readonly ILogger? _logger;
    private readonly HealthCheck? _healthCheck;
    private readonly ISettingsService? _settingsService;
    private readonly IThemeService? _themeService;
    private readonly INotificationCenter? _notificationCenter;
    private readonly IDataListService? _dataListService;
    private readonly string? _dataDirectory;
    private readonly string? _encryptionKey;
    private int _unreadNotificationCount;

    public MainViewModel(
        User currentUser,
        ILogger? logger = null,
        HealthCheck? healthCheck = null,
        ISettingsService? settingsService = null,
        IThemeService? themeService = null,
        INotificationCenter? notificationCenter = null,
        IDataListService? dataListService = null,
        string? dataDirectory = null,
        string? encryptionKey = null)
    {
        _currentUser = currentUser;
        _logger = logger;
        _healthCheck = healthCheck;
        _settingsService = settingsService;
        _themeService = themeService;
        _notificationCenter = notificationCenter;
        _dataListService = dataListService;
        _dataDirectory = dataDirectory;
        _encryptionKey = encryptionKey;

        // コマンド初期化
        ShowSettingsCommand = new RelayCommand(ShowSettings);
        ShowDiagnosticsCommand = new RelayCommand(ShowDiagnostics);
        ShowAboutCommand = new RelayCommand(ShowAbout);
        ShowNotificationsCommand = new RelayCommand(ShowNotifications);
        ShowDataListCommand = new RelayCommand(ShowDataList);
        LogoutCommand = new RelayCommand(Logout);

        // 通知センターのイベント購読
        if (_notificationCenter != null)
        {
            _notificationCenter.NotificationAdded += async (s, e) => await UpdateUnreadCountAsync();
            _ = UpdateUnreadCountAsync();
        }
    }

    /// <summary>
    /// 現在のユーザー
    /// </summary>
    public User CurrentUser
    {
        get => _currentUser;
        set => SetProperty(ref _currentUser, value);
    }

    /// <summary>
    /// 現在のビュータイトル
    /// </summary>
    public string CurrentViewTitle
    {
        get => _currentViewTitle;
        set => SetProperty(ref _currentViewTitle, value);
    }

    /// <summary>
    /// 表示名（ウェルカムメッセージ用）
    /// </summary>
    public string WelcomeMessage => $"ようこそ、{CurrentUser.DisplayName} さん";

    /// <summary>
    /// 設定画面表示コマンド
    /// </summary>
    public ICommand ShowSettingsCommand { get; }

    /// <summary>
    /// 診断画面表示コマンド
    /// </summary>
    public ICommand ShowDiagnosticsCommand { get; }

    /// <summary>
    /// バージョン情報表示コマンド
    /// </summary>
    public ICommand ShowAboutCommand { get; }

    /// <summary>
    /// 通知センター表示コマンド
    /// </summary>
    public ICommand ShowNotificationsCommand { get; }

    /// <summary>
    /// データ一覧表示コマンド
    /// </summary>
    public ICommand ShowDataListCommand { get; }

    /// <summary>
    /// ログアウトコマンド
    /// </summary>
    public ICommand LogoutCommand { get; }

    /// <summary>
    /// 未読通知件数
    /// </summary>
    public int UnreadNotificationCount
    {
        get => _unreadNotificationCount;
        set
        {
            _unreadNotificationCount = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// ログアウト成功イベント
    /// </summary>
    public event EventHandler? LogoutRequested;

    private void ShowSettings()
    {
        CurrentViewTitle = "設定";

        if (_settingsService == null || _dataDirectory == null || _encryptionKey == null || _logger == null)
        {
            return;
        }

        var viewModel = new SettingsViewModel(_settingsService, _dataDirectory, _encryptionKey, _logger, _themeService);
        var settingsWindow = new Views.SettingsWindow(viewModel);
        settingsWindow.ShowDialog();

        CurrentViewTitle = "ホーム";
    }

    private void ShowDiagnostics()
    {
        CurrentViewTitle = "診断";

        if (_logger == null)
        {
            return;
        }

        var viewModel = new DiagnosticsViewModel(_logger, _healthCheck);
        var diagnosticsWindow = new Views.DiagnosticsWindow(viewModel);
        diagnosticsWindow.ShowDialog();

        CurrentViewTitle = "ホーム";
    }

    private void ShowAbout()
    {
        CurrentViewTitle = "バージョン情報";
        var aboutWindow = new Views.AboutWindow();
        aboutWindow.ShowDialog();
        CurrentViewTitle = "ホーム";
    }

    private void ShowNotifications()
    {
        CurrentViewTitle = "通知センター";

        if (_notificationCenter == null)
        {
            _logger?.Warn("MainViewModel", "通知センターが初期化されていません");
            return;
        }

        var viewModel = new NotificationCenterViewModel(_notificationCenter);
        var notificationWindow = new Views.NotificationCenterWindow(viewModel);
        notificationWindow.ShowDialog();

        CurrentViewTitle = "ホーム";
    }

    private void ShowDataList()
    {
        CurrentViewTitle = "データ一覧";

        if (_dataListService == null)
        {
            _logger?.Warn("MainViewModel", "データ一覧サービスが初期化されていません");
            System.Windows.MessageBox.Show(
                "データ一覧機能を利用するには、Databaseモードで起動し、データベース接続を設定する必要があります。",
                "機能利用不可",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
            CurrentViewTitle = "ホーム";
            return;
        }

        var viewModel = new DataListViewModel(_dataListService, _logger);
        var dataListWindow = new Views.DataListWindow(viewModel);
        dataListWindow.ShowDialog();

        CurrentViewTitle = "ホーム";
    }

    private async Task UpdateUnreadCountAsync()
    {
        if (_notificationCenter != null)
        {
            UnreadNotificationCount = await _notificationCenter.GetUnreadCountAsync();
        }
    }

    private void Logout()
    {
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }
}
