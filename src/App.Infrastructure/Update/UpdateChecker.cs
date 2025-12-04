using System.Text.Json;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;

namespace DeskAppKit.Infrastructure.Update;

/// <summary>
/// アップデートチェッカー
/// </summary>
public class UpdateChecker
{
    private readonly IRepository<SchemaVersion> _schemaVersionRepository;
    private readonly ILogger _logger;
    private readonly string _updateServerUrl;
    private readonly string _currentAppVersion;

    public UpdateChecker(
        IRepository<SchemaVersion> schemaVersionRepository,
        ILogger logger,
        string updateServerUrl,
        string currentAppVersion)
    {
        _schemaVersionRepository = schemaVersionRepository;
        _logger = logger;
        _updateServerUrl = updateServerUrl;
        _currentAppVersion = currentAppVersion;
    }

    /// <summary>
    /// 最新バージョンをチェック
    /// </summary>
    public async Task<VersionInfo?> CheckForUpdatesAsync()
    {
        try
        {
            _logger.Info("UpdateChecker", "アップデートチェック開始");

            // 配布可能な最新バージョンを取得
            var latestVersion = await GetLatestAvailableVersionAsync();

            if (latestVersion == null)
            {
                _logger.Info("UpdateChecker", "配布可能なバージョンが見つかりません");
                return null;
            }

            // 現在のバージョンと比較
            if (latestVersion.IsNewerThan(_currentAppVersion))
            {
                _logger.Info("UpdateChecker", $"新しいバージョンが見つかりました: {latestVersion.Version}");
                return latestVersion;
            }

            _logger.Info("UpdateChecker", "最新バージョンです");
            return null;
        }
        catch (Exception ex)
        {
            _logger.Error("UpdateChecker", "アップデートチェックに失敗しました", ex);
            return null;
        }
    }

    /// <summary>
    /// 配布可能な最新バージョンを取得
    /// </summary>
    private async Task<VersionInfo?> GetLatestAvailableVersionAsync()
    {
        // DBから配布上限バージョンを取得
        var versions = await _schemaVersionRepository.FindAsync(v =>
            v.DevApproved &&
            v.UserApproved &&
            v.IsAvailableForUpgrade);

        var latestSchemaVersion = versions
            .OrderByDescending(v => v.ReleaseDate)
            .FirstOrDefault();

        if (latestSchemaVersion == null)
            return null;

        // アップデートサーバーからバージョン情報を取得
        return await FetchVersionInfoFromServerAsync(latestSchemaVersion.Version);
    }

    /// <summary>
    /// アップデートサーバーからバージョン情報を取得
    /// </summary>
    private async Task<VersionInfo?> FetchVersionInfoFromServerAsync(string version)
    {
        try
        {
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);

            var url = $"{_updateServerUrl}/versions/{version}.json";
            var response = await httpClient.GetAsync(url);

            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<VersionInfo>(json);
        }
        catch (Exception ex)
        {
            _logger.Error("UpdateChecker", $"サーバーからバージョン情報の取得に失敗: {version}", ex);
            return null;
        }
    }

    /// <summary>
    /// アップデート履歴を記録
    /// </summary>
    public async Task RecordUpdateCheckAsync(Guid clientVersionStatusId, bool hasUpdate, string? errorMessage = null)
    {
        // ClientVersionStatusを更新
        // 実装は省略（リポジトリパターンで実装）
        await Task.CompletedTask;
    }
}
