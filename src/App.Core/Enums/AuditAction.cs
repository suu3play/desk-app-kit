namespace DeskAppKit.Core.Enums;

/// <summary>
/// 監査ログ操作種別
/// </summary>
public enum AuditAction
{
    /// <summary>
    /// 作成
    /// </summary>
    Create,

    /// <summary>
    /// 読み取り
    /// </summary>
    Read,

    /// <summary>
    /// 更新
    /// </summary>
    Update,

    /// <summary>
    /// 削除
    /// </summary>
    Delete,

    /// <summary>
    /// ログイン
    /// </summary>
    Login,

    /// <summary>
    /// ログアウト
    /// </summary>
    Logout,

    /// <summary>
    /// エクスポート
    /// </summary>
    Export,

    /// <summary>
    /// インポート
    /// </summary>
    Import
}
