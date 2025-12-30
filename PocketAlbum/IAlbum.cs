using PocketAlbum.Models;

namespace PocketAlbum;

public interface IAlbum : IAsyncDisposable
{
    Task<MetadataModel> GetMetadata();

    Task SetMetadata(MetadataModel metadata);

    Task<AlbumInfo> GetInfo(FilterModel filter);

    Task<byte[]> GetData(string id);

    Task<List<ImageThumbnail>> GetImages(FilterModel filter, Interval paging);

    Task<bool> ImageExists(string id);

    Task Insert(ImageInfo image, byte[] thumbnail, byte[] data);

    Task<List<YearIndex>> GetYearIndex();

    Task StoreYearIndex(YearIndex yearIndex);

    Task RemoveYearIndex(int year);
}
