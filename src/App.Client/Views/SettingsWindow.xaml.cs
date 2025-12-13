using System.Windows;
using System.Windows.Controls;
using DeskAppKit.Client.ViewModels;

namespace DeskAppKit.Client.Views;

/// <summary>
/// Interaction logic for SettingsWindow.xaml
/// </summary>
public partial class SettingsWindow : Window
{
    private readonly SettingsViewModel _viewModel;

    public SettingsWindow(SettingsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        DataContext = viewModel;

        // 初期パスワード設定
        PasswordBox.Password = viewModel.DatabaseSettings.DbPassword;
    }

    private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
    {
        if (sender is PasswordBox passwordBox)
        {
            _viewModel.DatabaseSettings.DbPassword = passwordBox.Password;
        }
    }

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var result = MessageBox.Show(
            "設定を保存しますか?\nStorageModeを変更した場合はアプリケーションの再起動が必要です。",
            "確認",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result == MessageBoxResult.Yes)
        {
            // SaveCommandを実行して設定を保存
            if (_viewModel.SaveCommand.CanExecute(null))
            {
                _viewModel.SaveCommand.Execute(null);
            }

            // データベース設定も保存
            if (_viewModel.DatabaseSettings.SaveCommand.CanExecute(null))
            {
                _viewModel.DatabaseSettings.SaveCommand.Execute(null);
            }

            DialogResult = true;
            Close();
        }
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }
}
