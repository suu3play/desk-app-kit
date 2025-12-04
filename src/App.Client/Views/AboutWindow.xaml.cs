using System.Windows;

namespace DeskAppKit.Client.Views;

/// <summary>
/// Interaction logic for AboutWindow.xaml
/// </summary>
public partial class AboutWindow : Window
{
    public AboutWindow()
    {
        InitializeComponent();
        DataContext = new
        {
            AppVersion = "1.0.0",
            DotNetVersion = Environment.Version.ToString(),
            OsVersion = Environment.OSVersion.ToString()
        };
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}
