using System.Threading.Tasks;
using PocketAlbum.Models;

namespace PocketAlbum.Studio.ViewModels;

public class YearStatistics
{
    public required int Year { get; init; }

    public required int Count { get; init; }

    public required uint Crc { get; init; }

    public required ulong FullSize { get; init; }

    public ulong CompressedSize { get; set; }

    public ulong ThumbnailsSize { get; set; }

    public static async Task<YearStatistics> FromIndex(YearIndex index, IAlbum album)
    {
        var filter = new FilterModel(){ Year = new Interval(index.Year) };
        var info = await album.GetInfo(filter);
        return new YearStatistics()
        {
            Year = index.Year,
            Count = index.Count,
            Crc = index.Crc,
            FullSize = index.Size,
            CompressedSize = (ulong)info.ImagesSize,
            ThumbnailsSize = (ulong)info.ThumbnailsSize
        };
    }
}