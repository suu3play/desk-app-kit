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
    private readonly BootstrapDbManager _bootstrapDbManager;
    private readonly ILogger _logger;

    private StorageMode _selectedStorageMode;
    private string _dbServer = string.Empty;
    private string _dbName = string.Empty;
    private string _dbUsername = string.Empty;
    private string _dbPassword = string.Empty;
    private bool _useIntegratedSecurity;
    private string _statusMessage = string.Empty;
    private bool _isTesting;

    public SettingsViewModel(ISettingsService settingsService, string dataDirectory, string encryptionKey, ILogger logger)
    {
        _settingsService = settingsService;
        _bootstrapDbManager = new BootstrapDbManager(dataDirectory, encryptionKey);
        _logger = logger;

        // 現在の設定を読み込み
        _selectedStorageMode = _settingsService.GetStorageMode();
        LoadDatabaseConfig();

        // コマンド初期化
        TestConnectionCommand = new RelayCommand(async () => await TestConnectionAsync(), () => !IsTesting);
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
    /// DBサーバー名
    /// </summary>
    public string DbServer
    {
        get => _dbServer;
        set => SetProperty(ref _dbServer, value);
    }

    /// <summary>
    /// データベース名
    /// </summary>
    public string DbName
    {
        get => _dbName;
        set => SetProperty(ref _dbName, value);
    }

    /// <summary>
    /// ユーザー名
    /// </summary>
    public string DbUsername
    {
        get => _dbUsername;
        set => SetProperty(ref _dbUsername, value);
    }

    /// <summary>
    /// パスワード
    /// </summary>
    public string DbPassword
    {
        get => _dbPassword;
        set => SetProperty(ref _dbPassword, value);
    }

    /// <summary>
    /// Windows認証を使用
    /// </summary>
    public bool UseIntegratedSecurity
    {
        get => _useIntegratedSecurity;
        set
        {
            if (SetProperty(ref _useIntegratedSecurity, value))
            {
                OnPropertyChanged(nameof(IsUserPasswordEnabled));
            }
        }
    }

    /// <summary>
    /// ユーザー名/パスワード入力が有効か
    /// </summary>
    public bool IsUserPasswordEnabled => !UseIntegratedSecurity;

    /// <summary>
    /// ステータスメッセージ
    /// </summary>
    public string StatusMessage
    {
        get => _statusMessage;
        set => SetProperty(ref _statusMessage, value);
    }

    /// <summary>
    /// テスト中フラグ
    /// </summary>
    public bool IsTesting
    {
        get => _isTesting;
        set => SetProperty(ref _isTesting, value);
    }

    /// <summary>
    /// 接続テストコマンド
    /// </summary>
    public ICommand TestConnectionCommand { get; }

    /// <summary>
    /// 保存コマンド
    /// </summary>
    public ICommand SaveCommand { get; }

    /// <summary>
    /// キャンセルコマンド
    /// </summary>
    public ICommand CancelCommand { get; }

    /// <summary>
    /// DB設定読み込み
    /// </summary>
    private void LoadDatabaseConfig()
    {
        var config = _bootstrapDbManager.Load();
        if (config != null)
        {
            DbServer = config.Server;
            DbName = config.Database;
            DbUsername = config.UserId ?? string.Empty;
            DbPassword = config.Password ?? string.Empty;
            UseIntegratedSecurity = config.IntegratedSecurity;
        }
        else
        {
            // デフォルト値
            DbServer = "localhost";
            DbName = "DeskAppKitDb";
            UseIntegratedSecurity = true;
        }
    }

    /// <summary>
    /// 接続テスト
    /// </summary>
    private async Task TestConnectionAsync()
    {
        try
        {
            IsTesting = true;
            StatusMessage = "接続テスト中...";

            var config = new BootstrapDbConfig
            {
                Server = DbServer,
                Database = DbName,
                UserId = UseIntegratedSecurity ? string.Empty : DbUsername,
                Password = UseIntegratedSecurity ? string.Empty : DbPassword,
                IntegratedSecurity = UseIntegratedSecurity
            };

            var canConnect = await _bootstrapDbManager.TestConnectionAsync(config);

            if (canConnect)
            {
                StatusMessage = "接続成功";
                _logger.Info("SettingsViewModel", "DB接続テスト成功");
            }
            else
            {
                StatusMessage = "接続失敗";
                _logger.Warn("SettingsViewModel", "DB接続テスト失敗");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"エラー: {ex.Message}";
            _logger.Error("SettingsViewModel", "DB接続テスト中にエラー", ex);
        }
        finally
        {
            IsTesting = false;
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

            // DB設定保存
            if (SelectedStorageMode == StorageMode.Database)
            {
                var config = new BootstrapDbConfig
                {
                    Server = DbServer,
                    Database = DbName,
                    UserId = UseIntegratedSecurity ? string.Empty : DbUsername,
                    Password = UseIntegratedSecurity ? string.Empty : DbPassword,
                    IntegratedSecurity = UseIntegratedSecurity
                };

                _bootstrapDbManager.Save(config);
            }

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
