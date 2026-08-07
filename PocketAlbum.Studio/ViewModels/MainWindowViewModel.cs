using CommunityToolkit.Mvvm.ComponentModel;
using PocketAlbum.Studio.Services;

namespace PocketAlbum.Studio.ViewModels;

public partial class MainWindowViewModel(AlbumService albumService) : ObservableObject
{
    public AlbumService Album { get; } = albumService;

    [ObservableProperty]
    private SlideshowItem? selectedImage;
}