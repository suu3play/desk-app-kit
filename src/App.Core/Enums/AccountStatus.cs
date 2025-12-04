namespace DeskAppKit.Core.Enums;

/// <summary>
/// ユーザーアカウントの状態
/// </summary>
public enum AccountStatus
{
    /// <summary>
    /// 有効
    /// </summary>
    Active,

    /// <summary>
    /// ロック中（ログイン失敗回数超過）
    /// </summary>
    Locked,

    /// <summary>
    /// 一時停止
    /// </summary>
    Suspended,

    /// <summary>
    /// 無効化（退職等）
    /// </summary>
    Retired
}
