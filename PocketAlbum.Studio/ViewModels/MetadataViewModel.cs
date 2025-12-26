using CommunityToolkit.Mvvm.ComponentModel;

namespace PocketAlbum.Studio.ViewModels;

public partial class MetadataViewModel : ViewModelBase
{
    [ObservableProperty]
    private string title = "Untitled";

    [ObservableProperty]
    private string? description;
}
