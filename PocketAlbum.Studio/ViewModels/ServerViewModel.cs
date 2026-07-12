using Avalonia.Media.Imaging;
using PocketAlbum.Server;
using PocketAlbum.Server.Controllers;
using QRCoder;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;

namespace PocketAlbum.Studio.ViewModels;

internal class ServerViewModel : ViewModelBase
{
    public ServerViewModel() : this(new ServerHost([], null))
    {

    }

    public ServerViewModel(ServerHost serverHost)
    {
        ServerHost = serverHost;
        ServerHost.ServerStateChanged += ServerHost_ServerStateChanged;
        if (ServerHost.CurrentInstance is ServerHost.ServerInstance inst)
        {
            inst.AuthService.ClientsChanged += AuthService_ClientsChanged;
        }
    }

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
        if (ServerHost.CurrentInstance is ServerHost.ServerInstance inst)
        {
            inst.AuthService.ClientsChanged += AuthService_ClientsChanged;
        }
    }

    public ServerHost ServerHost { get; }

    public ServerInfo? ServerInfo
    {
        get
        {
            var urls = ServerHost.CurrentInstance?.WebApp.Urls;
            return ServerHost.CurrentInstance?.AuthService.GetServerInfo(urls!);
        }
    }

    public Bitmap? ServerInfoQr => GenerateQrCode(JsonSerializer.Serialize(ServerInfo));

    public bool ServerRunning => ServerHost.IsRunning;

    public string ServerStatus => ServerRunning ? "Running" : "Stopped";

    public IList<string> Clients => ServerHost.CurrentInstance?.AuthService.Clients ?? [];

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
