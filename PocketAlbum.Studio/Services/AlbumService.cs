using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using PocketAlbum.Studio.ViewModels;

namespace PocketAlbum.Studio.Services;

public partial class AlbumService : ObservableObject
{
    [ObservableProperty]
    private IReadOnlyList<GalleryItem>? images;

    public IAlbum? Current;

    public bool HasImages => Current != null && (Images?.Count ?? 0) > 0;

    [ObservableProperty]
    private string albumPath = "";

    [ObservableProperty]
    private double? progress;

    [ObservableProperty]
    private bool isOpened;

    public bool HasProgress => Progress.HasValue;

    public Bitmap SourceIcon => GetSourceIcon();

    public string StatusString
    {
        get 
        {
            if (HasProgress)
            {
                return "Opening album";
            }
            if (Current == null)
            {
                return "No album opened";
            }
            if ((Images?.Count ?? 0) == 0)
            {
                return "Album is empty";
            }
            return "Ok";
        }
    }
    
    public async Task OpenAlbum(IAlbum album, string path)
    {
        await CloseAlbum();
        
        Progress = 0;
        OnPropertyChanged(nameof(HasProgress));
        OnPropertyChanged(nameof(StatusString));
        await IntegrityChecker.CheckAllYears(album, p =>
        {
            Progress = p * 0.9;
        });

        Progress = 0.9;
        Images = await ObservableAlbum.FromAlbum(album, new Models.FilterModel());

        Current = album;
        AlbumPath = path;
        Progress = null;
        IsOpened = true;
        OnPropertyChanged(nameof(HasProgress));
        OnPropertyChanged(nameof(HasImages));
        OnPropertyChanged(nameof(StatusString));
    }

    internal async Task CloseAlbum()
    {
        if (Current != null) {
            await Current.DisposeAsync();
            Current = null;
        }
        Images = null;
        AlbumPath = "";
        IsOpened = false;
        OnPropertyChanged(nameof(HasImages));
        OnPropertyChanged(nameof(StatusString));
    }

    public Bitmap GetSourceIcon()
    {
        var uri = new Uri("avares://PocketAlbum.Studio/Assets/MDI/database_black_24.png");
        return new Bitmap(AssetLoader.Open(uri));
    }
}