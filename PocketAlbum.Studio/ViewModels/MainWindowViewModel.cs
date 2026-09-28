using System;
using System.ComponentModel;
using System.Threading.Tasks;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using PocketAlbum.Studio.Services;
using PocketAlbum.Studio.Views;

namespace PocketAlbum.Studio.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel() : this(App.GetServices())
    { }

    private MainWindowViewModel(IServiceProvider services) : this(
        services.GetRequiredService<AlbumService>(),
        services.GetRequiredService<ServerService>(),
        services.GetRequiredService<CastingService>()) 
    { }

    public MainWindowViewModel(AlbumService albumService, ServerService serverService,
        CastingService castingService)
    {
        Album = albumService;
        Server = serverService;
        Casting = castingService;
        Server.ServerStateChanged += ServerStateChanged;
    }

    public AlbumService Album { get; }
    public ServerService Server { get; }
    public CastingService Casting { get; }

    [ObservableProperty]
    private SlideshowItem? selectedImage;

    public string WindowTitle => $"PocketAlbum Studio {PocketAlbumConstants.VersionString}";

    public IBrush ServerIndicator => new SolidColorBrush(
        Server.IsRunning ? Color.FromRgb(0, 255, 0) : Color.FromRgb(150, 150, 150));

    public string ServerStatus => Server.IsRunning ? 
        "Server running" : "No server running";

    private void ServerStateChanged()
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(ServerStatus)));
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(ServerIndicator)));
    }

    public async Task Cast()
    {
        if (Album.Current is IAlbum album && selectedImage is SlideshowItem it)
        {
            var data = await album.GetImageData(it.Id);
            var info = await album.GetImageInfo(it.Id);
            await Casting.Cast(info, data);
        }
    }

    internal void SelectImage(string id)
    {
        var item = new SlideshowItem(Album.Current, id);
        _ = item.EnsureLoadedAsync();
        SelectedImage = item;

        if (Casting.IsConnected)
        {
            _ = Cast();
        }
    }
}