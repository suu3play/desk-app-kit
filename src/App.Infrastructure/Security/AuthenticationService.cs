using DeskAppKit.Core.Enums;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;

namespace DeskAppKit.Infrastructure.Security;

/// <summary>
/// 認証サービス実装
/// </summary>
public class AuthenticationService : IAuthenticationService
{
    private readonly IRepository<User> _userRepository;
    private User? _currentUser;

    public AuthenticationService(IRepository<User> userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User?> LoginAsync(string loginId, string password)
    {
        var users = await _userRepository.FindAsync(u => u.LoginId == loginId);
        var user = users.FirstOrDefault();

        if (user == null)
            return null;

        // アカウントがロック中かチェック
        if (user.AccountStatus == AccountStatus.Locked)
        {
            if (user.LockoutUntil.HasValue && user.LockoutUntil.Value > DateTime.UtcNow)
            {
                throw new InvalidOperationException($"アカウントがロックされています。解除時刻: {user.LockoutUntil.Value.ToLocalTime()}");
            }
            else
            {
                // ロック解除時刻を過ぎている場合は自動解除
                await UnlockAccountAsync(user.UserId);
                user.AccountStatus = AccountStatus.Active;
            }
        }

        // アカウントが無効
        if (user.AccountStatus != AccountStatus.Active)
        {
            throw new InvalidOperationException($"アカウントが無効です。状態: {user.AccountStatus}");
        }

        // パスワード検証
        if (!PasswordHasher.VerifyPassword(password, user.PasswordHash, user.Salt))
        {
            // ログイン失敗カウントを増やす
            user.LockoutCount++;

            if (user.LockoutCount >= 5)
            {
                // 5回失敗でロック
                await LockAccountAsync(user.UserId);
                throw new InvalidOperationException("ログイン失敗が5回に達したため、アカウントをロックしました。");
            }

            await _userRepository.UpdateAsync(user);
            await _userRepository.SaveChangesAsync();

            return null;
        }

        // ログイン成功
        user.LockoutCount = 0;
        user.LastLoginAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();

        _currentUser = user;
        return user;
    }

    public void Logout()
    {
        _currentUser = null;
    }

    public User? GetCurrentUser()
    {
        return _currentUser;
    }

    public (string hash, string salt) HashPassword(string password)
    {
        return PasswordHasher.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hash, string salt)
    {
        return PasswordHasher.VerifyPassword(password, hash, salt);
    }

    public async Task LockAccountAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("ユーザーが見つかりません。");

        user.AccountStatus = AccountStatus.Locked;
        user.LockoutUntil = DateTime.UtcNow.AddMinutes(30); // 30分ロック

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
    }

    public async Task UnlockAccountAsync(Guid userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
            throw new InvalidOperationException("ユーザーが見つかりません。");

        user.AccountStatus = AccountStatus.Active;
        user.LockoutCount = 0;
        user.LockoutUntil = null;

        await _userRepository.UpdateAsync(user);
        await _userRepository.SaveChangesAsync();
    }
}
