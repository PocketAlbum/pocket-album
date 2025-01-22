namespace PocketAlbum.Models;

public class YearIndex
{
    public required int Year { get; init; }

    public required int Count { get; init; }

    public required uint Crc { get; init; }

    public required ulong Size { get; init; }
}
