using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;
using DeskAppKit.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DeskAppKit.Infrastructure.Audit;

/// <summary>
/// データベース形式の監査ログストレージ
/// </summary>
public class DatabaseAuditLogStorage : IAuditLogStorage
{
    private readonly AppDbContext _dbContext;
    private readonly ILogger? _logger;

    public DatabaseAuditLogStorage(AppDbContext dbContext, ILogger? logger = null)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger;
    }

    public async Task SaveAsync(AuditLog log)
    {
        try
        {
            await _dbContext.AuditLogs.AddAsync(log).ConfigureAwait(false);
            await _dbContext.SaveChangesAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.Error("DatabaseAuditLogStorage", "監査ログ保存エラー", ex);
            throw;
        }
    }

    public async Task<IEnumerable<AuditLog>> GetAllAsync()
    {
        try
        {
            return await _dbContext.AuditLogs
                .OrderByDescending(l => l.Timestamp)
                .ToListAsync()
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger?.Error("DatabaseAuditLogStorage", "監査ログ取得エラー", ex);
            return new List<AuditLog>();
        }
    }

    public async Task DeleteOldLogsAsync(DateTime threshold)
    {
        try
        {
            var oldLogs = await _dbContext.AuditLogs
                .Where(l => l.Timestamp < threshold)
                .ToListAsync()
                .ConfigureAwait(false);

            if (oldLogs.Any())
            {
                _dbContext.AuditLogs.RemoveRange(oldLogs);
                await _dbContext.SaveChangesAsync().ConfigureAwait(false);
                _logger?.Info("DatabaseAuditLogStorage", $"古い監査ログを削除しました: {oldLogs.Count}件");
            }
        }
        catch (Exception ex)
        {
            _logger?.Error("DatabaseAuditLogStorage", "古い監査ログ削除エラー", ex);
            throw;
        }
    }
}
