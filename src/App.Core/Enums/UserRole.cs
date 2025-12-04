namespace DeskAppKit.Core.Enums;

/// <summary>
/// ユーザー権限ロール
/// </summary>
public enum UserRole
{
    /// <summary>
    /// 参照のみ
    /// </summary>
    Viewer,

    /// <summary>
    /// 通常利用・診断
    /// </summary>
    User,

    /// <summary>
    /// 設定・ユーザー管理・アップデート・ダウングレード
    /// </summary>
    Admin
}
