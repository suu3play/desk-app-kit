using DeskAppKit.Core.Enums;
using DeskAppKit.Infrastructure.Database;
using DeskAppKit.Infrastructure.Persistence.DbContexts;
using Microsoft.EntityFrameworkCore;

namespace DeskAppKit.IntegrationTests;

/// <summary>
/// DatabaseSeederのテスト
/// </summary>
public class DatabaseSeederTests : IDisposable
{
    private readonly AppDbContext _dbContext;
    private readonly DatabaseSeeder _seeder;

    public DatabaseSeederTests()
    {
        // インメモリデータベースを使用
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _dbContext = new AppDbContext(options);
        _seeder = new DatabaseSeeder();
    }

    [Fact]
    public async Task SeedAsync_ShouldInsertSampleUsers()
    {
        // Act
        var (userCount, _) = await _seeder.SeedAsync(_dbContext);

        // Assert
        Assert.Equal(2, userCount);
        var users = await _dbContext.Users.ToListAsync();
        Assert.Equal(2, users.Count);

        // Adminユーザーの検証
        var adminUser = users.FirstOrDefault(u => u.LoginId == "admin");
        Assert.NotNull(adminUser);
        Assert.Equal("管理者", adminUser.DisplayName);
        Assert.Equal(UserRole.Admin, adminUser.Role);
        Assert.Equal(AccountStatus.Active, adminUser.AccountStatus);
        Assert.NotEmpty(adminUser.PasswordHash);
        Assert.NotEmpty(adminUser.Salt);

        // 一般ユーザーの検証
        var normalUser = users.FirstOrDefault(u => u.LoginId == "user");
        Assert.NotNull(normalUser);
        Assert.Equal("一般ユーザー", normalUser.DisplayName);
        Assert.Equal(UserRole.User, normalUser.Role);
        Assert.Equal(AccountStatus.Active, normalUser.AccountStatus);
        Assert.NotEmpty(normalUser.PasswordHash);
        Assert.NotEmpty(normalUser.Salt);
    }

    [Fact]
    public async Task SeedAsync_ShouldInsertSampleNotifications()
    {
        // Act
        var (_, notificationCount) = await _seeder.SeedAsync(_dbContext);

        // Assert
        Assert.Equal(12, notificationCount); // Info(3) + Success(3) + Warning(3) + Error(3)
        var notifications = await _dbContext.Notifications.ToListAsync();
        Assert.Equal(12, notifications.Count);

        // 各レベルの通知数を検証
        Assert.Equal(3, notifications.Count(n => n.Level == NotificationLevel.Info));
        Assert.Equal(3, notifications.Count(n => n.Level == NotificationLevel.Success));
        Assert.Equal(3, notifications.Count(n => n.Level == NotificationLevel.Warning));
        Assert.Equal(3, notifications.Count(n => n.Level == NotificationLevel.Error));

        // 通知の内容を一つだけ詳細検証
        var infoNotification = notifications.FirstOrDefault(n => n.Title == "システム情報");
        Assert.NotNull(infoNotification);
        Assert.Equal(NotificationLevel.Info, infoNotification.Level);
        Assert.NotEmpty(infoNotification.Message);
    }

    [Fact]
    public async Task SeedAsync_ShouldNotInsertDuplicateUsers()
    {
        // 初回実行
        await _seeder.SeedAsync(_dbContext);

        // 2回目実行（べき等性確認）
        var (userCount, _) = await _seeder.SeedAsync(_dbContext);

        // Assert - 2回目は投入しない
        Assert.Equal(0, userCount);
        var users = await _dbContext.Users.ToListAsync();
        Assert.Equal(2, users.Count); // 初回の2件のみ
    }

    [Fact]
    public async Task SeedAsync_ShouldNotInsertDuplicateNotifications()
    {
        // 初回実行
        await _seeder.SeedAsync(_dbContext);

        // 2回目実行（べき等性確認）
        var (_, notificationCount) = await _seeder.SeedAsync(_dbContext);

        // Assert - 2回目は投入しない
        Assert.Equal(0, notificationCount);
        var notifications = await _dbContext.Notifications.ToListAsync();
        Assert.Equal(12, notifications.Count); // 初回の12件のみ
    }

    public void Dispose()
    {
        _dbContext.Dispose();
    }
}
