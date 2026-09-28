using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using PocketAlbum.Models;
using Sharpcaster;
using Sharpcaster.Models;
using Sharpcaster.Models.Media;
using static PocketAlbum.Server.ServerHost;

namespace PocketAlbum.Studio.Services;

public partial class CastingService(ServerService serverService) : ObservableObject
{
    Guid? shareId;
    private readonly Dictionary<ChromecastReceiver, ChromecastClient> clients = [];
    public IEnumerable<ChromecastClient> ConnectedClients => clients.Values;

    public bool IsConnected => clients.Any();

    public Func<Task>? ChromecastSelectionCallback;

    public async Task Cast(ImageInfo image, byte[] data)
    {
        var server = await GetServer();
            
        if (shareId != null)
        {
            server.PublicService.Unshare(shareId.Value);
        }
        shareId = server.PublicService.Share(data);

        var info = server.AuthService.GetServerInfo(server.WebApp.Urls);
        var url = $"{info.Endpoints.First()}/api/public/{shareId}";

        if (!clients.Any())
        {
            await ChromecastSelectionCallback?.Invoke();
        }

        var media = new Media
        {
            ContentUrl = url,
            ContentType = "image/jpeg",
            Metadata = new MediaMetadata
            {
                Title = image.Created.ToString(),
                SubTitle = image.Filename
            }
        };

        foreach (var client in clients.Values)
        {
            await client.MediaChannel.LoadAsync(media);
        }
    }

    private async Task<ServerInstance> GetServer()
    {
        var host = serverService.Host ?? await serverService.StartServer();
        return host.CurrentInstance ?? 
            throw new InvalidCastException("Instance is null");
    }

    public async Task<IEnumerable<ChromecastReceiver>> FindReceivers()
    {
        var locator = new ChromecastLocator();
        return await locator.FindReceiversAsync(TimeSpan.FromSeconds(5));
    }

    public async Task ConnectToChromecast(ChromecastReceiver receiver)
    {
        var existing = clients.Keys.FirstOrDefault(r => r.DeviceUri == receiver.DeviceUri);
        if (existing != null)
        {
            return; // Already connected
        }

        var client = new ChromecastClient();
        await client.ConnectChromecast(receiver);
        await client.LaunchApplicationAsync("CC1AD845"); // Default Media Receiver
        client.Disconnected += ClientDisconnected;
        clients[receiver] = client;
    }

    private void ClientDisconnected(object? sender, EventArgs e)
    {
        var receiver = clients
            .FirstOrDefault(c => c.Value == sender as ChromecastClient)
            .Key;
        clients.Remove(receiver);
    }
}