using System.Collections.Concurrent;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;

namespace DeskAppKit.Infrastructure.Encryption;

/// <summary>
/// 暗号化キー管理実装
/// </summary>
public class EncryptionKeyManager : IEncryptionKeyManager
{
    private readonly ConcurrentDictionary<string, byte[]> _keys = new();
    private readonly ConcurrentDictionary<string, KeyInfo> _keyInfos = new();
    private string _currentKeyId = "default";
    private readonly ILogger? _logger;

    public EncryptionKeyManager(ILogger? logger = null)
    {
        _logger = logger;
    }

    public string GetCurrentKeyId()
    {
        return _currentKeyId;
    }

    public byte[] GetKey(string keyId)
    {
        if (_keys.TryGetValue(keyId, out var key))
        {
            return key;
        }

        _logger?.Error("EncryptionKeyManager", $"キーが見つかりません: {keyId}");
        throw new KeyNotFoundException($"キーが見つかりません: {keyId}");
    }

    public void AddKey(string keyId, byte[] key)
    {
        if (key == null || key.Length == 0)
        {
            throw new ArgumentException("キーが無効です", nameof(key));
        }

        _keys[keyId] = key;
        _keyInfos[keyId] = new KeyInfo
        {
            KeyId = keyId,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        _logger?.Info("EncryptionKeyManager", $"キーを追加しました: {keyId}");
    }

    public void RotateKey(string newKeyId, byte[] newKey)
    {
        // 現在のキーを非アクティブにする
        if (_keyInfos.TryGetValue(_currentKeyId, out var oldKeyInfo))
        {
            oldKeyInfo.IsActive = false;
            oldKeyInfo.ExpiredAt = DateTime.UtcNow;
        }

        // 新しいキーを追加
        AddKey(newKeyId, newKey);
        _currentKeyId = newKeyId;

        _logger?.Info("EncryptionKeyManager", $"キーをローテーションしました: {_currentKeyId} -> {newKeyId}");
    }

    public IEnumerable<KeyInfo> GetKeyHistory()
    {
        return _keyInfos.Values.OrderByDescending(k => k.CreatedAt);
    }

    public bool HasKey(string keyId)
    {
        return _keys.ContainsKey(keyId);
    }

    /// <summary>
    /// デフォルトキーを設定
    /// </summary>
    public void SetDefaultKey(string keyId, byte[] key)
    {
        AddKey(keyId, key);
        _currentKeyId = keyId;
    }
}
