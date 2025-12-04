using System.Text.Json;
using DeskAppKit.Core.Enums;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Infrastructure.Settings.Local;
using DeskAppKit.Infrastructure.Settings.Database;

namespace DeskAppKit.Infrastructure.Settings;

/// <summary>
/// 設定サービス実装
/// </summary>
public class SettingsService : ISettingsService
{
    private readonly LocalSettingsStore _localStore;
    private readonly BootstrapDbManager _bootstrapDbManager;
    private StorageMode _currentMode;
    private string _baseDirectory;

    public SettingsService(string baseDirectory, string encryptionKey)
    {
        _baseDirectory = baseDirectory;
        _localStore = new LocalSettingsStore(baseDirectory, encryptionKey);
        _bootstrapDbManager = new BootstrapDbManager(baseDirectory, encryptionKey);

        // 初期化時にLocalから読み込み
        _localStore.Load();
        _currentMode = DetermineStorageMode();
    }

    /// <summary>
    /// StorageModeを判定（フェイルセーフ付き）
    /// </summary>
    private StorageMode DetermineStorageMode()
    {
        // settings_app.jsonからStorageModeを取得
        var modeStr = _localStore.GetAppSetting("System", "StorageMode");

        if (modeStr == "Database")
        {
            // Databaseモードが指定されている場合、接続可能かチェック
            try
            {
                var dbConfig = _bootstrapDbManager.Load();
                if (dbConfig == null)
                {
                    // bootstrap_db.jsonが存在しない → Localにフォールバック
                    return StorageMode.Local;
                }

                // 接続テスト（同期的に実行）
                var canConnect = _bootstrapDbManager.TestConnectionAsync(dbConfig).GetAwaiter().GetResult();
                if (!canConnect)
                {
                    // 接続失敗 → Localにフォールバック
                    return StorageMode.Local;
                }

                return StorageMode.Database;
            }
            catch
            {
                // エラー発生 → Localにフォールバック
                return StorageMode.Local;
            }
        }

        return StorageMode.Local;
    }

    public StorageMode GetStorageMode()
    {
        return _currentMode;
    }

    public T Get<T>(string category, string key, T defaultValue = default!)
    {
        try
        {
            if (_currentMode == StorageMode.Local)
            {
                var value = _localStore.GetAppSetting(category, key);
                if (value == null)
                    return defaultValue;

                return DeserializeValue<T>(value);
            }
            else
            {
                // TODO: Database実装
                return defaultValue;
            }
        }
        catch
        {
            return defaultValue;
        }
    }

    public void Set<T>(string category, string key, T value)
    {
        var serialized = SerializeValue(value);

        if (_currentMode == StorageMode.Local)
        {
            _localStore.SetAppSetting(category, key, serialized);
        }
        else
        {
            // TODO: Database実装
        }
    }

    public T GetUser<T>(Guid userId, string category, string key, T defaultValue = default!)
    {
        try
        {
            if (_currentMode == StorageMode.Local)
            {
                var value = _localStore.GetUserSetting(category, key);
                if (value == null)
                    return defaultValue;

                return DeserializeValue<T>(value);
            }
            else
            {
                // TODO: Database実装
                return defaultValue;
            }
        }
        catch
        {
            return defaultValue;
        }
    }

    public void SetUser<T>(Guid userId, string category, string key, T value)
    {
        var serialized = SerializeValue(value);

        if (_currentMode == StorageMode.Local)
        {
            _localStore.SetUserSetting(category, key, serialized);
        }
        else
        {
            // TODO: Database実装
        }
    }

    public void Load()
    {
        if (_currentMode == StorageMode.Local)
        {
            _localStore.Load();
        }
        else
        {
            // TODO: Database実装
        }
    }

    public void Save()
    {
        if (_currentMode == StorageMode.Local)
        {
            _localStore.Save();

            // バージョン情報をLocalにも保存（フォールバック時のため）
            SaveVersionInfoToLocal();
        }
        else
        {
            // TODO: Database実装
        }
    }

    private void SaveVersionInfoToLocal()
    {
        // LastKnownAppVersion, LastKnownSchemaVersionをローカルに保存
        // これによりLocal Fallback時もバージョン情報が利用可能
    }

    private string SerializeValue<T>(T value)
    {
        if (value == null)
            return string.Empty;

        if (value is string str)
            return str;

        return JsonSerializer.Serialize(value);
    }

    private T DeserializeValue<T>(string value)
    {
        if (typeof(T) == typeof(string))
            return (T)(object)value;

        if (string.IsNullOrEmpty(value))
            return default!;

        return JsonSerializer.Deserialize<T>(value)!;
    }
}
