using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using DeskAppKit.Core.Exceptions;

namespace DeskAppKit.Infrastructure.Logging;

/// <summary>
/// ファイルベースの例外ロガー
/// </summary>
public class FileExceptionLogger : IExceptionLogger
{
    private readonly string _logDirectory;
    private readonly string _logFileName;
    private static readonly object _lockObject = new();

    public FileExceptionLogger(string? logDirectory = null)
    {
        _logDirectory = logDirectory ?? Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
            "DeskAppKit",
            "Logs");

        _logFileName = $"exceptions_{DateTime.Now:yyyyMMdd}.log";

        // ログディレクトリを作成
        Directory.CreateDirectory(_logDirectory);
    }

    public Task LogExceptionAsync(Exception exception, string context)
    {
        return Task.Run(() =>
        {
            var logEntry = BuildLogEntry(exception, context);
            var logPath = Path.Combine(_logDirectory, _logFileName);

            lock (_lockObject)
            {
                File.AppendAllText(logPath, logEntry, Encoding.UTF8);
            }
        });
    }

    private string BuildLogEntry(Exception exception, string context)
    {
        var sb = new StringBuilder();

        sb.AppendLine("============================================");
        sb.AppendLine($"Timestamp: {DateTime.Now:yyyy-MM-dd HH:mm:ss.fff}");
        sb.AppendLine($"Context: {context}");
        sb.AppendLine($"Exception Type: {exception.GetType().FullName}");
        sb.AppendLine($"Message: {exception.Message}");

        if (exception.InnerException != null)
        {
            sb.AppendLine($"Inner Exception: {exception.InnerException.GetType().FullName}");
            sb.AppendLine($"Inner Message: {exception.InnerException.Message}");
        }

        sb.AppendLine("Stack Trace:");
        sb.AppendLine(exception.StackTrace);

        if (exception.Data.Count > 0)
        {
            sb.AppendLine("Additional Data:");
            foreach (var key in exception.Data.Keys)
            {
                sb.AppendLine($"  {key}: {exception.Data[key]}");
            }
        }

        sb.AppendLine("============================================");
        sb.AppendLine();

        return sb.ToString();
    }

    /// <summary>
    /// 古いログファイルをクリーンアップします
    /// </summary>
    public void CleanupOldLogs(int daysToKeep = 30)
    {
        var files = Directory.GetFiles(_logDirectory, "exceptions_*.log");

        foreach (var file in files)
        {
            var fileInfo = new FileInfo(file);
            if ((DateTime.Now - fileInfo.LastWriteTime).Days > daysToKeep)
            {
                File.Delete(file);
            }
        }
    }
}
