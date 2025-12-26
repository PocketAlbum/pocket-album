using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PocketAlbum.Models;
using PocketAlbum.SQLite;
using PocketAlbum.Studio.Core;
using PocketAlbum.Studio.ViewModels;

namespace PocketAlbum.Studio.Views;

public partial class MainWindow : Window
{
    IAlbum? album;

    public MainWindow()
    {
        InitializeComponent();
    }

    public async void NewAlbumClick(object sender, RoutedEventArgs args)
    {
        MetadataWindow window = new MetadataWindow()
        {
            DataContext = new MetadataViewModel()
        };
        var metadata = await window.ShowDialog<MetadataModel?>(this);
        if (metadata == null)
        {
            return;
        }
        var files = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
        {
            Title = "Save album to file",
            SuggestedFileName = $"{metadata.Name}.sqlite"
        });
        if (files == null)
        {
            return;
        }
        await CreateAlbum(files.Path.ToString()[8..], metadata);
    }

    private async Task CreateAlbum(string path, MetadataModel metadata)
    {
        album = await SQLiteAlbum.Create(path, metadata);

        if (DataContext is GalleryViewModel gvm)
        {
            await gvm.OpenAlbum(album);
        }
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
        Environment.Exit(0);
    }

    public async void ImportImagesClick(object? sender, RoutedEventArgs args)
    {
        if (album != null)
        {
            var folder = await StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions { Title = "Select images to import" });
            if (!folder.Any())
            {
                return;
            }
            var path = folder.Single().Path.ToString().Substring(8);
            RecursiveFilesImporter importer = new RecursiveFilesImporter(path, album);
            importer.Start();
        }
    }

    public async void EditMetadataClick(object? sender, RoutedEventArgs args)
    {
        if (album != null)
        {
            var metadata = await album.GetMetadata();
            MetadataWindow window = new MetadataWindow()
            {
                DataContext = new MetadataViewModel()
                {
                    Name = metadata.Name,
                    Description = metadata.Description
                }
            };
            var newMetadata = await window.ShowDialog<MetadataModel?>(this);
            if (newMetadata == null)
            {
                return;
            }
            await album.SetMetadata(new MetadataModel
            {
                Id = metadata.Id,
                Version = PocketAlbum.VersionName,
                Name = newMetadata.Name,
                Description = newMetadata.Description,
                Created = metadata.Created,
                Updated = DateTime.Now
            });
        }
    }
}