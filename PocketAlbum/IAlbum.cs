using PocketAlbum.Models;

namespace PocketAlbum;

public interface IAlbum : IAsyncDisposable
{
    Task<MetadataModel> GetMetadata();

    Task SetMetadata(MetadataModel metadata);

    Task<AlbumInfo> GetInfo(FilterModel filter);

    Task<ImageInfo> GetImageInfo(string id);

    Task<byte[]> GetImageData(string id);

    Task<byte[]> GetImageThumbnail(string id);

    Task<List<ImageInfo>> List(FilterModel filter, Interval paging);

    Task<List<ImageThumbnail>> ListThumbnails(FilterModel filter, Interval paging);

    Task<bool> ImageExists(string id);

    Task Insert(ImageInfo image, byte[] thumbnail, byte[] data);

    Task<List<YearIndex>> GetYearIndex();

    Task StoreYearIndex(YearIndex yearIndex);

    Task RemoveYearIndex(int year);
}
