using System;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using PocketAlbum.Models;
using PocketAlbum.SQLite;
using PocketAlbum.Studio.Core;
using PocketAlbum.Studio.ViewModels;

namespace PocketAlbum.Studio.Views;

public partial class MainWindow : Window
{
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
            SuggestedFileName = $"{metadata.Name}.sqlite",
            ShowOverwritePrompt = false
        });
        if (files == null)
        {
            return;
        }
        try {
            if (DataContext is GalleryViewModel)
            {
                var path = files.Path.ToString()[8..];
                await SQLiteAlbum.Create(path, metadata);
                await OpenAlbum(path);
            }
        }
        catch (Exception e)
        {
            await ShowError(e.Message);
        }
    }

    public async void OpenAlbumClick(object sender, RoutedEventArgs args)
    {
        var files = await StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions
        {
            Title = "Open album file",
            AllowMultiple = false
        });

        try {
            if (files.SingleOrDefault()?.Path?.ToString() is string path && 
                path.StartsWith("file://"))
            {
                await OpenAlbum(path[8..]);
            }
        }
        catch (Exception e)
        {
            await ShowError(e.Message);
        }
    }

    private async Task ShowError(string message)
    {
        await MessageBoxManager
                .GetMessageBoxStandard("Error", message, ButtonEnum.Ok, 
                    MsBox.Avalonia.Enums.Icon.Error)
                .ShowAsync();
    }

    private async Task OpenAlbum(string path)
    {
        if (DataContext is GalleryViewModel gvm)
        {
            var album = await SQLiteAlbum.Open(path);
            await gvm.OpenAlbum(album, path);
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
        if (DataContext is GalleryViewModel gvm && gvm.Album is IAlbum album)
        {
            var folder = await StorageProvider.OpenFolderPickerAsync(
                new FolderPickerOpenOptions { Title = "Select images to import" });
            if (!folder.Any())
            {
                return;
            }
            var path = folder.Single().Path.ToString().Substring(8);
            RecursiveFilesImporter importer = new RecursiveFilesImporter(path, album);
            await importer.Start(this);
            await gvm.OpenAlbum(album, gvm.AlbumPath);
        }
    }

    public async void EditMetadataClick(object? sender, RoutedEventArgs args)
    {
        if (DataContext is GalleryViewModel gvm && gvm.Album is IAlbum album)
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

    public async void CloseAlbumClick(object? sender, RoutedEventArgs args)
    {
        if (DataContext is GalleryViewModel gvm)
        {
            gvm.CloseAlbum();
        }
    }

    public async void ShowStatisticsClick(object? sender, RoutedEventArgs args)
    {
        if (DataContext is GalleryViewModel gvm && gvm.Album is IAlbum album) {
            AlbumStatisticsWindow window = new AlbumStatisticsWindow()
            {
                DataContext = new AlbumStatisticsViewModel()
                {
                    Album = album,
                    Metadata = await album.GetMetadata(),
                    YearIndex = await album.GetYearIndex()
                }
            };
            await window.ShowDialog(this);
        }
    }
}