using System.Windows;
using DeskAppKit.Client.ViewModels;

namespace DeskAppKit.Client.Views;

/// <summary>
/// Interaction logic for DiagnosticsWindow.xaml
/// </summary>
public partial class DiagnosticsWindow : Window
{
    public DiagnosticsWindow(DiagnosticsViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
