using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Infrastructure.Persistence.DbContexts;
using DeskAppKit.Infrastructure.Data;

namespace DeskAppKit.Infrastructure.Database;

/// <summary>
/// LocalモードでのSQLite初期化サービス
/// </summary>
public class LocalSqliteSetupService
{
    private readonly ILogger? _logger;

    public LocalSqliteSetupService(ILogger? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// SQLiteデータベースをセットアップ
    /// </summary>
    public async Task<LocalSqliteSetupResult> SetupAsync(string dataDirectory, bool seedSampleData = true)
    {
        var result = new LocalSqliteSetupResult
        {
            Success = false,
            ConnectionString = string.Empty,
            Steps = new List<string>()
        };

        try
        {
            _logger?.Info("LocalSqliteSetupService", "SQLite初期化開始");

            // 1. データディレクトリ作成
            if (!Directory.Exists(dataDirectory))
            {
                Directory.CreateDirectory(dataDirectory);
                result.Steps.Add("✓ データディレクトリ作成");
            }

            // 2. SQLiteファイルパス
            var dbFilePath = Path.Combine(dataDirectory, "local.db");
            var connectionString = $"Data Source={dbFilePath}";
            result.ConnectionString = connectionString;

            _logger?.Info("LocalSqliteSetupService", $"SQLiteファイルパス: {dbFilePath}");
            result.Steps.Add($"✓ SQLiteファイルパス: {dbFilePath}");

            // 3. DbContext作成とスキーマ生成
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            optionsBuilder.UseSqlite(connectionString);

            using (var dbContext = new AppDbContext(optionsBuilder.Options))
            {
                _logger?.Info("LocalSqliteSetupService", "データベーススキーマ作成中");

                // EnsureCreatedを使用してSQLiteに対応したスキーマを自動生成
                var created = await dbContext.Database.EnsureCreatedAsync().ConfigureAwait(false);
                _logger?.Info("LocalSqliteSetupService", $"EnsureCreatedAsync結果: {created}");

                // テーブルが作成されたかを確認
                var tableExists = await dbContext.Database.CanConnectAsync().ConfigureAwait(false);
                _logger?.Info("LocalSqliteSetupService", $"DB接続可能: {tableExists}");

                // 4. サンプルデータ投入（同じDbContextインスタンスで実行）
                if (seedSampleData)
                {
                    _logger?.Info("LocalSqliteSetupService", "サンプルデータ投入を開始");
                    var seeder = new DatabaseSeeder(_logger);
                    var (userCount, notificationCount) = await seeder.SeedAsync(dbContext).ConfigureAwait(false);
                    result.Steps.Add($"✓ サンプルユーザー {userCount}件、通知 {notificationCount}件を投入");
                }

                result.Steps.Add("✓ データベーススキーマ作成完了");
            }

            // 5. 接続テスト
            _logger?.Info("LocalSqliteSetupService", "接続テスト");
            using (var connection = new SqliteConnection(connectionString))
            {
                await connection.OpenAsync().ConfigureAwait(false);
                result.Steps.Add("✓ 接続テスト成功");
            }

            result.Success = true;
            _logger?.Info("LocalSqliteSetupService", "SQLite初期化完了");

            return result;
        }
        catch (Exception ex)
        {
            _logger?.Error("LocalSqliteSetupService", "SQLite初期化エラー", ex);
            result.Steps.Add($"✗ エラー: {ex.Message}");
            result.ErrorMessage = ex.Message;
            return result;
        }
    }

}

/// <summary>
/// SQLiteセットアップ結果
/// </summary>
public class LocalSqliteSetupResult
{
    public bool Success { get; set; }
    public string ConnectionString { get; set; } = string.Empty;
    public List<string> Steps { get; set; } = new();
    public string? ErrorMessage { get; set; }
}
