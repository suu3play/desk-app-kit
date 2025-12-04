using System.Text.Json;
using DeskAppKit.Infrastructure.Settings.Encryption;

namespace DeskAppKit.Infrastructure.Settings.Database;

/// <summary>
/// DB接続情報（bootstrap_db.json）
/// </summary>
public class BootstrapDbConfig
{
    public string Server { get; set; } = string.Empty;
    public string Database { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public bool IntegratedSecurity { get; set; }
    public int Port { get; set; } = 1433;

    /// <summary>
    /// 接続文字列を取得
    /// </summary>
    public string GetConnectionString()
    {
        if (IntegratedSecurity)
        {
            return $"Server={Server},{Port};Database={Database};Integrated Security=True;TrustServerCertificate=True;";
        }
        else
        {
            return $"Server={Server},{Port};Database={Database};User Id={UserId};Password={Password};TrustServerCertificate=True;";
        }
    }
}

/// <summary>
/// bootstrap_db.json管理クラス
/// </summary>
public class BootstrapDbManager
{
    private readonly string _filePath;
    private readonly string _encryptionKey;

    public BootstrapDbManager(string baseDirectory, string encryptionKey)
    {
        _filePath = Path.Combine(baseDirectory, "bootstrap_db.json");
        _encryptionKey = encryptionKey;
    }

    /// <summary>
    /// DB接続情報を読み込み
    /// </summary>
    public BootstrapDbConfig? Load()
    {
        try
        {
            if (!File.Exists(_filePath))
                return null;

            var encryptedJson = File.ReadAllText(_filePath);
            var json = EncryptionHelper.Decrypt(encryptedJson, _encryptionKey);

            return JsonSerializer.Deserialize<BootstrapDbConfig>(json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("bootstrap_db.jsonの読み込みに失敗しました。", ex);
        }
    }

    /// <summary>
    /// DB接続情報を保存
    /// </summary>
    public void Save(BootstrapDbConfig config)
    {
        try
        {
            var directory = Path.GetDirectoryName(_filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(config, options);
            var encryptedJson = EncryptionHelper.Encrypt(json, _encryptionKey);

            File.WriteAllText(_filePath, encryptedJson);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("bootstrap_db.jsonの保存に失敗しました。", ex);
        }
    }

    /// <summary>
    /// 接続テスト
    /// </summary>
    public async Task<bool> TestConnectionAsync(BootstrapDbConfig config)
    {
        try
        {
            using var connection = new Microsoft.Data.SqlClient.SqlConnection(config.GetConnectionString());
            await connection.OpenAsync();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
