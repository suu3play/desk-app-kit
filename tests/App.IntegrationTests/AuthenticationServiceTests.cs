using DeskAppKit.Core.Models;
using DeskAppKit.Core.Enums;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Infrastructure.Persistence.DbContexts;
using DeskAppKit.Infrastructure.Persistence.Repositories;
using DeskAppKit.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;

namespace App.IntegrationTests;

/// <summary>
/// 認証サービスの統合テスト
/// </summary>
public class AuthenticationServiceTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly IRepository<User> _userRepository;
    private readonly IAuthenticationService _authService;

    public AuthenticationServiceTests()
    {
        // InMemoryデータベースを使用
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _userRepository = new Repository<User>(_context);
        _authService = new AuthenticationService(_userRepository);
    }

    [Fact]
    public async Task Login_正常系_認証成功()
    {
        // Arrange - ユーザーを事前登録
        var loginId = "testuser";
        var password = "Password123!";
        var (hash, salt) = _authService.HashPassword(password);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            LoginId = loginId,
            DisplayName = "テストユーザー",
            PasswordHash = hash,
            Salt = salt,
            Role = UserRole.User,
            AccountStatus = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Act
        var result = await _authService.LoginAsync(loginId, password);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(loginId, result.LoginId);
        Assert.Equal("テストユーザー", result.DisplayName);

        // LastLoginAtが更新されていることを確認
        var updatedUser = await _userRepository.GetByIdAsync(user.UserId);
        Assert.NotNull(updatedUser);
        Assert.NotNull(updatedUser.LastLoginAt);
    }

    [Fact]
    public async Task Login_異常系_存在しないユーザー()
    {
        // Act
        var result = await _authService.LoginAsync("nonexistent", "password");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task Login_異常系_パスワード不一致()
    {
        // Arrange
        var loginId = "wrongpassuser";
        var correctPassword = "CorrectPassword123!";
        var (hash, salt) = _authService.HashPassword(correctPassword);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            LoginId = loginId,
            DisplayName = "パスワードミスユーザー",
            PasswordHash = hash,
            Salt = salt,
            Role = UserRole.User,
            AccountStatus = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Act
        var result = await _authService.LoginAsync(loginId, "WrongPassword123!");

        // Assert
        Assert.Null(result);

        // ログイン失敗カウントが増えていることを確認
        var updatedUser = await _userRepository.GetByIdAsync(user.UserId);
        Assert.NotNull(updatedUser);
        Assert.Equal(1, updatedUser.LockoutCount);
    }

    [Fact]
    public async Task Login_異常系_ロックアウト中のアカウント()
    {
        // Arrange
        var loginId = "lockeduser";
        var password = "Password123!";
        var (hash, salt) = _authService.HashPassword(password);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            LoginId = loginId,
            DisplayName = "ロックユーザー",
            PasswordHash = hash,
            Salt = salt,
            Role = UserRole.User,
            AccountStatus = AccountStatus.Locked,
            LockoutUntil = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.LoginAsync(loginId, password)
        );

        Assert.Contains("ロックされています", exception.Message);
    }

    [Fact]
    public async Task Login_連続失敗でロックアウト()
    {
        // Arrange
        var loginId = "lockouttest";
        var correctPassword = "Password123!";
        var (hash, salt) = _authService.HashPassword(correctPassword);

        var user = new User
        {
            UserId = Guid.NewGuid(),
            LoginId = loginId,
            DisplayName = "ロックアウトテスト",
            PasswordHash = hash,
            Salt = salt,
            Role = UserRole.User,
            AccountStatus = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Act - 4回失敗させる
        for (int i = 0; i < 4; i++)
        {
            await _authService.LoginAsync(loginId, "WrongPassword!");
        }

        // 5回目の失敗でロックされる（例外が発生する）
        await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.LoginAsync(loginId, "WrongPassword!")
        );

        // Assert - アカウントがロックされているはず
        var lockedUser = await _userRepository.GetByIdAsync(user.UserId);
        Assert.NotNull(lockedUser);
        Assert.Equal(AccountStatus.Locked, lockedUser.AccountStatus);
        Assert.Equal(5, lockedUser.LockoutCount);

        // 正しいパスワードでもロックされているので失敗する
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _authService.LoginAsync(loginId, correctPassword)
        );
        Assert.Contains("ロック", exception.Message);
    }

    [Fact]
    public void HashPassword_パスワードハッシュ生成()
    {
        // Arrange
        var password = "TestPassword123!";

        // Act
        var (hash, salt) = _authService.HashPassword(password);

        // Assert
        Assert.NotNull(hash);
        Assert.NotNull(salt);
        Assert.NotEmpty(hash);
        Assert.NotEmpty(salt);

        // 同じパスワードで検証できることを確認
        Assert.True(_authService.VerifyPassword(password, hash, salt));

        // 異なるパスワードでは検証失敗することを確認
        Assert.False(_authService.VerifyPassword("WrongPassword!", hash, salt));
    }

    [Fact]
    public async Task LockAccountAsync_アカウントロック()
    {
        // Arrange
        var user = new User
        {
            UserId = Guid.NewGuid(),
            LoginId = "lockme",
            DisplayName = "ロック対象",
            PasswordHash = "hash",
            Salt = "salt",
            Role = UserRole.User,
            AccountStatus = AccountStatus.Active,
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Act
        await _authService.LockAccountAsync(user.UserId);

        // Assert
        var lockedUser = await _userRepository.GetByIdAsync(user.UserId);
        Assert.NotNull(lockedUser);
        Assert.Equal(AccountStatus.Locked, lockedUser.AccountStatus);
        Assert.NotNull(lockedUser.LockoutUntil);
        Assert.True(lockedUser.LockoutUntil > DateTime.UtcNow);
    }

    [Fact]
    public async Task UnlockAccountAsync_アカウントロック解除()
    {
        // Arrange
        var user = new User
        {
            UserId = Guid.NewGuid(),
            LoginId = "unlockme",
            DisplayName = "ロック解除対象",
            PasswordHash = "hash",
            Salt = "salt",
            Role = UserRole.User,
            AccountStatus = AccountStatus.Locked,
            LockoutCount = 5,
            LockoutUntil = DateTime.UtcNow.AddHours(1),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        // Act
        await _authService.UnlockAccountAsync(user.UserId);

        // Assert
        var unlockedUser = await _userRepository.GetByIdAsync(user.UserId);
        Assert.NotNull(unlockedUser);
        Assert.Equal(AccountStatus.Active, unlockedUser.AccountStatus);
        Assert.Equal(0, unlockedUser.LockoutCount);
        Assert.Null(unlockedUser.LockoutUntil);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}
