using System.Windows;
using DeskAppKit.Client.ViewModels;

namespace DeskAppKit.Client.Views;

/// <summary>
/// LoginWindow.xaml の相互作用ロジック
/// </summary>
public partial class LoginWindow : Window
{
    public LoginWindow()
    {
        InitializeComponent();
    }

    public LoginWindow(LoginViewModel viewModel) : this()
    {
        DataContext = viewModel;

        // PasswordBoxのバインディング（セキュリティ上、XAMLでバインドできないため）
        PasswordBox.PasswordChanged += (s, e) =>
        {
            if (DataContext is LoginViewModel vm)
            {
                vm.Password = PasswordBox.Password;
            }
        };

        // Enterキーでログイン
        LoginIdTextBox.KeyDown += (s, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter)
            {
                PasswordBox.Focus();
            }
        };

        PasswordBox.KeyDown += (s, e) =>
        {
            if (e.Key == System.Windows.Input.Key.Enter && viewModel.LoginCommand.CanExecute(null))
            {
                viewModel.LoginCommand.Execute(null);
            }
        };

        // ログイン成功時にウィンドウを閉じる
        viewModel.LoginSucceeded += (s, user) =>
        {
            DialogResult = true;
            Close();
        };

        // フォーカス設定
        Loaded += (s, e) => LoginIdTextBox.Focus();
    }
}
