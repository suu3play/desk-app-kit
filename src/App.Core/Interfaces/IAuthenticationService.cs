using DeskAppKit.Core.Models;

namespace DeskAppKit.Core.Interfaces;

/// <summary>
/// 認証サービスインターフェイス
/// </summary>
public interface IAuthenticationService
{
    /// <summary>
    /// ログイン
    /// </summary>
    Task<User?> LoginAsync(string loginId, string password);

    /// <summary>
    /// ログアウト
    /// </summary>
    void Logout();

    /// <summary>
    /// 現在ログイン中のユーザーを取得
    /// </summary>
    User? GetCurrentUser();

    /// <summary>
    /// パスワードハッシュを生成
    /// </summary>
    (string hash, string salt) HashPassword(string password);

    /// <summary>
    /// パスワードを検証
    /// </summary>
    bool VerifyPassword(string password, string hash, string salt);

    /// <summary>
    /// アカウントをロック
    /// </summary>
    Task LockAccountAsync(Guid userId);

    /// <summary>
    /// アカウントロックを解除
    /// </summary>
    Task UnlockAccountAsync(Guid userId);
}
