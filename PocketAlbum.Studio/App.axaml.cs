using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using PocketAlbum.Studio.ViewModels;
using PocketAlbum.Studio.Views;
using PocketAlbum.Server;
using Microsoft.Extensions.DependencyInjection;
using PocketAlbum.Studio.Services;

namespace PocketAlbum.Studio;

public partial class App : Application
{
    public ServerHost? ServerHost { get; private set; }

    public readonly ServiceProvider Services = GetServices();

    public static ServiceProvider GetServices()
    {
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<AlbumService>();
        services.AddSingleton<ServerService>();

        // View models
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ServerViewModel>();

        return services.BuildServiceProvider();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
