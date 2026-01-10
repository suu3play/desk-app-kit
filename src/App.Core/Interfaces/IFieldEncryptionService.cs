namespace DeskAppKit.Core.Interfaces;

/// <summary>
/// フィールド暗号化サービスインターフェース
/// </summary>
public interface IFieldEncryptionService
{
    /// <summary>
    /// 文字列を暗号化
    /// </summary>
    string Encrypt(string plainText, string? keyId = null);

    /// <summary>
    /// 文字列を復号化
    /// </summary>
    string Decrypt(string cipherText);

    /// <summary>
    /// エンティティを暗号化
    /// </summary>
    Task<T> EncryptEntityAsync<T>(T entity) where T : class;

    /// <summary>
    /// エンティティを復号化
    /// </summary>
    Task<T> DecryptEntityAsync<T>(T entity) where T : class;

    /// <summary>
    /// キーをローテーション
    /// </summary>
    Task RotateKeyAsync(string oldKeyId, string newKeyId);
}
