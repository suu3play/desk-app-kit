using System.Windows;
using System.Windows.Input;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;

namespace DeskAppKit.Client.ViewModels;

/// <summary>
/// ログイン画面のViewModel
/// </summary>
public class LoginViewModel : ViewModelBase
{
    private readonly IAuthenticationService _authenticationService;
    private readonly ILogger _logger;
    private string _loginId = string.Empty;
    private string _password = string.Empty;
    private bool _isLoading;
    private string _errorMessage = string.Empty;

    public LoginViewModel(IAuthenticationService authenticationService, ILogger logger)
    {
        _authenticationService = authenticationService;
        _logger = logger;

        LoginCommand = new RelayCommand(async () => await LoginAsync(), CanLogin);
        ClearCommand = new RelayCommand(Clear);
    }

    public string LoginId
    {
        get => _loginId;
        set
        {
            if (SetProperty(ref _loginId, value))
            {
                ErrorMessage = string.Empty;
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public string Password
    {
        get => _password;
        set
        {
            if (SetProperty(ref _password, value))
            {
                ErrorMessage = string.Empty;
                ((RelayCommand)LoginCommand).RaiseCanExecuteChanged();
            }
        }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set => SetProperty(ref _isLoading, value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand ClearCommand { get; }

    public event EventHandler<User>? LoginSucceeded;

    private bool CanLogin()
    {
        return !string.IsNullOrWhiteSpace(LoginId) &&
               !string.IsNullOrWhiteSpace(Password) &&
               !IsLoading;
    }

    private async Task LoginAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = string.Empty;

            _logger.Info("Login", $"ログイン試行: {LoginId}");

            var user = await _authenticationService.LoginAsync(LoginId, Password);

            if (user != null)
            {
                _logger.Info("Login", $"ログイン成功: {user.LoginId}");
                LoginSucceeded?.Invoke(this, user);
            }
            else
            {
                ErrorMessage = "ログインIDまたはパスワードが正しくありません。";
                _logger.Warn("Login", $"ログイン失敗: {LoginId}");
            }
        }
        catch (InvalidOperationException ex)
        {
            ErrorMessage = ex.Message;
            _logger.Warn("Login", $"ログインエラー: {ex.Message}");
        }
        catch (Exception ex)
        {
            ErrorMessage = "ログイン処理でエラーが発生しました。";
            _logger.Error("Login", "ログイン処理でエラーが発生しました", ex);
        }
        finally
        {
            IsLoading = false;
            Password = string.Empty; // セキュリティのためパスワードをクリア
        }
    }

    private void Clear()
    {
        LoginId = string.Empty;
        Password = string.Empty;
        ErrorMessage = string.Empty;
    }
}
