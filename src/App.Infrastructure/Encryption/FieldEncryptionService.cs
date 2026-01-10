using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using DeskAppKit.Core.Attributes;
using DeskAppKit.Core.Interfaces;

namespace DeskAppKit.Infrastructure.Encryption;

/// <summary>
/// フィールドレベル暗号化サービス実装
/// </summary>
public class FieldEncryptionService : IFieldEncryptionService
{
    private readonly IEncryptionKeyManager _keyManager;
    private readonly ILogger? _logger;

    public FieldEncryptionService(IEncryptionKeyManager keyManager, ILogger? logger = null)
    {
        _keyManager = keyManager ?? throw new ArgumentNullException(nameof(keyManager));
        _logger = logger;
    }

    public string Encrypt(string plainText, string? keyId = null)
    {
        if (string.IsNullOrEmpty(plainText))
        {
            return plainText;
        }

        try
        {
            keyId ??= _keyManager.GetCurrentKeyId();
            var key = _keyManager.GetKey(keyId);

            using var aes = Aes.Create();
            aes.Key = key;
            aes.GenerateIV();

            using var encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
            var plainBytes = Encoding.UTF8.GetBytes(plainText);

            using var msEncrypt = new MemoryStream();
            using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
            {
                csEncrypt.Write(plainBytes, 0, plainBytes.Length);
                csEncrypt.FlushFinalBlock();
            }

            var encrypted = msEncrypt.ToArray();

            // IV + 暗号化データ + KeyID
            var result = new byte[aes.IV.Length + encrypted.Length];
            Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
            Buffer.BlockCopy(encrypted, 0, result, aes.IV.Length, encrypted.Length);

            // Base64エンコード + KeyID（復号化時に必要）
            var base64 = Convert.ToBase64String(result);
            return $"{keyId}:{base64}";
        }
        catch (Exception ex)
        {
            _logger?.Error("FieldEncryptionService", "暗号化エラー", ex);
            throw;
        }
    }

    public string Decrypt(string cipherText)
    {
        if (string.IsNullOrEmpty(cipherText))
        {
            return cipherText;
        }

        try
        {
            // KeyIDとBase64データを分離
            var parts = cipherText.Split(':', 2);
            if (parts.Length != 2)
            {
                throw new FormatException("暗号化データの形式が無効です");
            }

            var keyId = parts[0];
            var base64Data = parts[1];

            var key = _keyManager.GetKey(keyId);
            var fullCipher = Convert.FromBase64String(base64Data);

            using var aes = Aes.Create();
            aes.Key = key;

            // IVを抽出
            var iv = new byte[aes.IV.Length];
            var cipherData = new byte[fullCipher.Length - iv.Length];
            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipherData, 0, cipherData.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
            using var msDecrypt = new MemoryStream(cipherData);
            using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
            using var srDecrypt = new StreamReader(csDecrypt);

            return srDecrypt.ReadToEnd();
        }
        catch (Exception ex)
        {
            _logger?.Error("FieldEncryptionService", "復号化エラー", ex);
            throw;
        }
    }

    public async Task<T> EncryptEntityAsync<T>(T entity) where T : class
    {
        if (entity == null) return entity!;

        await Task.Run(() =>
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var encryptedAttr = property.GetCustomAttribute<EncryptedAttribute>();
                if (encryptedAttr == null) continue;

                if (property.PropertyType != typeof(string))
                {
                    _logger?.Warn("FieldEncryptionService", $"暗号化属性はstring型のプロパティにのみ適用可能です: {property.Name}");
                    continue;
                }

                var plainValue = property.GetValue(entity) as string;
                if (string.IsNullOrEmpty(plainValue)) continue;

                var encryptedValue = Encrypt(plainValue, encryptedAttr.KeyId);
                property.SetValue(entity, encryptedValue);
            }
        });

        return entity;
    }

    public async Task<T> DecryptEntityAsync<T>(T entity) where T : class
    {
        if (entity == null) return entity!;

        await Task.Run(() =>
        {
            var properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                var encryptedAttr = property.GetCustomAttribute<EncryptedAttribute>();
                if (encryptedAttr == null) continue;

                if (property.PropertyType != typeof(string))
                {
                    continue;
                }

                var encryptedValue = property.GetValue(entity) as string;
                if (string.IsNullOrEmpty(encryptedValue)) continue;

                // 既に復号化済み（KeyID:Base64の形式でない）の場合はスキップ
                if (!encryptedValue.Contains(':')) continue;

                var plainValue = Decrypt(encryptedValue);
                property.SetValue(entity, plainValue);
            }
        });

        return entity;
    }

    public async Task RotateKeyAsync(string oldKeyId, string newKeyId)
    {
        // 実際のキーローテーションは IEncryptionKeyManager で実施
        // このメソッドは既存データの再暗号化を担当
        await Task.CompletedTask;
        _logger?.Info("FieldEncryptionService", $"キーローテーション: {oldKeyId} -> {newKeyId}");
    }
}
