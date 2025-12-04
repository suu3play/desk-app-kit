using System.IO;
using System.Windows;
using DeskAppKit.Client.Services;
using DeskAppKit.Client.ViewModels;
using DeskAppKit.Client.Views;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Infrastructure.Diagnostics;
using DeskAppKit.Infrastructure.Logging;
using DeskAppKit.Infrastructure.Persistence.DbContexts;
using DeskAppKit.Infrastructure.Persistence.Repositories;
using DeskAppKit.Infrastructure.Security;
using DeskAppKit.Infrastructure.Settings;
using DeskAppKit.Infrastructure.Settings.Database;
using Microsoft.EntityFrameworkCore;

namespace DeskAppKit.Client;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private ILogger? _logger;
    private IAuthenticationService? _authenticationService;

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

            // 3-2. グローバル例外ハンドラー登録
            var errorLogDirectory = Path.Combine(baseDirectory, "ErrorLogs");
            var exceptionHandler = new GlobalExceptionHandler(_logger, errorLogDirectory);
            exceptionHandler.Register();
            _logger.Info("App", "グローバル例外ハンドラーを登録しました");

            // 4. 設定サービス初期化
            var settingsService = new SettingsService(dataDirectory, encryptionKey);
            settingsService.Load();

            // 5. DB接続またはローカルモード判定
            var storageMode = settingsService.GetStorageMode();
            _logger.Info("App", $"StorageMode: {storageMode}");

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

            // 7. Localモードの場合はログイン画面をスキップ
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
                var mainWindow = new MainWindow(localUser, _logger, healthCheck, settingsService, dataDirectory, encryptionKey);
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
                var mainWindow = new MainWindow(user, _logger, healthCheck, settingsService, dataDirectory, encryptionKey);
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
            _logger?.Error("App", "起動時エラー", ex);
            MessageBox.Show(
                $"アプリケーションの起動に失敗しました。\n\n{ex.Message}",
                "起動エラー",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            Shutdown();
        }
    }
}

