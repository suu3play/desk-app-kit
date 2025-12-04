using System.Runtime.InteropServices;

namespace DeskAppKit.Infrastructure.Diagnostics;

/// <summary>
/// 環境情報
/// </summary>
public class EnvironmentInfo
{
    public string MachineName { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string OSVersion { get; set; } = string.Empty;
    public string RuntimeVersion { get; set; } = string.Empty;
    public string ProcessorCount { get; set; } = string.Empty;
    public string TotalMemory { get; set; } = string.Empty;
    public string CurrentDirectory { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 現在の環境情報を取得
    /// </summary>
    public static EnvironmentInfo GetCurrent(string appVersion)
    {
        var info = new EnvironmentInfo
        {
            MachineName = Environment.MachineName,
            UserName = Environment.UserName,
            OSVersion = GetOSVersion(),
            RuntimeVersion = RuntimeInformation.FrameworkDescription,
            ProcessorCount = Environment.ProcessorCount.ToString(),
            TotalMemory = GetTotalMemory(),
            CurrentDirectory = Environment.CurrentDirectory,
            AppVersion = appVersion,
            StartTime = DateTime.UtcNow
        };

        return info;
    }

    /// <summary>
    /// OS詳細バージョンを取得
    /// </summary>
    private static string GetOSVersion()
    {
        var osVersion = Environment.OSVersion;
        var arch = RuntimeInformation.ProcessArchitecture;

        return $"{osVersion.Platform} {osVersion.Version} ({arch})";
    }

    /// <summary>
    /// 総メモリ量を取得
    /// </summary>
    private static string GetTotalMemory()
    {
        try
        {
            var gcMemory = GC.GetGCMemoryInfo();
            var totalMemoryGB = gcMemory.TotalAvailableMemoryBytes / (1024.0 * 1024.0 * 1024.0);
            return $"{totalMemoryGB:F2} GB";
        }
        catch
        {
            return "不明";
        }
    }

    /// <summary>
    /// テキスト形式で出力
    /// </summary>
    public override string ToString()
    {
        return $@"
========== 環境情報 ==========
マシン名: {MachineName}
ユーザー名: {UserName}
OS: {OSVersion}
.NET Runtime: {RuntimeVersion}
プロセッサ数: {ProcessorCount}
総メモリ: {TotalMemory}
作業ディレクトリ: {CurrentDirectory}
アプリバージョン: {AppVersion}
起動時刻: {StartTime.ToLocalTime()}
============================
";
    }

    /// <summary>
    /// エラーレポート用のZIPファイルを生成
    /// </summary>
    public static async Task<string> GenerateErrorReportZipAsync(
        string appVersion,
        string logDirectory,
        string outputDirectory,
        Exception? exception = null)
    {
        try
        {
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var zipFileName = $"ErrorReport_{timestamp}.zip";
            var zipFilePath = Path.Combine(outputDirectory, zipFileName);

            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            // 一時ディレクトリを作成
            var tempDir = Path.Combine(Path.GetTempPath(), $"ErrorReport_{timestamp}");
            Directory.CreateDirectory(tempDir);

            try
            {
                // 環境情報をテキストファイルとして保存
                var envInfo = GetCurrent(appVersion);
                var envInfoPath = Path.Combine(tempDir, "environment.txt");
                await File.WriteAllTextAsync(envInfoPath, envInfo.ToString());

                // 例外情報を保存
                if (exception != null)
                {
                    var exceptionPath = Path.Combine(tempDir, "exception.txt");
                    await File.WriteAllTextAsync(exceptionPath, exception.ToString());
                }

                // 最新のログファイルをコピー
                if (Directory.Exists(logDirectory))
                {
                    var logFiles = Directory.GetFiles(logDirectory, "app_*.log")
                        .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                        .Take(3); // 最新3ファイル

                    foreach (var logFile in logFiles)
                    {
                        var fileName = Path.GetFileName(logFile);
                        File.Copy(logFile, Path.Combine(tempDir, fileName));
                    }
                }

                // ZIPファイルを作成
                System.IO.Compression.ZipFile.CreateFromDirectory(tempDir, zipFilePath);

                return zipFilePath;
            }
            finally
            {
                // 一時ディレクトリを削除
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }
        catch
        {
            throw new InvalidOperationException("エラーレポートZIPの生成に失敗しました");
        }
    }
}
