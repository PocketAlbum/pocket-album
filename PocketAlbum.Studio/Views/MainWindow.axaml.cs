using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PocketAlbum.SQLite;
using PocketAlbum.Studio.ViewModels;

namespace PocketAlbum.Studio.Views;

public partial class MainWindow : Window
{
    IAlbum album;

    public MainWindow()
    {
        InitializeComponent();
    }

    public async void OpenAlbumClick(object sender, RoutedEventArgs args)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open album file",
            AllowMultiple = false
        });

        if (files.SingleOrDefault()?.Path?.ToString() is string path && 
            path.StartsWith("file://"))
        {
            await OpenAlbum(path.Substring(7));
        }
    }

    private async Task OpenAlbum(string path)
    {
        album = await SQLiteAlbum.Open(path);

        if (DataContext is GalleryViewModel gvm)
        {
            await gvm.OpenAlbum(album);
        }
    }

    private void OnElementPrepared(object? sender, 
        ItemsRepeaterElementPreparedEventArgs args)
    {
        if (args.Element is Image img && img.DataContext is GalleryItem item)
        {
            _ = item.EnsureLoadedAsync();
        }
    }

    public async void ExitClick(object? sender, RoutedEventArgs args)
    {
        Close();
        System.Environment.Exit(0);
    }

    public async void ImportImagesClick(object? sender, RoutedEventArgs args)
    {
        /*if (album != null)*/ 
        {
            /*ImportWindow importWindow = new ImportWindow(album)
            {
                DataContext = new ImportWindowViewModel()
            };
            importWindow.Show();*/
            ImageProgressWindow progressWindow = new ImageProgressWindow();
            progressWindow.Show();
        }
    }
}