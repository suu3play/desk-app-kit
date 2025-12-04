using DeskAppKit.Core.Enums;

namespace DeskAppKit.Core.Models;

/// <summary>
/// ログ情報
/// </summary>
public class Log
{
    public Guid LogId { get; set; }
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Logger { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? Exception { get; set; }
    public Guid? UserId { get; set; }
    public string MachineName { get; set; } = string.Empty;
    public string AppVersion { get; set; } = string.Empty;
    public string? ContextJson { get; set; }
}
