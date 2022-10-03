using Avalonia.Controls;
using Avalonia.Interactivity;
using Larus.ViewModels;
using ReactiveUI;
using System.Reactive;
using System.Threading.Tasks;

namespace Larus.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    public async void BrowseDir(object sender, RoutedEventArgs e)
    {
        // TODO change this to ReactiveCommand
        var dialog = new OpenFolderDialog();
        var result = await dialog.ShowAsync(this);

        if (result != null)
        {
            (DataContext as MainWindowViewModel)!.Dir = result;
        }
    }
}
