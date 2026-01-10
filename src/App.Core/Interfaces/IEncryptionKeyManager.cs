using DeskAppKit.Core.Models;

namespace DeskAppKit.Core.Interfaces;

/// <summary>
/// 暗号化キー管理インターフェース
/// </summary>
public interface IEncryptionKeyManager
{
    /// <summary>
    /// 現在のキーIDを取得
    /// </summary>
    string GetCurrentKeyId();

    /// <summary>
    /// キーを取得
    /// </summary>
    byte[] GetKey(string keyId);

    /// <summary>
    /// キーを追加
    /// </summary>
    void AddKey(string keyId, byte[] key);

    /// <summary>
    /// キーをローテーション
    /// </summary>
    void RotateKey(string newKeyId, byte[] newKey);

    /// <summary>
    /// キー履歴を取得
    /// </summary>
    IEnumerable<KeyInfo> GetKeyHistory();

    /// <summary>
    /// キーが存在するか確認
    /// </summary>
    bool HasKey(string keyId);
}
