using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using PocketAlbum.Studio.ViewModels;
using PocketAlbum.Studio.Views;
using PocketAlbum.Server;
using System.Threading.Tasks;
using PocketAlbum.Server.Controllers;
using Avalonia.Threading;

namespace PocketAlbum.Studio;

public partial class App : Application
{
    public ServerHost? ServerHost { get; private set; }

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
                DataContext = new GalleryViewModel(),
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

    public async Task<ServerHost> StartServer(IAlbum album)
    {
        ServerHost = new ServerHost([], AuthService_ConnectionRequest, [ album ]);
        await ServerHost.Start();
        return ServerHost;
    }

    private string AuthService_ConnectionRequest(TokenRequest request)
    {
        var codeTask = new TaskCompletionSource<string>();
        Dispatcher.UIThread.Post(async () =>
        {
            var model = new PairViewModel(request);
            var pairDialog = new PairWindow()
            {
                DataContext = model
            };
            if (Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                if (await pairDialog.ShowDialog<bool?>(desktop.MainWindow!) == true)
                {
                    codeTask.SetResult(model.Code);
                    return;
                }
            }
            codeTask.SetResult("");
        });
        codeTask.Task.Wait();
        return codeTask.Task.Result;
    }

    public async Task StopServer()
    {
        if (ServerHost != null)
        {
            await ServerHost.Stop();
        }
    }
}
