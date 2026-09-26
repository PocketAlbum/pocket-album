using System;
using System.Threading.Tasks;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using PocketAlbum.Server;
using PocketAlbum.Server.Controllers;
using PocketAlbum.Studio.ViewModels;
using PocketAlbum.Studio.Views;

namespace PocketAlbum.Studio.Services;

public partial class ServerService(AlbumService albumService) : ObservableObject
{
    public ServerHost Host { private set; get; }

    [ObservableProperty]
    private bool isOpened;

    public event Action? ServerStateChanged;

    public bool IsRunning => Host?.IsRunning ?? false;
    
    public async Task<ServerHost> StartServer()
    {
        Host = new ServerHost([], AuthService_ConnectionRequest, [ albumService.Current ]);
        Host.ServerStateChanged += () =>
        {
            ServerStateChanged?.Invoke();  
        };
        await Host.Start();
        return Host;
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
            if (App.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
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
        if (Host != null)
        {
            await Host.Stop();
        }
    }
}