using System.Diagnostics;
using System.Text.RegularExpressions;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Infrastructure.Settings.Database;
using Microsoft.EntityFrameworkCore;

namespace DeskAppKit.Infrastructure.Database;

/// <summary>
/// LocalDBセットアップ結果
/// </summary>
public class LocalDbSetupResult
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public string? ConnectionString { get; set; }
    public List<string> Steps { get; set; } = new();
}

/// <summary>
/// SQL Server Express LocalDB自動セットアップサービス
/// </summary>
public class LocalDbSetupService
{
    private readonly ILogger? _logger;
    private readonly string _instanceName;
    private readonly string _databaseName;

    public LocalDbSetupService(ILogger? logger = null, string instanceName = "DeskAppKit", string databaseName = "DeskAppKitDb")
    {
        _logger = logger;
        _instanceName = instanceName;
        _databaseName = databaseName;
    }

    /// <summary>
    /// LocalDBが利用可能かチェック
    /// </summary>
    public async Task<bool> IsLocalDbAvailableAsync()
    {
        try
        {
            var result = await ExecuteCommandAsync("sqllocaldb", "info");
            return result.Success;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// LocalDBインスタンスが存在するかチェック
    /// </summary>
    public async Task<bool> InstanceExistsAsync()
    {
        try
        {
            var result = await ExecuteCommandAsync("sqllocaldb", "info");
            if (!result.Success) return false;

            return result.Output.Contains(_instanceName, StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// LocalDB環境を自動セットアップ
    /// </summary>
    /// <param name="dataDirectory">データディレクトリ</param>
    /// <param name="encryptionKey">暗号化キー</param>
    /// <param name="seedSampleData">サンプルデータを投入するか</param>
    public async Task<LocalDbSetupResult> SetupAsync(string dataDirectory, string encryptionKey, bool seedSampleData = true)
    {
        var result = new LocalDbSetupResult();

        try
        {
            // 1. LocalDB利用可能性チェック
            _logger?.Info("LocalDbSetupService", "LocalDB利用可能性チェック");
            result.Steps.Add("LocalDB利用可能性チェック");

            if (!await IsLocalDbAvailableAsync())
            {
                result.Success = false;
                result.Message = "SQL Server Express LocalDBがインストールされていません。\n" +
                               "Visual Studio 2022または以下からインストールしてください：\n" +
                               "https://www.microsoft.com/sql-server/sql-server-downloads";
                return result;
            }

            result.Steps.Add("✓ LocalDB利用可能");

            // 2. インスタンスの存在確認
            _logger?.Info("LocalDbSetupService", "インスタンス存在確認");
            result.Steps.Add("インスタンス存在確認");

            bool instanceExists = await InstanceExistsAsync();

            // 3. インスタンスが存在しない場合は作成
            if (!instanceExists)
            {
                _logger?.Info("LocalDbSetupService", $"インスタンス '{_instanceName}' を作成");
                result.Steps.Add($"インスタンス '{_instanceName}' を作成中");

                var createResult = await ExecuteCommandAsync("sqllocaldb", $"create \"{_instanceName}\"");
                if (!createResult.Success)
                {
                    result.Success = false;
                    result.Message = $"インスタンス作成に失敗しました：{createResult.Output}";
                    return result;
                }

                result.Steps.Add($"✓ インスタンス '{_instanceName}' を作成");
            }
            else
            {
                result.Steps.Add($"✓ インスタンス '{_instanceName}' は既に存在");
            }

            // 4. インスタンス起動
            _logger?.Info("LocalDbSetupService", "インスタンス起動");
            result.Steps.Add("インスタンス起動中");

            var startResult = await ExecuteCommandAsync("sqllocaldb", $"start \"{_instanceName}\"");
            // 既に起動している場合もSuccessとして扱う
            if (!startResult.Success && !startResult.Output.Contains("already running", StringComparison.OrdinalIgnoreCase))
            {
                result.Success = false;
                result.Message = $"インスタンス起動に失敗しました：{startResult.Output}";
                return result;
            }

            result.Steps.Add("✓ インスタンス起動完了");

            // 5. 接続文字列生成
            var connectionString = $"Server=(localdb)\\{_instanceName};Database={_databaseName};Integrated Security=True;TrustServerCertificate=True;";
            result.ConnectionString = connectionString;

            // 6. bootstrap_db.json保存
            _logger?.Info("LocalDbSetupService", "bootstrap_db.json保存");
            result.Steps.Add("接続設定を保存中");

            var bootstrapDbManager = new BootstrapDbManager(dataDirectory, encryptionKey);
            var config = new BootstrapDbConfig
            {
                Server = $"(localdb)\\{_instanceName}",
                Database = _databaseName,
                IntegratedSecurity = true,
                Port = 1433
            };

            bootstrapDbManager.Save(config);
            result.Steps.Add("✓ bootstrap_db.jsonを保存");

            // 7. データベース作成（EF Core Migrationsを使用）
            _logger?.Info("LocalDbSetupService", "データベース初期化");
            result.Steps.Add("データベース初期化中");

            await InitializeDatabaseAsync(connectionString);

            result.Steps.Add("✓ データベース初期化完了");

            // 7.5. サンプルデータ投入
            if (seedSampleData)
            {
                _logger?.Info("LocalDbSetupService", "サンプルデータ投入");
                result.Steps.Add("サンプルデータ投入中");

                var (userCount, notificationCount) = await SeedSampleDataAsync(connectionString);

                result.Steps.Add($"✓ サンプルユーザー {userCount}件、通知 {notificationCount}件を投入");
            }

            // 8. 接続テスト（データベース作成直後なので少し待機）
            _logger?.Info("LocalDbSetupService", "接続テスト");
            result.Steps.Add("接続テスト中");

            // データベース初期化直後は接続できない場合があるため、少し待機
            await Task.Delay(1000);

            bool connectionSuccess = await bootstrapDbManager.TestConnectionAsync(config);
            if (!connectionSuccess)
            {
                _logger?.Warn("LocalDbSetupService", "接続テスト失敗（データベースは作成済み）");
                // データベースは作成されているので、接続テスト失敗でも成功扱いにする
                result.Steps.Add("⚠ 接続テスト失敗（データベースは作成済み）");
            }
            else
            {
                result.Steps.Add("✓ 接続テスト成功");
            }

            result.Success = true;
            result.Message = "LocalDB環境のセットアップが完了しました。";
            _logger?.Info("LocalDbSetupService", "セットアップ完了");

            return result;
        }
        catch (Exception ex)
        {
            _logger?.Error("LocalDbSetupService", "セットアップエラー", ex);
            result.Success = false;
            result.Message = $"セットアップ中にエラーが発生しました：{ex.Message}";
            return result;
        }
    }

    /// <summary>
    /// データベース初期化（EF Core Migrations適用）
    /// </summary>
    private async Task InitializeDatabaseAsync(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<Persistence.DbContexts.AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var dbContext = new Persistence.DbContexts.AppDbContext(optionsBuilder.Options);

        // Migrationsを適用（データベースが存在しない場合は作成）
        await dbContext.Database.MigrateAsync();
    }

    /// <summary>
    /// サンプルデータを投入
    /// </summary>
    private async Task<(int userCount, int notificationCount)> SeedSampleDataAsync(string connectionString)
    {
        var optionsBuilder = new DbContextOptionsBuilder<Persistence.DbContexts.AppDbContext>();
        optionsBuilder.UseSqlServer(connectionString);

        using var dbContext = new Persistence.DbContexts.AppDbContext(optionsBuilder.Options);

        var seeder = new DatabaseSeeder(_logger);
        return await seeder.SeedAsync(dbContext);
    }

    /// <summary>
    /// コマンド実行ヘルパー
    /// </summary>
    private async Task<(bool Success, string Output)> ExecuteCommandAsync(string fileName, string arguments)
    {
        try
        {
            using var process = new Process();
            process.StartInfo = new ProcessStartInfo
            {
                FileName = fileName,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            process.Start();

            var output = await process.StandardOutput.ReadToEndAsync();
            var error = await process.StandardError.ReadToEndAsync();

            await process.WaitForExitAsync();

            var fullOutput = string.IsNullOrEmpty(error) ? output : $"{output}\n{error}";

            return (process.ExitCode == 0, fullOutput);
        }
        catch (Exception ex)
        {
            return (false, ex.Message);
        }
    }
}
