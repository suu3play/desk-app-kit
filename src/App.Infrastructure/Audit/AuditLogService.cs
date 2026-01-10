using System.Text.Json;
using DeskAppKit.Core.Enums;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;

namespace DeskAppKit.Infrastructure.Audit;

/// <summary>
/// 監査ログサービス実装
/// </summary>
public class AuditLogService : IAuditLogService
{
    private readonly IAuditLogStorage _storage;
    private readonly ILogger? _logger;

    public AuditLogService(IAuditLogStorage storage, ILogger? logger = null)
    {
        _storage = storage ?? throw new ArgumentNullException(nameof(storage));
        _logger = logger;
    }

    public async Task LogAsync(AuditLog log)
    {
        try
        {
            await _storage.SaveAsync(log).ConfigureAwait(false);
            _logger?.Debug("AuditLogService", $"監査ログ記録: {log.Action} - {log.EntityType}");
        }
        catch (Exception ex)
        {
            _logger?.Error("AuditLogService", "監査ログ記録エラー", ex);
            // 監査ログの記録失敗は元の処理を中断しない
        }
    }

    public async Task LogCreateAsync<T>(T entity, Guid userId, string? reason = null)
    {
        if (entity == null) return;

        var log = new AuditLog
        {
            UserId = userId,
            Action = AuditAction.Create,
            EntityType = typeof(T).Name,
            EntityId = GetEntityId(entity),
            NewValue = SerializeEntity(entity),
            Reason = reason
        };

        await LogAsync(log).ConfigureAwait(false);
    }

    public async Task LogUpdateAsync<T>(T oldEntity, T newEntity, Guid userId, string? reason = null)
    {
        if (oldEntity == null || newEntity == null) return;

        var log = new AuditLog
        {
            UserId = userId,
            Action = AuditAction.Update,
            EntityType = typeof(T).Name,
            EntityId = GetEntityId(newEntity),
            OldValue = SerializeEntity(oldEntity),
            NewValue = SerializeEntity(newEntity),
            Reason = reason
        };

        await LogAsync(log).ConfigureAwait(false);
    }

    public async Task LogDeleteAsync<T>(T entity, Guid userId, string? reason = null)
    {
        if (entity == null) return;

        var log = new AuditLog
        {
            UserId = userId,
            Action = AuditAction.Delete,
            EntityType = typeof(T).Name,
            EntityId = GetEntityId(entity),
            OldValue = SerializeEntity(entity),
            Reason = reason
        };

        await LogAsync(log).ConfigureAwait(false);
    }

    public async Task LogLoginAsync(Guid userId, string userName, bool isSuccess, string? errorMessage = null)
    {
        var log = new AuditLog
        {
            UserId = userId,
            UserName = userName,
            Action = AuditAction.Login,
            EntityType = "Authentication",
            IsSuccess = isSuccess,
            ErrorMessage = errorMessage
        };

        await LogAsync(log).ConfigureAwait(false);
    }

    public async Task LogLogoutAsync(Guid userId, string userName)
    {
        var log = new AuditLog
        {
            UserId = userId,
            UserName = userName,
            Action = AuditAction.Logout,
            EntityType = "Authentication"
        };

        await LogAsync(log).ConfigureAwait(false);
    }

    public async Task<IEnumerable<AuditLog>> GetLogsAsync(AuditLogFilter? filter = null)
    {
        var allLogs = await _storage.GetAllAsync().ConfigureAwait(false);
        var query = allLogs.AsEnumerable();

        if (filter != null)
        {
            if (filter.UserId.HasValue)
            {
                query = query.Where(l => l.UserId == filter.UserId.Value);
            }

            if (filter.Action.HasValue)
            {
                query = query.Where(l => l.Action == filter.Action.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.EntityType))
            {
                query = query.Where(l => l.EntityType.Equals(filter.EntityType, StringComparison.OrdinalIgnoreCase));
            }

            if (filter.From.HasValue)
            {
                query = query.Where(l => l.Timestamp >= filter.From.Value);
            }

            if (filter.To.HasValue)
            {
                query = query.Where(l => l.Timestamp <= filter.To.Value);
            }

            if (!string.IsNullOrWhiteSpace(filter.SearchText))
            {
                var searchText = filter.SearchText.ToLower();
                query = query.Where(l =>
                    (l.UserName != null && l.UserName.ToLower().Contains(searchText)) ||
                    (l.EntityType != null && l.EntityType.ToLower().Contains(searchText)) ||
                    (l.Reason != null && l.Reason.ToLower().Contains(searchText)));
            }

            if (filter.IsSuccess.HasValue)
            {
                query = query.Where(l => l.IsSuccess == filter.IsSuccess.Value);
            }
        }

        return query.OrderByDescending(l => l.Timestamp);
    }

    public async Task<IEnumerable<AuditLog>> GetEntityHistoryAsync(string entityType, string entityId)
    {
        var allLogs = await _storage.GetAllAsync().ConfigureAwait(false);
        return allLogs
            .Where(l => l.EntityType.Equals(entityType, StringComparison.OrdinalIgnoreCase) &&
                       l.EntityId == entityId)
            .OrderByDescending(l => l.Timestamp);
    }

    private string? GetEntityId<T>(T entity)
    {
        if (entity == null) return null;

        var idProperty = typeof(T).GetProperty("Id") ?? typeof(T).GetProperty(typeof(T).Name + "Id");
        if (idProperty == null) return null;

        var idValue = idProperty.GetValue(entity);
        return idValue?.ToString();
    }

    private string? SerializeEntity<T>(T entity)
    {
        if (entity == null) return null;

        try
        {
            var options = new JsonSerializerOptions
            {
                WriteIndented = false,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };
            return JsonSerializer.Serialize(entity, options);
        }
        catch (Exception ex)
        {
            _logger?.Error("AuditLogService", "エンティティシリアライズエラー", ex);
            return null;
        }
    }
}
