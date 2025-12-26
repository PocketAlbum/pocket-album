using System.Collections.Generic;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PocketAlbum.Studio.ViewModels;

public partial class GalleryViewModel : ObservableObject
{
    [ObservableProperty]
    private IReadOnlyList<GalleryItem>? images;

    public IAlbum? Album;

    public bool HasImages => Album != null && (Images?.Count ?? 0) > 0;

    public string AlbumPath = "";

    [ObservableProperty]
    private double? progress;

    public bool HasProgress => Progress.HasValue;

    public string WindowTitle => $"PocketAlbum Studio {PocketAlbum.VersionString}{(AlbumPath != "" ? "  -  " : "")}{AlbumPath}";

    public string StatusString
    {
        get 
        {
            if (HasProgress)
            {
                return "Opening album";
            }
            if (Album == null)
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
        Progress = 0;
        OnPropertyChanged(nameof(HasProgress));
        OnPropertyChanged(nameof(StatusString));
        await new IntegrityChecker(album).CheckAllYears(p =>
        {
            Progress = p * 0.9;
        });

        Progress = 0.9;
        Images = await ObservableAlbum.FromAlbum(album, new Models.FilterModel());

        Album = album;
        AlbumPath = path;
        Progress = null;
        OnPropertyChanged(nameof(HasProgress));
        OnPropertyChanged(nameof(HasImages));
        OnPropertyChanged(nameof(StatusString));
        OnPropertyChanged(nameof(WindowTitle));
    }

    internal void CloseAlbum()
    {
        Album = null;
        Images = null;
        AlbumPath = "";
        OnPropertyChanged(nameof(HasImages));
        OnPropertyChanged(nameof(StatusString));
        OnPropertyChanged(nameof(WindowTitle));
    }
}