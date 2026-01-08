using System.IO;
using System.Windows;
using DeskAppKit.Client.Services;
using DeskAppKit.Client.ViewModels;
using DeskAppKit.Client.Views;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Infrastructure.Data;
using DeskAppKit.Infrastructure.Diagnostics;
using DeskAppKit.Infrastructure.Logging;
using DeskAppKit.Infrastructure.Persistence.DbContexts;
using DeskAppKit.Infrastructure.Persistence.Repositories;
using DeskAppKit.Infrastructure.Security;
using DeskAppKit.Infrastructure.Settings;
using DeskAppKit.Infrastructure.Settings.Database;
using DeskAppKit.Infrastructure.Themes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace DeskAppKit.Client;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ILogger? _logger;
    private IAuthenticationService? _authenticationService;
    private IThemeService? _themeService;
    private INotificationCenter? _notificationCenter;

    private void Application_Startup(object sender, StartupEventArgs e)
    {
        try
        {
            // 1. 基本ディレクトリ設定
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var dataDirectory = Path.Combine(baseDirectory, "Data");
            var logDirectory = Path.Combine(baseDirectory, "Logs");

            Directory.CreateDirectory(dataDirectory);
            Directory.CreateDirectory(logDirectory);

            // 2. 暗号化キー（本番環境では安全な場所から取得）
            var encryptionKey = "YourSecureEncryptionKey32Chars!!";

            // 3. ロガー初期化
            var appVersion = "1.0.0";
            _logger = new FileLogger(logDirectory, appVersion);
            _logger.Info("App", "アプリケーション起動");

            // 3-1. 通知センター初期化（StorageMode取得後に実施）
            // 一時的にメモリ内ストレージで初期化
            var tempStorage = new Infrastructure.Notifications.JsonNotificationStorage(dataDirectory, _logger);
            _notificationCenter = new Infrastructure.Notifications.NotificationCenter(tempStorage, _logger);
            _logger.Info("App", "通知センターを初期化しました（一時）");

            // 3-2. グローバル例外ハンドラー登録
            var errorLogDirectory = Path.Combine(baseDirectory, "ErrorLogs");
            var exceptionHandler = new GlobalExceptionHandler(_logger, errorLogDirectory, _notificationCenter);
            exceptionHandler.Register();
            _logger.Info("App", "グローバル例外ハンドラーを登録しました");

            // 4. 設定サービス初期化
            _logger.Info("App", "設定サービスを初期化中...");
            var settingsService = new SettingsService(dataDirectory, encryptionKey);
            _logger.Info("App", "設定サービスのLoad()を呼び出し中...");
            settingsService.Load();
            _logger.Info("App", "設定サービスのLoad()完了");

            // 4-2. テーマサービス初期化
            _logger.Info("App", "テーマサービスを初期化中...");
            _themeService = new ThemeService(settingsService);
            _logger.Info("App", "テーマサービスのLoadSavedTheme()を呼び出し中...");
            _themeService.LoadSavedTheme();
            _logger.Info("App", $"テーマサービスを初期化しました（現在のテーマ: {_themeService.CurrentMode}）");

            // 5. DB接続またはローカルモード判定
            var storageMode = settingsService.GetStorageMode();
            _logger.Info("App", $"StorageMode: {storageMode}");

            // 5-1. NotificationCenterをStorageModeに応じて再初期化
            INotificationStorage notificationStorage;
            if (storageMode == Core.Enums.StorageMode.Local)
            {
                notificationStorage = new Infrastructure.Notifications.JsonNotificationStorage(dataDirectory, _logger);
                _logger.Info("App", "通知センター: JSONストレージを使用");
            }
            else
            {
                // Database用は後で初期化（DbContext作成後）
                notificationStorage = tempStorage;
            }
            _notificationCenter = new Infrastructure.Notifications.NotificationCenter(notificationStorage, _logger);

            // 6. 認証サービス初期化
            if (storageMode == Core.Enums.StorageMode.Database)
            {
                try
                {
                    var bootstrapDbManager = new BootstrapDbManager(dataDirectory, encryptionKey);
                    var dbConfig = bootstrapDbManager.Load();
                    if (dbConfig != null)
                    {
                        var connectionString = dbConfig.GetConnectionString();
                        var optionsBuilder = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>();
                        optionsBuilder.UseSqlServer(connectionString);

                        var dbContext = new AppDbContext(optionsBuilder.Options);

                        // DatabaseNotificationStorageを使用してNotificationCenterを再初期化
                        var dbNotificationStorage = new Infrastructure.Notifications.DatabaseNotificationStorage(dbContext, _logger);
                        _notificationCenter = new Infrastructure.Notifications.NotificationCenter(dbNotificationStorage, _logger);
                        _logger.Info("App", "通知センター: Databaseストレージを使用");

                        var userRepository = new Repository<Core.Models.User>(dbContext);
                        _authenticationService = new AuthenticationService(userRepository);
                        _logger.Info("App", "Databaseモードで認証サービスを初期化しました");
                    }
                    else
                    {
                        _logger.Warn("App", "bootstrap_db.jsonが見つかりません。Localモードにフォールバックします");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("App", "Databaseモード初期化エラー。Localモードにフォールバックします", ex);
                }
            }

            // 6-2. ヘルスチェック初期化
            HealthCheck? healthCheck = null;
            if (storageMode == Core.Enums.StorageMode.Database && _authenticationService != null)
            {
                var bootstrapDbManager = new BootstrapDbManager(dataDirectory, encryptionKey);
                var dbConfig = bootstrapDbManager.Load();
                if (dbConfig != null)
                {
                    healthCheck = new HealthCheck(_logger, bootstrapDbManager);
                    _logger.Info("App", "ヘルスチェック機能を初期化しました");
                }
            }

            // 6-3. Localモード時のSQLite初期化
            string? sqliteConnectionString = null;
            if (storageMode == Core.Enums.StorageMode.Local)
            {
                _logger.Info("App", "LocalモードでSQLiteデータベースを初期化中...");
                try
                {
                    var sqliteSetup = new Infrastructure.Database.LocalSqliteSetupService(_logger);
                    var sqliteResult = sqliteSetup.SetupAsync(dataDirectory, seedSampleData: true).GetAwaiter().GetResult();

                    if (sqliteResult.Success)
                    {
                        sqliteConnectionString = sqliteResult.ConnectionString;
                        _logger.Info("App", $"SQLite初期化完了: {sqliteResult.ConnectionString}");

                        // SQLite用の認証サービスを初期化
                        var optionsBuilder = new Microsoft.EntityFrameworkCore.DbContextOptionsBuilder<AppDbContext>();
                        optionsBuilder.UseSqlite(sqliteConnectionString);
                        var dbContext = new AppDbContext(optionsBuilder.Options);

                        // DatabaseNotificationStorageを使用してNotificationCenterを再初期化（SQLite）
                        var sqliteNotificationStorage = new Infrastructure.Notifications.DatabaseNotificationStorage(dbContext, _logger);
                        _notificationCenter = new Infrastructure.Notifications.NotificationCenter(sqliteNotificationStorage, _logger);
                        _logger.Info("App", "通知センター: SQLite Databaseストレージを使用");

                        var userRepository = new Repository<Core.Models.User>(dbContext);
                        _authenticationService = new AuthenticationService(userRepository);
                        _logger.Info("App", "SQLite用認証サービスを初期化しました");
                    }
                    else
                    {
                        _logger.Warn("App", $"SQLite初期化失敗: {sqliteResult.ErrorMessage}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("App", "SQLite初期化エラー", ex);
                }
            }

            // 6-4. データ一覧サービス初期化
            IDataListService? dataListService = null;
            if (storageMode == Core.Enums.StorageMode.Database)
            {
                try
                {
                    var bootstrapDbManager = new BootstrapDbManager(dataDirectory, encryptionKey);
                    var dbConfig = bootstrapDbManager.Load();
                    if (dbConfig != null)
                    {
                        // IConfigurationを作成してDataListServiceに渡す
                        var configBuilder = new ConfigurationBuilder();
                        var connectionString = dbConfig.GetConnectionString();
                        var configDict = new Dictionary<string, string?>
                        {
                            ["ConnectionStrings:DefaultConnection"] = connectionString
                        };
                        configBuilder.AddInMemoryCollection(configDict);
                        var configuration = configBuilder.Build();

                        dataListService = new DataListService(configuration, _logger);
                        _logger.Info("App", "データ一覧サービスを初期化しました");
                    }
                    else
                    {
                        _logger.Warn("App", "bootstrap_db.jsonが見つかりません。データ一覧サービスは利用できません。");
                    }
                }
                catch (Exception ex)
                {
                    _logger.Error("App", "データ一覧サービス初期化エラー", ex);
                }
            }
            else if (storageMode == Core.Enums.StorageMode.Local && !string.IsNullOrEmpty(sqliteConnectionString))
            {
                // LocalモードでSQLite接続が成功した場合、データ一覧サービスを初期化
                try
                {
                    var configBuilder = new ConfigurationBuilder();
                    var configDict = new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = sqliteConnectionString
                    };
                    configBuilder.AddInMemoryCollection(configDict);
                    var configuration = configBuilder.Build();

                    dataListService = new DataListService(configuration, _logger);
                    _logger.Info("App", "SQLite用データ一覧サービスを初期化しました");
                }
                catch (Exception ex)
                {
                    _logger.Error("App", "SQLite用データ一覧サービス初期化エラー", ex);
                }
            }

            // 7. Localモード（SQLite使用）の場合はログイン画面を表示
            if (_authenticationService == null)
            {
                _logger.Info("App", "Localモードで起動します（ログイン機能なし）");

                // Localモード用のローカルユーザーを作成
                var localUser = new Core.Models.User
                {
                    UserId = Guid.NewGuid(),
                    LoginId = "local",
                    DisplayName = "ローカルユーザー",
                    PasswordHash = string.Empty,
                    Salt = string.Empty,
                    Role = Core.Enums.UserRole.Admin,
                    AccountStatus = Core.Enums.AccountStatus.Active,
                    CreatedAt = DateTime.UtcNow
                };

                // 直接メイン画面を表示
                var mainWindow = new MainWindow(localUser, _logger, healthCheck, settingsService, _themeService, _notificationCenter, dataListService, dataDirectory, encryptionKey);
                mainWindow.Show();
                return;
            }

            // 8. Databaseモードの場合はLoginWindow表示
            _logger.Info("App", "Databaseモードで起動します（ログイン機能あり）");
            var loginViewModel = new LoginViewModel(_authenticationService, _logger);
            var loginWindow = new LoginWindow(loginViewModel);

            loginViewModel.LoginSucceeded += (s, user) =>
            {
                _logger.Info("App", $"ログイン成功: {user.DisplayName}");
                var mainWindow = new MainWindow(user, _logger, healthCheck, settingsService, _themeService, _notificationCenter, dataListService, dataDirectory, encryptionKey);
                mainWindow.Show();
            };

            loginWindow.ShowDialog();

            if (loginWindow.DialogResult != true)
            {
                _logger.Info("App", "ログインがキャンセルされました。アプリケーション終了");
                Shutdown();
            }
        }
        catch (Exception ex)
        {
            // ログファイルに詳細を記録（MessageBox表示前に確実に記録）
            var baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            var logDirectory = Path.Combine(baseDirectory, "Logs");
            Directory.CreateDirectory(logDirectory);
            var errorLogPath = Path.Combine(logDirectory, $"startup_error_{DateTime.Now:yyyyMMddHHmmss}.txt");
            try
            {
                var errorDetails = $"""
                    起動時エラー
                    発生日時: {DateTime.Now:yyyy-MM-dd HH:mm:ss}

                    【例外情報】
                    タイプ: {ex.GetType().FullName}
                    メッセージ: {ex.Message}

                    【スタックトレース】
                    {ex.StackTrace}

                    【内部例外】
                    {(ex.InnerException != null ? $"{ex.InnerException.GetType().FullName}: {ex.InnerException.Message}\n{ex.InnerException.StackTrace}" : "なし")}
                    """;
                File.WriteAllText(errorLogPath, errorDetails);
            }
            catch
            {
                // ファイル書き込み失敗は無視
            }

            _logger?.Error("App", "起動時エラー", ex);

            try
            {
                MessageBox.Show(
                    $"アプリケーションの起動に失敗しました。\n\n{ex.Message}\n\n詳細: {errorLogPath}",
                    "起動エラー",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
            catch
            {
                // MessageBox表示失敗は無視
            }

            Shutdown();
        }
    }
}

