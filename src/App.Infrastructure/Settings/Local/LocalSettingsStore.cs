using System.Text.Json;
using DeskAppKit.Infrastructure.Settings.Encryption;

namespace DeskAppKit.Infrastructure.Settings.Local;

/// <summary>
/// ローカルJSON設定ストア
/// </summary>
public class LocalSettingsStore
{
    private readonly string _appSettingsPath;
    private readonly string _userSettingsPath;
    private readonly string _encryptionKey;
    private Dictionary<string, Dictionary<string, string>> _appSettings;
    private Dictionary<string, Dictionary<string, string>> _userSettings;

    public LocalSettingsStore(string baseDirectory, string encryptionKey)
    {
        _appSettingsPath = Path.Combine(baseDirectory, "settings_app.json");
        _userSettingsPath = Path.Combine(baseDirectory, "settings_user.json");
        _encryptionKey = encryptionKey;
        _appSettings = new Dictionary<string, Dictionary<string, string>>();
        _userSettings = new Dictionary<string, Dictionary<string, string>>();
    }

    /// <summary>
    /// 設定を読み込み
    /// </summary>
    public void Load()
    {
        _appSettings = LoadFromFile(_appSettingsPath);
        _userSettings = LoadFromFile(_userSettingsPath);
    }

    /// <summary>
    /// 設定を保存
    /// </summary>
    public void Save()
    {
        SaveToFile(_appSettingsPath, _appSettings);
        SaveToFile(_userSettingsPath, _userSettings);
    }

    /// <summary>
    /// アプリ設定を取得
    /// </summary>
    public string? GetAppSetting(string category, string key)
    {
        if (_appSettings.TryGetValue(category, out var categoryDict) &&
            categoryDict.TryGetValue(key, out var encryptedValue))
        {
            return EncryptionHelper.Decrypt(encryptedValue, _encryptionKey);
        }
        return null;
    }

    /// <summary>
    /// アプリ設定を保存
    /// </summary>
    public void SetAppSetting(string category, string key, string value)
    {
        if (!_appSettings.ContainsKey(category))
            _appSettings[category] = new Dictionary<string, string>();

        var encryptedValue = EncryptionHelper.Encrypt(value, _encryptionKey);
        _appSettings[category][key] = encryptedValue;
    }

    /// <summary>
    /// ユーザー設定を取得
    /// </summary>
    public string? GetUserSetting(string category, string key)
    {
        if (_userSettings.TryGetValue(category, out var categoryDict) &&
            categoryDict.TryGetValue(key, out var encryptedValue))
        {
            return EncryptionHelper.Decrypt(encryptedValue, _encryptionKey);
        }
        return null;
    }

    /// <summary>
    /// ユーザー設定を保存
    /// </summary>
    public void SetUserSetting(string category, string key, string value)
    {
        if (!_userSettings.ContainsKey(category))
            _userSettings[category] = new Dictionary<string, string>();

        var encryptedValue = EncryptionHelper.Encrypt(value, _encryptionKey);
        _userSettings[category][key] = encryptedValue;
    }

    private Dictionary<string, Dictionary<string, string>> LoadFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
                return new Dictionary<string, Dictionary<string, string>>();

            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, string>>>(json)
                   ?? new Dictionary<string, Dictionary<string, string>>();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"設定ファイルの読み込みに失敗しました: {filePath}", ex);
        }
    }

    private void SaveToFile(string filePath, Dictionary<string, Dictionary<string, string>> settings)
    {
        try
        {
            var directory = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
                Directory.CreateDirectory(directory);

            var options = new JsonSerializerOptions { WriteIndented = true };
            var json = JsonSerializer.Serialize(settings, options);
            File.WriteAllText(filePath, json);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException($"設定ファイルの保存に失敗しました: {filePath}", ex);
        }
    }
}
