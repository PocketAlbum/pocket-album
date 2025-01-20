namespace PocketAlbum.Models;

public class YearIndex
{
    public required int Year { get; init; }

    public required int Count { get; init; }

    public required string Checksum { get; init; }
}
