using Avalonia.Controls;
using Avalonia.Interactivity;
using PocketAlbum.Studio.ViewModels;

namespace PocketAlbum.Studio.Views;

public partial class ServerWindow : Window
{    
    public ServerWindow()
    {
        InitializeComponent();
    }

    public async void StopServer(object? sender, RoutedEventArgs args)
    {
        if (DataContext is ServerViewModel svm)
        {
            await svm.ServerHost.Stop();
        }
    }

    public async void StartServer(object? sender, RoutedEventArgs args)
    {
        if (DataContext is ServerViewModel svm)
        {
            //await svm.ServerHost.Start();
        }
    }
}
