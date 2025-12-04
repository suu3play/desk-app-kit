using DeskAppKit.Core.Enums;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;

namespace DeskAppKit.Infrastructure.Update;

/// <summary>
/// アップデートサービス（統合）
/// </summary>
public class UpdateService
{
    private readonly UpdateChecker _updateChecker;
    private readonly PackageDownloader _packageDownloader;
    private readonly InstallerRunner _installerRunner;
    private readonly IRepository<ClientVersionStatus> _clientVersionRepository;
    private readonly ILogger _logger;
    private readonly string _machineName;
    private readonly Guid? _currentUserId;

    public UpdateService(
        UpdateChecker updateChecker,
        PackageDownloader packageDownloader,
        InstallerRunner installerRunner,
        IRepository<ClientVersionStatus> clientVersionRepository,
        ILogger logger,
        Guid? currentUserId = null)
    {
        _updateChecker = updateChecker;
        _packageDownloader = packageDownloader;
        _installerRunner = installerRunner;
        _clientVersionRepository = clientVersionRepository;
        _logger = logger;
        _machineName = Environment.MachineName;
        _currentUserId = currentUserId;
    }

    /// <summary>
    /// アップデートチェックと実行（完全な流れ）
    /// </summary>
    public async Task<UpdateResult> CheckAndInstallUpdateAsync(IProgress<int>? progress = null, bool autoInstall = false)
    {
        try
        {
            _logger.Info("UpdateService", "アップデートプロセス開始");

            // 1. アップデートチェック
            var versionInfo = await _updateChecker.CheckForUpdatesAsync();

            if (versionInfo == null)
            {
                await RecordUpdateResultAsync(UpdateResult.NoUpdate);
                return UpdateResult.NoUpdate;
            }

            _logger.Info("UpdateService", $"新バージョン検出: {versionInfo.Version}");

            // 2. ダウンロード
            var installerPath = await _packageDownloader.DownloadAsync(versionInfo, progress);

            if (installerPath == null)
            {
                await RecordUpdateResultAsync(UpdateResult.Failed, "ダウンロードに失敗しました");
                return UpdateResult.Failed;
            }

            // 3. 検証
            if (!_installerRunner.ValidateBeforeInstall(installerPath))
            {
                await RecordUpdateResultAsync(UpdateResult.Failed, "インストーラーの検証に失敗しました");
                return UpdateResult.Failed;
            }

            // 4. インストール
            if (autoInstall)
            {
                _logger.Info("UpdateService", "自動インストールを開始します");
                _installerRunner.RunInstallerAndExit(installerPath);
                // ここには到達しない（アプリケーション終了）
            }

            await RecordUpdateResultAsync(UpdateResult.Success);
            return UpdateResult.Success;
        }
        catch (Exception ex)
        {
            _logger.Error("UpdateService", "アップデートプロセスでエラーが発生しました", ex);
            await RecordUpdateResultAsync(UpdateResult.Failed, ex.Message);
            return UpdateResult.Failed;
        }
    }

    /// <summary>
    /// アップデートチェックのみ実行
    /// </summary>
    public async Task<VersionInfo?> CheckForUpdatesOnlyAsync()
    {
        return await _updateChecker.CheckForUpdatesAsync();
    }

    /// <summary>
    /// ダウンロードのみ実行
    /// </summary>
    public async Task<string?> DownloadUpdateAsync(VersionInfo versionInfo, IProgress<int>? progress = null)
    {
        return await _packageDownloader.DownloadAsync(versionInfo, progress);
    }

    /// <summary>
    /// インストール実行（アプリケーション終了）
    /// </summary>
    public void InstallUpdate(string installerPath)
    {
        if (_installerRunner.ValidateBeforeInstall(installerPath))
        {
            _installerRunner.RunInstallerAndExit(installerPath);
        }
        else
        {
            throw new InvalidOperationException("インストーラーの検証に失敗しました");
        }
    }

    /// <summary>
    /// アップデート結果を記録
    /// </summary>
    private async Task RecordUpdateResultAsync(UpdateResult result, string? errorMessage = null)
    {
        try
        {
            var statuses = await _clientVersionRepository.FindAsync(s =>
                s.MachineName == _machineName &&
                s.UserId == _currentUserId);

            var status = statuses.FirstOrDefault();

            if (status != null)
            {
                status.LastUpdateCheckAt = DateTime.UtcNow;
                status.LastUpdateResult = result;
                status.LastUpdateErrorMessage = errorMessage;

                await _clientVersionRepository.UpdateAsync(status);
                await _clientVersionRepository.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.Error("UpdateService", "アップデート結果の記録に失敗しました", ex);
        }
    }

    /// <summary>
    /// 自動アップデートが有効かチェック
    /// </summary>
    public async Task<bool> IsAutoUpdateEnabledAsync()
    {
        var statuses = await _clientVersionRepository.FindAsync(s =>
            s.MachineName == _machineName &&
            s.UserId == _currentUserId);

        var status = statuses.FirstOrDefault();
        return status?.AutoUpdateEnabled ?? false;
    }

    /// <summary>
    /// 自動アップデート設定を変更
    /// </summary>
    public async Task SetAutoUpdateEnabledAsync(bool enabled)
    {
        var statuses = await _clientVersionRepository.FindAsync(s =>
            s.MachineName == _machineName &&
            s.UserId == _currentUserId);

        var status = statuses.FirstOrDefault();

        if (status != null)
        {
            status.AutoUpdateEnabled = enabled;
            await _clientVersionRepository.UpdateAsync(status);
            await _clientVersionRepository.SaveChangesAsync();

            _logger.Info("UpdateService", $"自動アップデート設定を変更: {enabled}");
        }
    }
}
