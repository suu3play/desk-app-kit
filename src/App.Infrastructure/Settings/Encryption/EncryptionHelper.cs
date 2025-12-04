using System.Security.Cryptography;
using System.Text;

namespace DeskAppKit.Infrastructure.Settings.Encryption;

/// <summary>
/// AES-256暗号化ヘルパー
/// </summary>
public static class EncryptionHelper
{
    private const int KeySize = 256;
    private const int BlockSize = 128;

    /// <summary>
    /// データを暗号化
    /// </summary>
    public static string Encrypt(string plainText, string key)
    {
        if (string.IsNullOrEmpty(plainText))
            return string.Empty;

        using var aes = Aes.Create();
        aes.KeySize = KeySize;
        aes.BlockSize = BlockSize;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        var keyBytes = DeriveKey(key);
        aes.Key = keyBytes;
        aes.GenerateIV();

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var cipherBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        // IV + 暗号化データをBase64で返す
        var result = new byte[aes.IV.Length + cipherBytes.Length];
        Buffer.BlockCopy(aes.IV, 0, result, 0, aes.IV.Length);
        Buffer.BlockCopy(cipherBytes, 0, result, aes.IV.Length, cipherBytes.Length);

        return Convert.ToBase64String(result);
    }

    /// <summary>
    /// データを復号化
    /// </summary>
    public static string Decrypt(string cipherText, string key)
    {
        if (string.IsNullOrEmpty(cipherText))
            return string.Empty;

        try
        {
            var fullCipher = Convert.FromBase64String(cipherText);

            using var aes = Aes.Create();
            aes.KeySize = KeySize;
            aes.BlockSize = BlockSize;
            aes.Mode = CipherMode.CBC;
            aes.Padding = PaddingMode.PKCS7;

            var keyBytes = DeriveKey(key);
            aes.Key = keyBytes;

            // IVを抽出
            var iv = new byte[aes.BlockSize / 8];
            var cipherBytes = new byte[fullCipher.Length - iv.Length];

            Buffer.BlockCopy(fullCipher, 0, iv, 0, iv.Length);
            Buffer.BlockCopy(fullCipher, iv.Length, cipherBytes, 0, cipherBytes.Length);

            aes.IV = iv;

            using var decryptor = aes.CreateDecryptor();
            var plainBytes = decryptor.TransformFinalBlock(cipherBytes, 0, cipherBytes.Length);

            return Encoding.UTF8.GetString(plainBytes);
        }
        catch (CryptographicException)
        {
            throw new InvalidOperationException("復号化に失敗しました。キーが正しくない可能性があります。");
        }
    }

    /// <summary>
    /// パスワードから256ビットキーを生成
    /// </summary>
    private static byte[] DeriveKey(string password)
    {
        // 固定ソルト（本番環境では環境変数等から取得することを推奨）
        var salt = Encoding.UTF8.GetBytes("DeskAppKit-Salt-2024");

        using var deriveBytes = new Rfc2898DeriveBytes(
            password,
            salt,
            10000, // イテレーション回数
            HashAlgorithmName.SHA256);

        return deriveBytes.GetBytes(KeySize / 8);
    }

    /// <summary>
    /// ランダムなキーを生成
    /// </summary>
    public static string GenerateKey()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }
}
