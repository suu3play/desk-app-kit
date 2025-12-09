namespace DeskAppKit.Core.Interfaces;

/// <summary>
/// LocalDBセットアップサービスインターフェース
/// </summary>
public interface ILocalDbSetupService
{
    /// <summary>
    /// LocalDBが利用可能かチェック
    /// </summary>
    Task<bool> IsLocalDbAvailableAsync();

    /// <summary>
    /// LocalDBインスタンスが存在するかチェック
    /// </summary>
    Task<bool> InstanceExistsAsync();
}
