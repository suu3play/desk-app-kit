using System.Text.Json;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;

namespace DeskAppKit.Infrastructure.Audit;

/// <summary>
/// JSON形式の監査ログストレージ
/// </summary>
public class JsonAuditLogStorage : IAuditLogStorage
{
    private readonly string _dataDirectory;
    private readonly ILogger? _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public JsonAuditLogStorage(string dataDirectory, ILogger? logger = null)
    {
        _dataDirectory = dataDirectory;
        _logger = logger;
        Directory.CreateDirectory(_dataDirectory);
    }

    public async Task SaveAsync(AuditLog log)
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            // 日次ローテーション（ファイル名に日付を含める）
            var fileName = $"audit_{DateTime.UtcNow:yyyyMMdd}.json";
            var filePath = Path.Combine(_dataDirectory, fileName);

            var logs = await LoadLogsFromFileAsync(filePath).ConfigureAwait(false);
            logs.Add(log);
            await SaveLogsToFileAsync(filePath, logs).ConfigureAwait(false);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            var allLogs = new List<AuditLog>();
            var files = Directory.GetFiles(_dataDirectory, "audit_*.json");

            foreach (var file in files.OrderByDescending(f => f))
            {
                var logs = await LoadLogsFromFileAsync(file).ConfigureAwait(false);
                allLogs.AddRange(logs);
            }

            return allLogs.OrderByDescending(l => l.Timestamp);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task DeleteOldLogsAsync(DateTime threshold)
    {
        await _lock.WaitAsync().ConfigureAwait(false);
        try
        {
            var files = Directory.GetFiles(_dataDirectory, "audit_*.json");
            var deletedCount = 0;

            foreach (var file in files)
            {
                var logs = await LoadLogsFromFileAsync(file).ConfigureAwait(false);
                var originalCount = logs.Count;
                logs.RemoveAll(l => l.Timestamp < threshold);

                if (logs.Count == 0)
                {
                    // ファイル内のログがすべて古い場合はファイルごと削除
                    File.Delete(file);
                    deletedCount += originalCount;
                }
                else if (logs.Count < originalCount)
                {
                    // 一部のログが古い場合は更新
                    await SaveLogsToFileAsync(file, logs).ConfigureAwait(false);
                    deletedCount += (originalCount - logs.Count);
                }
            }

            _logger?.Info("JsonAuditLogStorage", $"古い監査ログを削除しました: {deletedCount}件");
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<List<AuditLog>> LoadLogsFromFileAsync(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return new List<AuditLog>();
        }

        try
        {
            var json = await File.ReadAllTextAsync(filePath).ConfigureAwait(false);
            var logs = JsonSerializer.Deserialize<List<AuditLog>>(json);
            return logs ?? new List<AuditLog>();
        }
        catch (Exception ex)
        {
            _logger?.Error("JsonAuditLogStorage", "監査ログ読み込みエラー", ex);
            return new List<AuditLog>();
        }
    }

    private async Task SaveLogsToFileAsync(string filePath, List<AuditLog> logs)
    {
        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            var json = JsonSerializer.Serialize(logs, options);
            await File.WriteAllTextAsync(filePath, json).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.Error("JsonAuditLogStorage", "監査ログ保存エラー", ex);
            throw;
        }
    }
}
