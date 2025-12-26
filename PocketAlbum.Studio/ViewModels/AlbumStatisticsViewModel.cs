using System.Collections.Generic;
using System.Linq;
using PocketAlbum.Models;

namespace PocketAlbum.Studio.ViewModels;

public partial class AlbumStatisticsViewModel : ViewModelBase
{
    public required IAlbum Album { get; init; }

    public required MetadataModel Metadata { get; init; }

    public required List<YearIndex> YearIndex { get; init; }

    public string YearsSummary => YearIndex.Count == 1 ? 
        $"{YearIndex.First().Year} (1)" :
        $"{YearIndex.First().Year} - {YearIndex.Last().Year} ({YearIndex.Count})";

    public string TotalSize => Utilities.FormatSize(YearIndex.Sum(i => (long)i.Size));
}
