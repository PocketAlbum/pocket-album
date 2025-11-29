using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Avalonia.Media.Imaging;
using PocketAlbum.Studio.ViewModels;

namespace PocketAlbum.Studio.Views;

public class GalleryItem : INotifyPropertyChanged
{
    private Bitmap? preview;
    private bool loading;
    private readonly ObservableAlbum album;
    public int Index { get; }

    public string Text => Index.ToString();

    public Bitmap? Preview
    {
        get => preview;
        private set
        {
            preview = value;
            OnPropertyChanged();
        }
    }

    public GalleryItem(ObservableAlbum album, int index)
    {
        this.album = album;
        Index = index;
    }

    public async Task EnsureLoadedAsync()
    {
        if (loading || preview != null)
            return;

        loading = true;

        await Task.Run(async () =>
        {
            var images = await album.GetImage(Index);
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

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? prop = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}
