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

    public string WindowTitle => $"PocketAlbum Studio {PocketAlbum.VersionString}{(AlbumPath != "" ? "  -  " : "")}{AlbumPath}";

    public string StatusString
    {
        get 
        {
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
        Images = await ObservableAlbum.FromAlbum(album, new Models.FilterModel());
        Album = album;
        AlbumPath = path;
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