using System.Security.Cryptography;
using DeskAppKit.Core.Interfaces;

namespace DeskAppKit.Infrastructure.Update;

/// <summary>
/// パッケージダウンローダー
/// </summary>
public class PackageDownloader
{
    private readonly ILogger _logger;
    private readonly string _downloadDirectory;

    public PackageDownloader(ILogger logger, string downloadDirectory)
    {
        _logger = logger;
        _downloadDirectory = downloadDirectory;

        if (!Directory.Exists(_downloadDirectory))
            Directory.CreateDirectory(_downloadDirectory);
    }

    /// <summary>
    /// パッケージをダウンロード
    /// </summary>
    public async Task<string?> DownloadAsync(VersionInfo versionInfo, IProgress<int>? progress = null)
    {
        try
        {
            _logger.Info("PackageDownloader", $"ダウンロード開始: {versionInfo.Version}");

            var fileName = $"Setup_{versionInfo.Version}.exe";
            var filePath = Path.Combine(_downloadDirectory, fileName);

            // 既にダウンロード済みでハッシュが一致する場合はスキップ
            if (File.Exists(filePath))
            {
                var existingHash = await CalculateFileHashAsync(filePath);
                if (existingHash == versionInfo.Hash)
                {
                    _logger.Info("PackageDownloader", "ダウンロード済み（ハッシュ一致）");
                    return filePath;
                }

                // ハッシュが一致しない場合は削除
                File.Delete(filePath);
            }

            // ダウンロード実行
            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(30);

            using var response = await httpClient.GetAsync(versionInfo.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            var totalBytes = response.Content.Headers.ContentLength ?? versionInfo.FileSize;
            var downloadedBytes = 0L;

            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);

            var buffer = new byte[8192];
            int bytesRead;

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
            {
                await fileStream.WriteAsync(buffer, 0, bytesRead);
                downloadedBytes += bytesRead;

                if (progress != null && totalBytes > 0)
                {
                    var progressPercentage = (int)((downloadedBytes * 100) / totalBytes);
                    progress.Report(progressPercentage);
                }
            }

            _logger.Info("PackageDownloader", $"ダウンロード完了: {filePath}");

            // ハッシュ検証
            var hash = await CalculateFileHashAsync(filePath);
            if (hash != versionInfo.Hash)
            {
                File.Delete(filePath);
                throw new InvalidOperationException("ハッシュ検証に失敗しました。ファイルが破損している可能性があります。");
            }

            _logger.Info("PackageDownloader", "ハッシュ検証成功");
            return filePath;
        }
        catch (Exception ex)
        {
            _logger.Error("PackageDownloader", "ダウンロードに失敗しました", ex);
            return null;
        }
    }

    /// <summary>
    /// ファイルのSHA256ハッシュを計算
    /// </summary>
    private async Task<string> CalculateFileHashAsync(string filePath)
    {
        using var sha256 = SHA256.Create();
        using var stream = File.OpenRead(filePath);
        var hashBytes = await sha256.ComputeHashAsync(stream);
        return BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
    }

    /// <summary>
    /// ダウンロードディレクトリをクリーンアップ
    /// </summary>
    public void CleanupOldDownloads(int retentionDays = 7)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-retentionDays);
            var files = Directory.GetFiles(_downloadDirectory, "Setup_*.exe");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTime < cutoffDate)
                {
                    File.Delete(file);
                    _logger.Info("PackageDownloader", $"古いダウンロードファイルを削除: {file}");
                }
            }
        }
        catch (Exception ex)
        {
            _logger.Error("PackageDownloader", "クリーンアップに失敗しました", ex);
        }
    }
}
