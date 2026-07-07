using System;
using System.Linq;
using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Sharpcaster;
using Sharpcaster.Models.Media;

namespace PocketAlbum.Studio.Views;

public partial class SlideshowControl : UserControl
{
    public SlideshowControl()
    {
        InitializeComponent();
    }

    public async void CastClick(object? sender, RoutedEventArgs args)
    {
        var locator = new ChromecastLocator();
        var cancellationToken = new CancellationTokenSource().Token;
        var chromecasts = await locator.FindReceiversAsync(TimeSpan.FromSeconds(5));

        if (!chromecasts.Any())
        {
            Console.WriteLine("No Chromecast devices found");
            return;
        }

        // Connect to first found device
        var chromecast = chromecasts.First();
        var client = new ChromecastClient();
        await client.ConnectChromecast(chromecast);

        await client.LaunchApplicationAsync("CC1AD845"); // Default Media Receiver

        var media = new Media
        {
            ContentUrl = "https://samplelib.com/mp4/sample-5s.mp4",
            ContentType = "video/mp4",
            Metadata = new MediaMetadata
            {
                Title = "Sample Video",
                SubTitle = "A demonstration video"
            }
        };

        var mediaStatus = await client.MediaChannel.LoadAsync(media);
        Console.WriteLine($"Media loaded: {mediaStatus.PlayerState}");
    }
}