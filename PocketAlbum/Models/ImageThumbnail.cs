namespace PocketAlbum.Models;

public class ImageThumbnail
{
    public required ImageInfo Info { get; init; }

    public required byte[] Thumbnail { get; init; }
}
