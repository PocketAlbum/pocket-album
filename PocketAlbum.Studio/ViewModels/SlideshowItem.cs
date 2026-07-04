using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PocketAlbum.Studio.ViewModels;

public partial class SlideshowItem(IAlbum album, string id) : ObservableObject
{
    [ObservableProperty]
    private Bitmap? image;
    private bool loading;
    private readonly IAlbum album = album;
    public string Id { get; } = id;

    public async Task EnsureLoadedAsync()
    {
        if (loading || Image != null)
            return;

        loading = true;

        await Task.Run(async () =>
        {
            var data = await album.GetImageData(Id);
            using var stream = new MemoryStream(data);
            Image = new Bitmap(stream);
        });

        loading = false;
    }

    public void Unload()
    {
        Image?.Dispose();
        Image = null;
    }
}
