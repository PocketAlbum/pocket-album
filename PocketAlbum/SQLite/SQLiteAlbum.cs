using SQLite;

namespace PocketAlbum.SQLite;

public class SQLiteAlbum : IAlbum
{
    public SQLiteAsyncConnection Connection { get; }

    private SQLiteAlbum(SQLiteAsyncConnection connection)
    {
        Connection = connection;
    }

    public static async Task<SQLiteAlbum> Open(string path)
    {
        var db = new SQLiteAsyncConnection(path, false);
        await db.CreateTableAsync<SQLiteImage>();
        return new SQLiteAlbum(db);
    }

    public async Task<AlbumInfo> GetInfo()
    {
        return new AlbumInfo()
        {
            ImageCount = await Connection.Table<SQLiteImage>().CountAsync(),
            DateCount = (int)await QueryNumber(
                "SELECT COUNT(DISTINCT DATE(Created)) FROM Image"),
            ThumbnailsSize = await QueryNumber(
                "SELECT SUM(LENGTH(Thumbnail)) FROM Image"),
            ImagesSize = await QueryNumber(
                "SELECT SUM(LENGTH(Data)) FROM Image")
        };
    }

    public async Task<List<ImageThumbnail>> GetImages(int from, int to)
    {
        if (to < from)
        {
            throw new ArgumentException("From index must be lower than to");
        }

        var images = await Connection.QueryAsync<SQLiteImage>(
            "SELECT \"Id\", \"Filename\", \"Created\", \"Width\", " +
            "\"Height\", \"Size\", \"Latitude\", \"Longitude\", " +
            "\"Checksum\", \"Thumbnail\"" +
            "FROM Image " +
            "ORDER BY Created DESC " +
            $"LIMIT {from}, {to - from + 1}");

        return images.Select(i => new ImageThumbnail()
        {
            Info = ConvertImage(i),
            Thumbnail = i.Thumbnail!
        }).ToList();
    }

    public async Task<byte[]> GetData(string id)
    {
        var image = await Connection.Table<SQLiteImage>()
            .FirstAsync(x => x.Id == id);
        return image.Data!;
    }

    private static ImageInfo ConvertImage(SQLiteImage sqliteImage)
    {
        return new ImageInfo()
        {
            Id = sqliteImage.Id!,
            Filename = sqliteImage.Filename!,
            Created = sqliteImage.Created,
            Width = sqliteImage.Width!.Value,
            Height = sqliteImage.Height!.Value,
            Size = sqliteImage.Size!.Value,
            Latitude = sqliteImage.Latitude,
            Longitude = sqliteImage.Longitude
        };
    }

    public async Task Insert(ImageInfo image, byte[] thumbnail, byte[] data)
    {
        await Connection.InsertAsync(new SQLiteImage()
        {
            Id = image.Id,
            Filename = image.Filename,
            Created = image.Created,
            Width = image.Width,
            Height = image.Height,
            Size = image.Size,
            Latitude = image.Latitude,
            Longitude = image.Longitude,
            Thumbnail = thumbnail,
            Data = data
        });
    }

    public async Task<bool> ImageExists(string id)
    {
        var result = await Connection.QueryScalarsAsync<int>(
            "SELECT EXISTS(SELECT 1 " +
            "FROM Image " +
            $"WHERE Id=\"{id}\")");
        return result.Count == 1 && result[0] == 1;
    }

    private async Task<long> QueryNumber(string query)
    {
        var q = await Connection.QueryScalarsAsync<string>(query);
        if (q.Count == 0)
        {
            return 0;
        }
        return Convert.ToInt64(q.FirstOrDefault());
    }
}
