using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PocketAlbum.Studio.ViewModels;
using Sharpcaster.Models;

namespace PocketAlbum.Studio.Views;

public partial class CastWindow : Window
{
    public CastWindow()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is CastViewModel cvm)
        {
            cvm.RefreshList();
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Accept_Click(object? sender, RoutedEventArgs e)
    {
        Close(true);
    }

    private void Refresh_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CastViewModel cvm)
        {
            cvm.RefreshList();
        }
    }

    private async void ListBox_DoubleTapped(object? sender, RoutedEventArgs e)
    {
        if (DataContext is CastViewModel cvm && 
            cvm.SelectedReceiver is ChromecastReceiver receiver)
        {
            await cvm.Service.ConnectToChromecast(receiver);
        }
    }
}
