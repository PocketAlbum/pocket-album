using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using PocketAlbum.Models;

namespace PocketAlbum.Studio.ViewModels;

public partial class AlbumStatisticsViewModel : ViewModelBase
{
    public required IAlbum Album { get; init; }

    public required MetadataModel Metadata { get; init; }

    public required List<YearStatistics> Years { get; init; }

    public int TotalPhotos => Years.Sum(i => i.Count);

    public string YearsSummary => Years.Count == 1 ? 
        $"{Years.First().Year} (1)" :
        $"{Years.First().Year} - {Years.Last().Year} ({Years.Count})";

    public ulong TotalSize => (ulong)Years.Sum(i => (long)i.FullSize);
    public ulong CompressedSize => (ulong)Years.Sum(i => (long)i.CompressedSize);
    public ulong ThumbnailsSize => (ulong)Years.Sum(i => (long)i.ThumbnailsSize);

    public async static Task<AlbumStatisticsViewModel> FromAlbum(IAlbum album)
    {
        var index = await album.GetYearIndex();
        List<YearStatistics> years = new List<YearStatistics>();
        foreach (var i in index)
        {
            years.Add(await YearStatistics.FromIndex(i, album));
        }

        return new AlbumStatisticsViewModel() {
            Album = album,
            Metadata = await album.GetMetadata(),
            Years = years
        };
    }
}
