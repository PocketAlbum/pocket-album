using Avalonia.Media.Imaging;
using Microsoft.Extensions.DependencyInjection;
using PocketAlbum.Server;
using PocketAlbum.Server.Controllers;
using PocketAlbum.Studio.Services;
using QRCoder;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace PocketAlbum.Studio.ViewModels;

internal class ServerViewModel : ViewModelBase
{
    public ServerViewModel() : this(App.GetServices())
    { }

    private ServerViewModel(IServiceProvider services) : this(
        services.GetRequiredService<ServerService>()) 
    { }

    public ServerViewModel(ServerService serverService)
    {
        Server = serverService;
        Server.ServerStateChanged += ServerHost_ServerStateChanged;
        if (Server.Host.CurrentInstance is ServerHost.ServerInstance inst)
        {
            inst.AuthService.ClientsChanged += AuthService_ClientsChanged;
        }
    }

    public ServerService Server { get; }

    private void AuthService_ClientsChanged()
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Clients)));
    }

    private void ServerHost_ServerStateChanged()
    {
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(ServerInfoQr)));
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(ServerRunning)));
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(ServerStatus)));
        OnPropertyChanged(new PropertyChangedEventArgs(nameof(Clients)));
        if (Server.Host.CurrentInstance is ServerHost.ServerInstance inst)
        {
            inst.AuthService.ClientsChanged += AuthService_ClientsChanged;
        }
    }

    public ServerInfo? ServerInfo
    {
        get
        {
            var urls = Server.Host.CurrentInstance?.WebApp.Urls;
            return Server.Host.CurrentInstance?.AuthService.GetServerInfo(urls!);
        }
    }

    public Bitmap? ServerInfoQr => GenerateQrCode(JsonSerializer.Serialize(ServerInfo));

    public bool ServerRunning => Server.IsRunning;

    public string ServerStatus => ServerRunning ? "Running" : "Stopped";

    public IList<string> Clients => Server.Host.CurrentInstance?.AuthService.Clients ?? [];

    private static Bitmap GenerateQrCode(string text)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);

        var pngQrCode = new PngByteQRCode(qrData);
        byte[] pngBytes = pngQrCode.GetGraphic(20);

        using var stream = new MemoryStream(pngBytes);

        return new Bitmap(stream);
    }
}
