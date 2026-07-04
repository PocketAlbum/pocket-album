using System.IO;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;

namespace PocketAlbum.Studio.ViewModels;

public partial class GalleryItem(ObservableAlbum album, int index) : ObservableObject
{
    [ObservableProperty]
    private Bitmap? preview;
    private bool loading;
    private readonly ObservableAlbum album = album;
    public int Index { get; } = index;
    public string Id { get; private set; }

    public string Text => Index.ToString();

    public async Task EnsureLoadedAsync()
    {
        if (loading || Preview != null)
            return;

        loading = true;

        await Task.Run(async () =>
        {
            var images = await album.GetImage(Index);
            Id = images.Info.Id;
            using var stream = new MemoryStream(images.Thumbnail);
            var bmp = new Bitmap(stream);
            Preview = bmp;
        });

        loading = false;
    }

    public void Unload()
    {
        Preview?.Dispose();
        Preview = null;
    }
}
