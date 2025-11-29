using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using PocketAlbum.Studio.Views;

namespace PocketAlbum.Studio.ViewModels;

public class GalleryViewModel : INotifyPropertyChanged
{
    private IReadOnlyList<GalleryItem>? images;
    public IReadOnlyList<GalleryItem>? Images { 
        get => images; 
        private set
        {
            images = value;
            OnPropertyChanged();
        }
    }

    public async Task OpenAlbum(IAlbum album)
    {
        Images = await ObservableAlbum.FromAlbum(album, new Models.FilterModel());
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? prop=null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
}