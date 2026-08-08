using CommunityToolkit.Mvvm.ComponentModel;
using PocketAlbum.Studio.Services;

namespace PocketAlbum.Studio.ViewModels;

public partial class MainWindowViewModel(AlbumService albumService) : ObservableObject
{
    public MainWindowViewModel() : this(new AlbumService())
    {
    }

    public AlbumService Album { get; } = albumService;

    [ObservableProperty]
    private SlideshowItem? selectedImage;

    public string WindowTitle => $"PocketAlbum Studio {PocketAlbumConstants.VersionString}";
}