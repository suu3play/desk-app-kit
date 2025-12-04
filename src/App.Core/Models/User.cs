using DeskAppKit.Core.Enums;

namespace DeskAppKit.Core.Models;

/// <summary>
/// ユーザー情報
/// </summary>
public class User
{
    public Guid UserId { get; set; }
    public string LoginId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Salt { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    public AccountStatus AccountStatus { get; set; }
    public int LockoutCount { get; set; }
    public DateTime? LockoutUntil { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
