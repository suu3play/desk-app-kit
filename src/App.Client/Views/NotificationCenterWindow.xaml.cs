using System.Windows;
using DeskAppKit.Client.ViewModels;

namespace DeskAppKit.Client.Views;

/// <summary>
/// NotificationCenterWindow.xaml の相互作用ロジック
/// </summary>
public partial class NotificationCenterWindow : Window
{
    public NotificationCenterWindow(NotificationCenterViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;

        Loaded += async (s, e) => await viewModel.LoadNotificationsAsync();
    }
}
