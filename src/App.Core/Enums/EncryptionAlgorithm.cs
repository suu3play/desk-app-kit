namespace DeskAppKit.Core.Enums;

/// <summary>
/// 暗号化アルゴリズム
/// </summary>
public enum EncryptionAlgorithm
{
    /// <summary>
    /// AES-256 (CBC mode)
    /// </summary>
    AES256,

    /// <summary>
    /// AES-256-GCM (認証付き暗号化)
    /// </summary>
    AES256GCM,

    /// <summary>
    /// ChaCha20-Poly1305 (認証付き暗号化)
    /// </summary>
    ChaCha20Poly1305
}
