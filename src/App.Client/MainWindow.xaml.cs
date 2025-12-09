using System.Windows;
using DeskAppKit.Client.ViewModels;
using DeskAppKit.Core.Interfaces;
using DeskAppKit.Core.Models;
using DeskAppKit.Infrastructure.Diagnostics;
using DeskAppKit.Infrastructure.Settings;

namespace DeskAppKit.Client;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public MainWindow(
        User currentUser,
        ILogger? logger = null,
        HealthCheck? healthCheck = null,
        ISettingsService? settingsService = null,
        IThemeService? themeService = null,
        INotificationCenter? notificationCenter = null,
        IDataListService? dataListService = null,
        string? dataDirectory = null,
        string? encryptionKey = null)
    {
        InitializeComponent();

        var viewModel = new MainViewModel(currentUser, logger, healthCheck, settingsService, themeService, notificationCenter, dataListService, dataDirectory, encryptionKey);
        DataContext = viewModel;

        // ログアウトイベント
        viewModel.LogoutRequested += (s, e) =>
        {
            var result = MessageBox.Show(
                "ログアウトしますか?",
                "確認",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result == MessageBoxResult.Yes)
            {
                Close();
                Application.Current.Shutdown();
            }
        };
    }
}