namespace PocketAlbum;

public interface IAlbum
{
    Task<AlbumInfo> GetInfo();

    Task<ImageInfo> GetImage(string id);

    Task<byte[]> GetThumbnail(string id);

    Task<byte[]> GetData(string id);

    Task<List<ImageThumbnail>> GetImages(int from, int to);

    Task<bool> ImageExists(string id);

    Task Insert(ImageInfo image, byte[] thumbnail, byte[] data);
}
