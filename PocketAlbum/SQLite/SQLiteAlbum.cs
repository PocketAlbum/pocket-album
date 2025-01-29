using PocketAlbum.Models;
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
        await db.CreateTableAsync<SQLiteYearIndex>();
        return new SQLiteAlbum(db);
    }

    public async Task<AlbumInfo> GetInfo()
    {
        var years = await Connection.QueryScalarsAsync<int>(
            "SELECT DISTINCT substr(\"Created\", 1, 4) FROM Image");
        
        return new AlbumInfo()
        {
            ImageCount = await Connection.Table<SQLiteImage>().CountAsync(),
            DateCount = (int)await QueryNumber(
                "SELECT COUNT(DISTINCT DATE(Created)) FROM Image"),
            ThumbnailsSize = await QueryNumber(
                "SELECT SUM(LENGTH(Thumbnail)) FROM Image"),
            ImagesSize = await QueryNumber(
                "SELECT SUM(LENGTH(Data)) FROM Image"),
            Years = years
        };
    }

    public async Task<List<ImageThumbnail>> GetImages(FilterModel filter)
    {
        if (!filter.Valid)
        {
            throw new ArgumentException(
                "At least one property of filter model must be filled");
        }

        string query = "SELECT \"Id\", \"Filename\", \"ContentType\", \"Created\", " +
            "\"Width\", \"Height\", \"Size\", \"Latitude\", \"Longitude\", " +
            "\"Crc\", \"Thumbnail\" " +
            "FROM Image ";

        if (filter.Year != null)
        {
            if (filter.Year.SingleValue)
            {
                query += $"WHERE \"Created\" LIKE '{filter.Year.To}-%' ";
            }
            else
            {
                query += $"WHERE substr(\"Created\", 1, 4) >= {filter.Year.From} ";
                query += $"AND substr(\"Created\", 1, 4) <= {filter.Year.To} ";
            }
        }

        query += "ORDER BY Created ASC ";

        if (filter.Index != null)
        {
            long count = filter.Index.To - filter.Index.From + 1;
            query += $"LIMIT {filter.Index.From}, {count}";
        }

        var images = await Connection.QueryAsync<SQLiteImage>(query);

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
            ContentType = sqliteImage.ContentType!,
            Created = sqliteImage.Created,
            Width = sqliteImage.Width!.Value,
            Height = sqliteImage.Height!.Value,
            Size = sqliteImage.Size!.Value,
            Latitude = sqliteImage.Latitude,
            Longitude = sqliteImage.Longitude,
            Crc = sqliteImage.Crc!.Value
        };
    }

    public async Task Insert(ImageInfo image, byte[] thumbnail, byte[] data)
    {
        await Connection.InsertAsync(new SQLiteImage()
        {
            Id = image.Id,
            Filename = image.Filename,
            ContentType = image.ContentType,
            Created = image.Created,
            Width = image.Width,
            Height = image.Height,
            Size = image.Size,
            Latitude = image.Latitude,
            Longitude = image.Longitude,
            Thumbnail = thumbnail,
            Data = data,
            Crc = image.Crc
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

    public async Task StoreYearIndex(YearIndex yearIndex)
    {
        await Connection.InsertAsync(new SQLiteYearIndex()
        {
            Year = yearIndex.Year,
            Count = yearIndex.Count,
            Crc = yearIndex.Crc,
            Size = yearIndex.Size
        });
    }

    public async Task<List<YearIndex>> GetYearIndex()
    {
        var index = await Connection.Table<SQLiteYearIndex>().ToListAsync();

        return index.Select(i => new YearIndex()
        {
            Year = i.Year!.Value,
            Count = i.Count!.Value,
            Crc = i.Crc!.Value,
            Size = i.Size!.Value
        }).ToList();
    }

    public async Task RemoveYearIndex(int year)
    {
        await Connection.DeleteAsync<SQLiteYearIndex>(year);
    }
}
