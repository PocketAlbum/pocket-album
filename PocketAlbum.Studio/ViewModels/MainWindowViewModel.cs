using System;
using System.ComponentModel;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using PocketAlbum.Studio.Services;

namespace PocketAlbum.Studio.ViewModels;

public partial class MainWindowViewModel : ObservableObject
{
    public MainWindowViewModel() : this(App.GetServices())
    { }

    private MainWindowViewModel(IServiceProvider services) : this(
        services.GetRequiredService<AlbumService>(),
        services.GetRequiredService<ServerService>()) 
    { }

    public MainWindowViewModel(AlbumService albumService, ServerService serverService)
    {
        Album = albumService;
        Server = serverService;
        Server.ServerStateChanged += ServerStateChanged;
    }

    public AlbumService Album { get; }
    public ServerService Server { get; }

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
}