namespace DeskAppKit.Core.Models;

/// <summary>
/// 暗号化キー情報
/// </summary>
public class KeyInfo
{
    /// <summary>
    /// キーID
    /// </summary>
    public string KeyId { get; set; } = string.Empty;

    /// <summary>
    /// 作成日時
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// 有効期限
    /// </summary>
    public DateTime? ExpiredAt { get; set; }

    /// <summary>
    /// アクティブフラグ
    /// </summary>
    public bool IsActive { get; set; } = true;
}
