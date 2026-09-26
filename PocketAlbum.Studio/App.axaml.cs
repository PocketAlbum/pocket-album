using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
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

    public readonly ServiceProvider Services;

    public App()
    {
        var services = new ServiceCollection();

        // Services
        services.AddSingleton<AlbumService>();
        services.AddSingleton<ServerService>();

        // View models
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<ServerViewModel>();

        Services = services.BuildServiceProvider();
    }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            desktop.MainWindow = new MainWindow
            {
                DataContext = Services.GetService<MainWindowViewModel>()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    private void DisableAvaloniaDataAnnotationValidation()
    {
        // Get an array of plugins to remove
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        // remove each entry found
        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
