namespace PocketAlbum.Models;

public class AlbumInfo
{
    public int ImageCount { get; init; }
    public int DateCount { get; init; }
    public long ThumbnailsSize { get; init; }
    public long ImagesSize { get; init; }
    public required List<YearIndex> Years { get; init; }
}
