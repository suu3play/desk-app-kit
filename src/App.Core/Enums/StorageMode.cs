namespace DeskAppKit.Core.Enums;

/// <summary>
/// 設定とログの保存方式
/// </summary>
public enum StorageMode
{
    /// <summary>
    /// ローカルファイル（settings_app.json, settings_user.json, logs/）
    /// </summary>
    Local,

    /// <summary>
    /// データベース（Settings, Logsテーブル）
    /// </summary>
    Database
}
