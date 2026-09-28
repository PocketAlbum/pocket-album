using Avalonia.Controls;
using Avalonia.Interactivity;
using PocketAlbum.Studio.ViewModels;

namespace PocketAlbum.Studio.Views;

public partial class SlideshowControl : UserControl
{
    public SlideshowControl()
    {
        InitializeComponent();
    }

    public async void CastClick(object? sender, RoutedEventArgs args)
    {
        if (DataContext is MainWindowViewModel vm)
        {
            await vm.Cast();
        }
    }
}