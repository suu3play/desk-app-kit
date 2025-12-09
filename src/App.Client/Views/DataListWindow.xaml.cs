using System.Windows;
using DeskAppKit.Client.ViewModels;

namespace DeskAppKit.Client.Views;

/// <summary>
/// Interaction logic for DataListWindow.xaml
/// </summary>
public partial class DataListWindow : Window
{
    public DataListWindow(DataListViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
