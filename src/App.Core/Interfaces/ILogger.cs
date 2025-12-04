using DeskAppKit.Core.Enums;

namespace DeskAppKit.Core.Interfaces;

/// <summary>
/// ロガーインターフェイス
/// </summary>
public interface ILogger
{
    void Log(LogLevel level, string logger, string message, Exception? exception = null, Guid? userId = null, Dictionary<string, object>? context = null);
    void Debug(string logger, string message, Dictionary<string, object>? context = null);
    void Info(string logger, string message, Dictionary<string, object>? context = null);
    void Warn(string logger, string message, Dictionary<string, object>? context = null);
    void Error(string logger, string message, Exception? exception = null, Dictionary<string, object>? context = null);
}
