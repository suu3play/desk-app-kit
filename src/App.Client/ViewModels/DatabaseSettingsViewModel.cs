using System.Windows.Input;
using DeskAppKit.Core.Enums;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Infrastructure.Database;
using DeskAppKit.Infrastructure.Settings.Database;

namespace DeskAppKit.Client.ViewModels;

/// <summary>
/// データベース設定ViewModel
/// </summary>
public class DatabaseSettingsViewModel : ViewModelBase
{
    private readonly BootstrapDbManager _bootstrapDbManager;
    private readonly ILogger _logger;
    private readonly string _dataDirectory;
    private readonly string _encryptionKey;
    private readonly StorageMode _currentStorageMode;

    private string _dbServer = string.Empty;
    private string _dbName = string.Empty;
    private string _dbUsername = string.Empty;
    private string _dbPassword = string.Empty;
    private bool _useIntegratedSecurity;
    private string _statusMessage = string.Empty;
    private bool _isTesting;
    private bool _isSettingUp;
    private string _connectionStatus = "未接続";
    private bool _includeSampleData = true;

    public DatabaseSettingsViewModel(
        string dataDirectory,
        string encryptionKey,
        StorageMode currentStorageMode,
        ILogger logger)
    {
        _dataDirectory = dataDirectory;
        _encryptionKey = encryptionKey;
        _currentStorageMode = currentStorageMode;
        _logger = logger;
        _bootstrapDbManager = new BootstrapDbManager(dataDirectory, encryptionKey);

        // 現在の設定を読み込み
        LoadDatabaseConfig();

        // コマンド初期化
        SetupLocalDbCommand = new RelayCommand(async () => await SetupLocalDbAsync(), () => !IsSettingUp);
        TestConnectionCommand = new RelayCommand(async () => await TestConnectionAsync(), () => !IsTesting);
        SaveCommand = new RelayCommand(Save);
    }

    #region Properties

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
        set
        {
            SetProperty(ref _isTesting, value);
            (TestConnectionCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// セットアップ中フラグ
    /// </summary>
    public bool IsSettingUp
    {
        get => _isSettingUp;
        set
        {
            SetProperty(ref _isSettingUp, value);
            (SetupLocalDbCommand as RelayCommand)?.RaiseCanExecuteChanged();
        }
    }

    /// <summary>
    /// 接続状態
    /// </summary>
    public string ConnectionStatus
    {
        get => _connectionStatus;
        set => SetProperty(ref _connectionStatus, value);
    }

    /// <summary>
    /// サンプルデータを含める
    /// </summary>
    public bool IncludeSampleData
    {
        get => _includeSampleData;
        set => SetProperty(ref _includeSampleData, value);
    }

    #endregion

    #region Commands

    /// <summary>
    /// LocalDBセットアップコマンド
    /// </summary>
    public ICommand SetupLocalDbCommand { get; }

    /// <summary>
    /// 接続テストコマンド
    /// </summary>
    public ICommand TestConnectionCommand { get; }

    /// <summary>
    /// 保存コマンド
    /// </summary>
    public ICommand SaveCommand { get; }

    #endregion

    #region Private Methods

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
            UpdateConnectionStatus(true);
        }
        else
        {
            // デフォルト値
            DbServer = "localhost";
            DbName = "DeskAppKitDb";
            UseIntegratedSecurity = true;
            UpdateConnectionStatus(false);
        }
    }

    /// <summary>
    /// LocalDBセットアップ
    /// </summary>
    private async Task SetupLocalDbAsync()
    {
        try
        {
            IsSettingUp = true;
            StatusMessage = "LocalDBセットアップを開始しています...";

            var setupService = new LocalDbSetupService(_logger);

            // LocalDB利用可能性チェック
            var isAvailable = await setupService.IsLocalDbAvailableAsync();
            if (!isAvailable)
            {
                StatusMessage = "LocalDBがインストールされていません";
                System.Windows.MessageBox.Show(
                    "SQL Server Express LocalDBがインストールされていません。\n\n" +
                    "Visual Studio 2022に同梱されているか、以下からダウンロードできます：\n" +
                    "https://www.microsoft.com/sql-server/sql-server-downloads",
                    "LocalDB未インストール",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }

            // セットアップ実行
            var result = await setupService.SetupAsync(_dataDirectory, _encryptionKey);

            if (result.Success)
            {
                StatusMessage = "LocalDBセットアップ完了";
                _logger.Info("DatabaseSettingsViewModel", "LocalDBセットアップ完了");

                // 設定を再読み込み
                LoadDatabaseConfig();

                // 成功メッセージ表示
                var stepsMessage = string.Join("\n", result.Steps);
                System.Windows.MessageBox.Show(
                    $"LocalDB環境のセットアップが完了しました。\n\n実行ステップ:\n{stepsMessage}\n\n" +
                    "アプリケーションを再起動してDatabaseモードで使用できます。",
                    "セットアップ完了",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = $"セットアップ失敗: {result.Message}";
                _logger.Error("DatabaseSettingsViewModel", $"LocalDBセットアップ失敗: {result.Message}");

                System.Windows.MessageBox.Show(
                    $"セットアップに失敗しました。\n\n{result.Message}",
                    "セットアップ失敗",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Error);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"エラー: {ex.Message}";
            _logger.Error("DatabaseSettingsViewModel", "LocalDBセットアップ中にエラー", ex);

            System.Windows.MessageBox.Show(
                $"セットアップ中にエラーが発生しました。\n\n{ex.Message}",
                "エラー",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsSettingUp = false;
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
                UpdateConnectionStatus(true);
                _logger.Info("DatabaseSettingsViewModel", "DB接続テスト成功");

                System.Windows.MessageBox.Show(
                    "データベースへの接続に成功しました。",
                    "接続成功",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Information);
            }
            else
            {
                StatusMessage = "接続失敗";
                UpdateConnectionStatus(false);
                _logger.Warn("DatabaseSettingsViewModel", "DB接続テスト失敗");

                System.Windows.MessageBox.Show(
                    "データベースへの接続に失敗しました。\n設定を確認してください。",
                    "接続失敗",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"エラー: {ex.Message}";
            UpdateConnectionStatus(false);
            _logger.Error("DatabaseSettingsViewModel", "DB接続テスト中にエラー", ex);

            System.Windows.MessageBox.Show(
                $"接続テスト中にエラーが発生しました。\n\n{ex.Message}",
                "エラー",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
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
            var config = new BootstrapDbConfig
            {
                Server = DbServer,
                Database = DbName,
                UserId = UseIntegratedSecurity ? string.Empty : DbUsername,
                Password = UseIntegratedSecurity ? string.Empty : DbPassword,
                IntegratedSecurity = UseIntegratedSecurity
            };

            _bootstrapDbManager.Save(config);
            StatusMessage = "設定を保存しました";
            _logger.Info("DatabaseSettingsViewModel", "DB設定保存完了");

            System.Windows.MessageBox.Show(
                "データベース設定を保存しました。\nアプリケーションを再起動して変更を反映してください。",
                "保存完了",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            StatusMessage = $"保存エラー: {ex.Message}";
            _logger.Error("DatabaseSettingsViewModel", "DB設定保存中にエラー", ex);

            System.Windows.MessageBox.Show(
                $"設定の保存中にエラーが発生しました。\n\n{ex.Message}",
                "エラー",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }

    /// <summary>
    /// 接続状態更新
    /// </summary>
    private void UpdateConnectionStatus(bool isConnected)
    {
        ConnectionStatus = isConnected ? "✓ 接続済み" : "✗ 未接続";
    }

    #endregion
}
