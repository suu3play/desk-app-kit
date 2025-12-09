using System.Windows.Input;
using DeskAppKit.Core.Enums;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Infrastructure.Settings;
using DeskAppKit.Infrastructure.Settings.Database;

namespace DeskAppKit.Client.ViewModels;

/// <summary>
/// 設定画面ViewModel
/// </summary>
public class SettingsViewModel : ViewModelBase
{
    private readonly ISettingsService _settingsService;
    private readonly ILogger _logger;
    private readonly IThemeService? _themeService;

    private StorageMode _selectedStorageMode;
    private string _statusMessage = string.Empty;
    private ThemeMode _selectedThemeMode;

    public SettingsViewModel(ISettingsService settingsService, string dataDirectory, string encryptionKey, ILogger logger, IThemeService? themeService = null)
    {
        _settingsService = settingsService;
        _logger = logger;
        _themeService = themeService;

        // 現在の設定を読み込み
        _selectedStorageMode = _settingsService.GetStorageMode();
        _selectedThemeMode = _themeService?.CurrentMode ?? ThemeMode.Light;

        // データベース設定ViewModelを初期化
        DatabaseSettings = new DatabaseSettingsViewModel(dataDirectory, encryptionKey, _selectedStorageMode, logger);

        // コマンド初期化
        SaveCommand = new RelayCommand(Save);
        CancelCommand = new RelayCommand(() => { });
    }

    /// <summary>
    /// 選択中のStorageMode
    /// </summary>
    public StorageMode SelectedStorageMode
    {
        get => _selectedStorageMode;
        set => SetProperty(ref _selectedStorageMode, value);
    }

    /// <summary>
    /// StorageMode一覧
    /// </summary>
    public StorageMode[] AvailableStorageModes => new[] { StorageMode.Local, StorageMode.Database };

    /// <summary>
    /// 選択中のテーマモード
    /// </summary>
    public ThemeMode SelectedThemeMode
    {
        get => _selectedThemeMode;
        set
        {
            if (SetProperty(ref _selectedThemeMode, value))
            {
                ApplyTheme(value);
            }
        }
    }

    /// <summary>
    /// テーマモード一覧
    /// </summary>
    public ThemeMode[] AvailableThemeModes => new[] { ThemeMode.Light, ThemeMode.Dark, ThemeMode.System };

    /// <summary>
    /// データベース設定ViewModel
    /// </summary>
    public DatabaseSettingsViewModel DatabaseSettings { get; }

    /// <summary>
    /// ステータスメッセージ
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// 保存コマンド
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// キャンセルコマンド
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// テーマ適用
    /// </summary>
    private void ApplyTheme(ThemeMode mode)
    {
        if (_themeService == null)
        {
            return;
        }

        try
        {
            _themeService.ApplyTheme(mode);
            _logger.Info("SettingsViewModel", $"テーマを切り替えました: {mode}");
        }
        catch (Exception ex)
        {
            _logger.Error("SettingsViewModel", "テーマ切り替え中にエラー", ex);
        }
    }

    /// <summary>
    /// 設定保存
    /// </summary>
    private void Save()
    {
        try
        {
            // StorageMode保存
            _settingsService.Set("App", "StorageMode", SelectedStorageMode.ToString());

            // テーマ保存
            _themeService?.SaveCurrentTheme();

            _settingsService.Save();
            StatusMessage = "設定を保存しました";
            _logger.Info("SettingsViewModel", "設定保存完了");
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存エラー: {ex.Message}";
            _logger.Error("SettingsViewModel", "設定保存中にエラー", ex);
        }
    }
}
