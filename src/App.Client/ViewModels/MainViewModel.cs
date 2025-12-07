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
    private readonly string? _dataDirectory;
    private readonly string? _encryptionKey;

    public MainViewModel(
        User currentUser,
        ILogger? logger = null,
        HealthCheck? healthCheck = null,
        ISettingsService? settingsService = null,
        IThemeService? themeService = null,
        string? dataDirectory = null,
        string? encryptionKey = null)
    {
        _currentUser = currentUser;
        _logger = logger;
        _healthCheck = healthCheck;
        _settingsService = settingsService;
        _themeService = themeService;
        _dataDirectory = dataDirectory;
        _encryptionKey = encryptionKey;

        // コマンド初期化
        ShowSettingsCommand = new RelayCommand(ShowSettings);
        ShowDiagnosticsCommand = new RelayCommand(ShowDiagnostics);
        ShowAboutCommand = new RelayCommand(ShowAbout);
        LogoutCommand = new RelayCommand(Logout);
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
    /// ログアウトコマンド
    /// </summary>
    public ICommand LogoutCommand { get; }

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

    private void Logout()
    {
        LogoutRequested?.Invoke(this, EventArgs.Empty);
    }
}
