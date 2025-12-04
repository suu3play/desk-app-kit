using System.Net.NetworkInformation;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Infrastructure.Settings.Database;

namespace DeskAppKit.Infrastructure.Diagnostics;

/// <summary>
/// ヘルスチェック結果
/// </summary>
public class HealthCheckResult
{
    public string Component { get; set; } = string.Empty;
    public bool IsHealthy { get; set; }
    public string Message { get; set; } = string.Empty;
    public TimeSpan ResponseTime { get; set; }
}

/// <summary>
/// ヘルスチェッククラス
/// </summary>
public class HealthCheck
{
    private readonly BootstrapDbManager? _dbManager;
    private readonly ILogger _logger;
    private readonly string? _updateServerUrl;

    public HealthCheck(ILogger logger, BootstrapDbManager? dbManager = null, string? updateServerUrl = null)
    {
        _logger = logger;
        _dbManager = dbManager;
        _updateServerUrl = updateServerUrl;
    }

    /// <summary>
    /// すべてのヘルスチェックを実行
    /// </summary>
    public async Task<List<HealthCheckResult>> RunAllChecksAsync()
    {
        var results = new List<HealthCheckResult>
        {
            await CheckDatabaseConnectionAsync(),
            await CheckUpdateServerAsync(),
            CheckDiskSpace(),
            CheckMemory()
        };

        return results;
    }

    /// <summary>
    /// データベース接続チェック
    /// </summary>
    public async Task<HealthCheckResult> CheckDatabaseConnectionAsync()
    {
        var result = new HealthCheckResult
        {
            Component = "Database"
        };

        if (_dbManager == null)
        {
            result.IsHealthy = true;
            result.Message = "ローカルモード（DB未使用）";
            return result;
        }

        var startTime = DateTime.UtcNow;

        try
        {
            var config = _dbManager.Load();

            if (config == null)
            {
                result.IsHealthy = false;
                result.Message = "bootstrap_db.jsonが見つかりません";
                return result;
            }

            var canConnect = await _dbManager.TestConnectionAsync(config);

            result.IsHealthy = canConnect;
            result.Message = canConnect ? "接続成功" : "接続失敗";
            result.ResponseTime = DateTime.UtcNow - startTime;
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Message = $"エラー: {ex.Message}";
            result.ResponseTime = DateTime.UtcNow - startTime;
        }

        return result;
    }

    /// <summary>
    /// アップデートサーバー疎通チェック
    /// </summary>
    public async Task<HealthCheckResult> CheckUpdateServerAsync()
    {
        var result = new HealthCheckResult
        {
            Component = "UpdateServer"
        };

        if (string.IsNullOrEmpty(_updateServerUrl))
        {
            result.IsHealthy = true;
            result.Message = "アップデートサーバー未設定";
            return result;
        }

        var startTime = DateTime.UtcNow;

        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            var response = await httpClient.GetAsync(_updateServerUrl);
            result.IsHealthy = response.IsSuccessStatusCode;
            result.Message = result.IsHealthy ? "疎通成功" : $"HTTPエラー: {response.StatusCode}";
            result.ResponseTime = DateTime.UtcNow - startTime;
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Message = $"エラー: {ex.Message}";
            result.ResponseTime = DateTime.UtcNow - startTime;
        }

        return result;
    }

    /// <summary>
    /// ディスク空き容量チェック
    /// </summary>
    public HealthCheckResult CheckDiskSpace()
    {
        var result = new HealthCheckResult
        {
            Component = "DiskSpace"
        };

        try
        {
            var drive = DriveInfo.GetDrives().FirstOrDefault(d => d.Name == Path.GetPathRoot(Environment.CurrentDirectory));

            if (drive == null)
            {
                result.IsHealthy = false;
                result.Message = "ドライブ情報が取得できません";
                return result;
            }

            var freeSpaceGB = drive.AvailableFreeSpace / (1024.0 * 1024.0 * 1024.0);
            result.IsHealthy = freeSpaceGB >= 1.0; // 1GB以上
            result.Message = $"空き容量: {freeSpaceGB:F2} GB";
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Message = $"エラー: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// メモリチェック
    /// </summary>
    public HealthCheckResult CheckMemory()
    {
        var result = new HealthCheckResult
        {
            Component = "Memory"
        };

        try
        {
            var process = System.Diagnostics.Process.GetCurrentProcess();
            var usedMemoryMB = process.WorkingSet64 / (1024.0 * 1024.0);

            result.IsHealthy = usedMemoryMB < 500; // 500MB未満
            result.Message = $"使用メモリ: {usedMemoryMB:F2} MB";
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Message = $"エラー: {ex.Message}";
        }

        return result;
    }

    /// <summary>
    /// ネットワーク接続チェック
    /// </summary>
    public HealthCheckResult CheckNetworkConnection()
    {
        var result = new HealthCheckResult
        {
            Component = "Network"
        };

        try
        {
            result.IsHealthy = NetworkInterface.GetIsNetworkAvailable();
            result.Message = result.IsHealthy ? "ネットワーク接続あり" : "ネットワーク接続なし";
        }
        catch (Exception ex)
        {
            result.IsHealthy = false;
            result.Message = $"エラー: {ex.Message}";
        }

        return result;
    }
}
