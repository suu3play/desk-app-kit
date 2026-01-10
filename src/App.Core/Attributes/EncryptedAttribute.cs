using DeskAppKit.Core.Enums;

namespace DeskAppKit.Core.Attributes;

/// <summary>
/// フィールドレベル暗号化属性
/// </summary>
[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class EncryptedAttribute : Attribute
{
    /// <summary>
    /// 暗号化キーID
    /// </summary>
    public string? KeyId { get; set; }

    /// <summary>
    /// 暗号化アルゴリズム
    /// </summary>
    public EncryptionAlgorithm Algorithm { get; set; } = EncryptionAlgorithm.AES256;

    /// <summary>
    /// 暗号化必須フラグ
    /// </summary>
    public bool Required { get; set; } = true;
}
