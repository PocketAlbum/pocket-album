using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace PocketAlbum.Studio.ViewModels;

public class ImportWindowViewModel : ViewModelBase
{
    public ObservableCollection<ImportItem> ImportImages { get; }

    public ImportWindowViewModel()
    {
        ImportImages = new ObservableCollection<ImportItem>(new List<ImportItem>());
    }
}