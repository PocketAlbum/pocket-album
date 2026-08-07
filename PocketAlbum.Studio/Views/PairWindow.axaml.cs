using Avalonia.Controls;
using Avalonia.Interactivity;
using PocketAlbum.Studio.ViewModels;

namespace PocketAlbum.Studio.Views;

public partial class PairWindow : Window
{
    public PairWindow()
    {
        InitializeComponent();
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Accept_Click(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }
}
