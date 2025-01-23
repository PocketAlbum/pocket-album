using PocketAlbum.Models;

namespace PocketAlbum;

public interface IAlbum
{
    Task<AlbumInfo> GetInfo();

    Task<byte[]> GetData(string id);

    Task<List<ImageThumbnail>> GetImages(FilterModel filter);

    Task<bool> ImageExists(string id);

    Task Insert(ImageInfo image, byte[] thumbnail, byte[] data);

    Task<List<YearIndex>> GetYearIndex();

    Task StoreYearIndex(YearIndex yearIndex);

    Task RemoveYearIndex(int year);
}
