using System.Windows;

namespace DeskAppKit.Client.Services;

/// <summary>
/// ダイアログサービス
/// </summary>
public class DialogService
{
    /// <summary>
    /// 情報メッセージを表示
    /// </summary>
    public void ShowInformation(string message, string title = "情報")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    /// <summary>
    /// 警告メッセージを表示
    /// </summary>
    public void ShowWarning(string message, string title = "警告")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Warning);
    }

    /// <summary>
    /// エラーメッセージを表示
    /// </summary>
    public void ShowError(string message, string title = "エラー")
    {
        MessageBox.Show(message, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// 確認ダイアログを表示
    /// </summary>
    public bool ShowConfirmation(string message, string title = "確認")
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.YesNo, MessageBoxImage.Question);
        return result == MessageBoxResult.Yes;
    }

    /// <summary>
    /// はい/いいえ/キャンセルダイアログを表示
    /// </summary>
    public MessageBoxResult ShowYesNoCancel(string message, string title = "確認")
    {
        return MessageBox.Show(message, title, MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
    }

    /// <summary>
    /// OKキャンセルダイアログを表示
    /// </summary>
    public bool ShowOkCancel(string message, string title = "確認")
    {
        var result = MessageBox.Show(message, title, MessageBoxButton.OKCancel, MessageBoxImage.Question);
        return result == MessageBoxResult.OK;
    }

    /// <summary>
    /// エラーと詳細を表示
    /// </summary>
    public void ShowErrorWithDetails(string message, string details, string title = "エラー詳細")
    {
        var fullMessage = $"{message}\n\n【詳細】\n{details}";
        MessageBox.Show(fullMessage, title, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// 例外を表示
    /// </summary>
    public void ShowException(Exception ex, string userMessage = "予期しないエラーが発生しました")
    {
        var message = $"{userMessage}\n\n【エラー詳細】\n{ex.Message}";

        if (ex.InnerException != null)
        {
            message += $"\n\n【内部エラー】\n{ex.InnerException.Message}";
        }

        MessageBox.Show(message, "エラー", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    /// <summary>
    /// 削除確認ダイアログを表示
    /// </summary>
    public bool ShowDeleteConfirmation(string itemName)
    {
        var message = $"'{itemName}' を削除してもよろしいですか?\n\nこの操作は元に戻せません。";
        return ShowConfirmation(message, "削除の確認");
    }

    /// <summary>
    /// 保存確認ダイアログを表示
    /// </summary>
    public MessageBoxResult ShowSaveConfirmation()
    {
        var message = "変更を保存しますか?";
        return ShowYesNoCancel(message, "保存の確認");
    }
}
