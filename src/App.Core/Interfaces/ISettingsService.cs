using DeskAppKit.Core.Enums;

namespace DeskAppKit.Core.Interfaces;

/// <summary>
/// 設定サービスインターフェイス
/// </summary>
public interface ISettingsService
{
    /// <summary>
    /// 現在のStorageModeを取得
    /// </summary>
    StorageMode GetStorageMode();

    /// <summary>
    /// 設定値を取得
    /// </summary>
    T Get<T>(string category, string key, T defaultValue = default!);

    /// <summary>
    /// 設定値を保存
    /// </summary>
    void Set<T>(string category, string key, T value);

    /// <summary>
    /// ユーザー別設定値を取得
    /// </summary>
    T GetUser<T>(Guid userId, string category, string key, T defaultValue = default!);

    /// <summary>
    /// ユーザー別設定値を保存
    /// </summary>
    void SetUser<T>(Guid userId, string category, string key, T value);

    /// <summary>
    /// すべての設定を読み込み
    /// </summary>
    void Load();

    /// <summary>
    /// すべての設定を保存
    /// </summary>
    void Save();
}
