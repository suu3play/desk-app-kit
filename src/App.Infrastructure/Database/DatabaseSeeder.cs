using DeskAppKit.Core.Enums;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;
using DeskAppKit.Infrastructure.Persistence.DbContexts;
using DeskAppKit.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace DeskAppKit.Infrastructure.Database;

/// <summary>
/// データベースサンプルデータSeeder
/// </summary>
public class DatabaseSeeder
{
    private readonly ILogger? _logger;

    public DatabaseSeeder(ILogger? logger = null)
    {
        _logger = logger;
    }

    /// <summary>
    /// サンプルデータを投入
    /// </summary>
    /// <param name="dbContext">データベースコンテキスト</param>
    /// <returns>投入したデータ数（ユーザー数、通知数）</returns>
    public async Task<(int userCount, int notificationCount)> SeedAsync(AppDbContext dbContext)
    {
        int userCount = 0;
        int notificationCount = 0;

        try
        {
            // ユーザーが存在しない場合のみ投入
            if (!await dbContext.Users.AnyAsync())
            {
                _logger?.Info("DatabaseSeeder", "サンプルユーザーを投入中");
                userCount = await SeedUsersAsync(dbContext);
                _logger?.Info("DatabaseSeeder", $"{userCount}件のサンプルユーザーを投入しました");
            }
            else
            {
                _logger?.Info("DatabaseSeeder", "既存ユーザーが存在するためスキップします");
            }

            // 通知が存在しない場合のみ投入
            if (!await dbContext.Notifications.AnyAsync())
            {
                _logger?.Info("DatabaseSeeder", "サンプル通知を投入中");
                notificationCount = await SeedNotificationsAsync(dbContext);
                _logger?.Info("DatabaseSeeder", $"{notificationCount}件のサンプル通知を投入しました");
            }
            else
            {
                _logger?.Info("DatabaseSeeder", "既存通知が存在するためスキップします");
            }

            return (userCount, notificationCount);
        }
        catch (Exception ex)
        {
            _logger?.Error("DatabaseSeeder", "サンプルデータ投入エラー", ex);
            throw;
        }
    }

    /// <summary>
    /// サンプルユーザーを投入
    /// </summary>
    private async Task<int> SeedUsersAsync(AppDbContext dbContext)
    {
        var now = DateTime.UtcNow;
        var users = new List<User>();

        // 1. Admin権限ユーザー
        var (adminHash, adminSalt) = PasswordHasher.HashPassword("admin123");
        users.Add(new User
        {
            UserId = Guid.NewGuid(),
            LoginId = "admin",
            DisplayName = "管理者",
            PasswordHash = adminHash,
            Salt = adminSalt,
            Role = UserRole.Admin,
            AccountStatus = AccountStatus.Active,
            LockoutCount = 0,
            LockoutUntil = null,
            LastLoginAt = null,
            CreatedAt = now,
            UpdatedAt = now
        });

        // 2. 一般ユーザー
        var (userHash, userSalt) = PasswordHasher.HashPassword("user123");
        users.Add(new User
        {
            UserId = Guid.NewGuid(),
            LoginId = "user",
            DisplayName = "一般ユーザー",
            PasswordHash = userHash,
            Salt = userSalt,
            Role = UserRole.User,
            AccountStatus = AccountStatus.Active,
            LockoutCount = 0,
            LockoutUntil = null,
            LastLoginAt = null,
            CreatedAt = now,
            UpdatedAt = now
        });

        await dbContext.Users.AddRangeAsync(users);
        await dbContext.SaveChangesAsync();

        return users.Count;
    }

    /// <summary>
    /// サンプル通知を投入
    /// </summary>
    private async Task<int> SeedNotificationsAsync(AppDbContext dbContext)
    {
        var now = DateTime.UtcNow;
        var notifications = new List<Notification>();

        // Info通知（3件）
        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            Title = "システム情報",
            Message = "アプリケーションが正常に起動しました。",
            Level = NotificationLevel.Info,
            Timestamp = now.AddMinutes(-30),
            IsRead = false,
            Category = "System"
        });

        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            Title = "データ同期",
            Message = "サーバーとのデータ同期が完了しました。",
            Level = NotificationLevel.Info,
            Timestamp = now.AddMinutes(-25),
            IsRead = false,
            Category = "Sync"
        });

        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            Title = "新機能のお知らせ",
            Message = "バージョン1.2.0で新しい機能が追加されました。詳細はヘルプをご確認ください。",
            Level = NotificationLevel.Info,
            Timestamp = now.AddMinutes(-20),
            IsRead = true,
            Category = "Feature"
        });

        // Success通知（3件）
        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            Title = "バックアップ完了",
            Message = "データベースのバックアップが正常に完了しました。",
            Level = NotificationLevel.Success,
            Timestamp = now.AddMinutes(-15),
            IsRead = false,
            Category = "Backup"
        });

        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            Title = "設定保存成功",
            Message = "アプリケーション設定が正常に保存されました。",
            Level = NotificationLevel.Success,
            Timestamp = now.AddMinutes(-12),
            IsRead = true,
            Category = "Settings"
        });

        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            Title = "データエクスポート完了",
            Message = "レポートデータのエクスポートが完了しました。",
            Level = NotificationLevel.Success,
            Timestamp = now.AddMinutes(-10),
            IsRead = true,
            Category = "Export"
        });

        // Warning通知（3件）
        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            Title = "ディスク容量警告",
            Message = "データベースディスクの使用率が80%を超えています。",
            Level = NotificationLevel.Warning,
            Timestamp = now.AddMinutes(-8),
            IsRead = false,
            Category = "System"
        });

        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            Title = "パスワード有効期限",
            Message = "パスワードの有効期限が7日後に切れます。早めに変更してください。",
            Level = NotificationLevel.Warning,
            Timestamp = now.AddMinutes(-5),
            IsRead = false,
            Category = "Security"
        });

        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            Title = "メンテナンス予告",
            Message = "明日22:00〜24:00にシステムメンテナンスを実施します。",
            Level = NotificationLevel.Warning,
            Timestamp = now.AddMinutes(-3),
            IsRead = true,
            Category = "Maintenance"
        });

        // Error通知（3件）
        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            Title = "データ同期エラー",
            Message = "サーバーとのデータ同期中にエラーが発生しました。ネットワーク接続を確認してください。",
            Level = NotificationLevel.Error,
            Timestamp = now.AddMinutes(-2),
            IsRead = false,
            Category = "Sync"
        });

        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            Title = "バックアップ失敗",
            Message = "自動バックアップ処理が失敗しました。ディスク容量を確認してください。",
            Level = NotificationLevel.Error,
            Timestamp = now.AddMinutes(-1),
            IsRead = false,
            Category = "Backup"
        });

        notifications.Add(new Notification
        {
            Id = Guid.NewGuid(),
            Title = "認証エラー",
            Message = "外部APIへの認証に失敗しました。認証情報を再設定してください。",
            Level = NotificationLevel.Error,
            Timestamp = now,
            IsRead = false,
            Category = "Authentication"
        });

        await dbContext.Notifications.AddRangeAsync(notifications);
        await dbContext.SaveChangesAsync();

        return notifications.Count;
    }
}
