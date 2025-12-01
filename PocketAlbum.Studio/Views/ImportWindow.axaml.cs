using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PocketAlbum.Studio.ViewModels;

namespace PocketAlbum.Studio.Views;

public partial class ImportWindow : Window
{
    private readonly IAlbum album;

    public ImportWindow(IAlbum album)
    {
        InitializeComponent();
        this.album = album;
    }

    public async void AddFilesClick(object? sender, RoutedEventArgs args)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Select images to import",
            AllowMultiple = true
        });

        if (DataContext is ImportWindowViewModel imvw)
        {
            foreach (var img in files)
            {
                string path = img.Path.ToString().Substring(6);
                imvw.ImportImages.Add(new ImportItem(path));
            }
        }
    }

    public async void AddFolderClick(object? sender, RoutedEventArgs args)
    {
        var folder = await StorageProvider.OpenFolderPickerAsync(
            new FolderPickerOpenOptions { Title = "Select images to import" });

        if (DataContext is ImportWindowViewModel imvw)
        {
            var path = folder.Single().Path.ToString().Substring(6);
            AddFilesRecursively(imvw.ImportImages, path);
        }
    }

    private void AddFilesRecursively(ObservableCollection<ImportItem> images, 
        string path)
    {
        foreach (var file in Directory.GetFiles(path))
        {
            images.Add(new ImportItem(file));
        }
        foreach (var folder in Directory.GetDirectories(path))
        {
            AddFilesRecursively(images, folder);
        }
    }

    public async void ImportClick(object? sender, RoutedEventArgs args)
    {
        
    }
}