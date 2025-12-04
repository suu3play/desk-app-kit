using System.Text;
using System.Text.Json;
using DeskAppKit.Core.Enums;
using DeskAppKit.Core.Interfaces;

namespace DeskAppKit.Infrastructure.Logging;

/// <summary>
/// ファイルロガー（全角スペース区切り形式）
/// </summary>
public class FileLogger : ILogger
{
    private readonly string _logDirectory;
    private readonly string _machineName;
    private readonly string _appVersion;
    private readonly object _lockObject = new object();
    private const string Delimiter = "　"; // 全角スペース

    public FileLogger(string logDirectory, string appVersion)
    {
        _logDirectory = logDirectory;
        _machineName = Environment.MachineName;
        _appVersion = appVersion;

        if (!Directory.Exists(_logDirectory))
            Directory.CreateDirectory(_logDirectory);
    }

    public void Log(LogLevel level, string logger, string message, Exception? exception = null, Guid? userId = null, Dictionary<string, object>? context = null)
    {
        var timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
        var levelText = level.ToString().ToUpper();
        var exceptionText = exception != null ? NormalizeField(exception.ToString()) : "";
        var userIdText = userId?.ToString() ?? "";
        var contextText = context != null ? NormalizeField(JsonSerializer.Serialize(context)) : "";

        var logLine = $"{timestamp}{Delimiter}{levelText}{Delimiter}{NormalizeField(logger)}{Delimiter}{NormalizeField(message)}{Delimiter}{exceptionText}{Delimiter}{userIdText}{Delimiter}{NormalizeField(_machineName)}{Delimiter}{NormalizeField(_appVersion)}{Delimiter}{contextText}";
        var logFilePath = GetLogFilePath();

        lock (_lockObject)
        {
            // ファイルが新規作成される場合はヘッダーを追加
            bool isNewFile = !File.Exists(logFilePath);

            using (var writer = new StreamWriter(logFilePath, append: true, Encoding.UTF8))
            {
                if (isNewFile)
                {
                    writer.WriteLine($"Timestamp{Delimiter}Level{Delimiter}Logger{Delimiter}Message{Delimiter}Exception{Delimiter}UserId{Delimiter}MachineName{Delimiter}AppVersion{Delimiter}Context");
                }
                writer.WriteLine(logLine);
            }
        }
    }

    /// <summary>
    /// フィールドの正規化処理（改行を空白に置換）
    /// </summary>
    private static string NormalizeField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return "";

        // 改行文字を半角スペースに置換
        return field.Replace("\r\n", " ").Replace("\n", " ").Replace("\r", " ");
    }

    public void Debug(string logger, string message, Dictionary<string, object>? context = null)
    {
        Log(LogLevel.Debug, logger, message, null, null, context);
    }

    public void Info(string logger, string message, Dictionary<string, object>? context = null)
    {
        Log(LogLevel.Info, logger, message, null, null, context);
    }

    public void Warn(string logger, string message, Dictionary<string, object>? context = null)
    {
        Log(LogLevel.Warn, logger, message, null, null, context);
    }

    public void Error(string logger, string message, Exception? exception = null, Dictionary<string, object>? context = null)
    {
        Log(LogLevel.Error, logger, message, exception, null, context);
    }

    private string GetLogFilePath()
    {
        var fileName = $"app_{DateTime.Now:yyyyMMdd}.log";
        return Path.Combine(_logDirectory, fileName);
    }

    /// <summary>
    /// 古いログファイルを削除
    /// </summary>
    public void CleanupOldLogs(int retentionDays)
    {
        try
        {
            var cutoffDate = DateTime.Now.AddDays(-retentionDays);
            var files = Directory.GetFiles(_logDirectory, "app_*.log");

            foreach (var file in files)
            {
                var fileInfo = new FileInfo(file);
                if (fileInfo.LastWriteTime < cutoffDate)
                {
                    File.Delete(file);
                }
            }
        }
        catch (Exception ex)
        {
            Error("FileLogger", "ログクリーンアップに失敗しました", ex);
        }
    }
}
