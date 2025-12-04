using BCrypt.Net;

namespace DeskAppKit.Infrastructure.Security;

/// <summary>
/// パスワードハッシュ化ヘルパー
/// </summary>
public static class PasswordHasher
{
    /// <summary>
    /// パスワードをハッシュ化（Salt付き）
    /// </summary>
    public static (string hash, string salt) HashPassword(string password)
    {
        var salt = BCrypt.Net.BCrypt.GenerateSalt(12);
        var hash = BCrypt.Net.BCrypt.HashPassword(password, salt);
        return (hash, salt);
    }

    /// <summary>
    /// パスワードを検証
    /// </summary>
    public static bool VerifyPassword(string password, string hash, string salt)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch
        {
            return false;
        }
    }
}
