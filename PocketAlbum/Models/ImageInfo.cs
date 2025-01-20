namespace PocketAlbum.Models;

public class ImageInfo
{
    public required string Id { get; init; }

    public required string Filename { get; init; }

    public required DateTime Created { get; init; }

    public required int Width { get; init; }

    public required int Height { get; init; }

    public required long Size { get; init; }

    public double? Latitude { get; init; }

    public double? Longitude { get; init; }

}
