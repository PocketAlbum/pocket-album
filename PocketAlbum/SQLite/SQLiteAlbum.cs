using PocketAlbum.Models;
using SQLite;

namespace PocketAlbum.SQLite;

public class SQLiteAlbum : IAlbum
{
    private const int APPLICATION_ID = 0x6C416F50;
    private const string yearQuery = "CAST(substr(created, 1, 4) AS SIGNED) AS y";
    private const string hourQuery = "CAST(substr(created, 12, 2) AS SIGNED) AS h";
    private const string filterQueries = yearQuery + ", " + hourQuery;

    public SQLiteAsyncConnection Connection { get; }

    private SQLiteAlbum(SQLiteAsyncConnection connection)
    {
        Connection = connection;
    }

    public static async Task<SQLiteAlbum> Create(string path, MetadataModel metadata)
    {
        if (!path.EndsWith(".sqlite"))
        {
            throw new ArgumentException($"Filename {path} must have sqlite extension");
        }
        if (File.Exists(path))
        {
            throw new InvalidDataException($"File with at path {path} already exists");
        }
        try
        {
            metadata.Validate();
        }
        catch (Exception ex)
        {
            throw new ArgumentException("Invalid metadata provided", ex);
        }

        var db = new SQLiteAsyncConnection(path, false);
        await db.CreateTableAsync<SQLiteImage>();
        await db.CreateTableAsync<SQLiteYearIndex>();
        await db.CreateTableAsync<SQLiteMetadata>();
        await db.ExecuteAsync($"PRAGMA application_id = {APPLICATION_ID};");

        await MetadataHelper.Write(db, metadata);

        return new SQLiteAlbum(db);
    }

    public static async Task<SQLiteAlbum> Open(string path)
    {
        try
        {
            var db = new SQLiteAsyncConnection(path, false);

            var result = await db.QueryScalarsAsync<int>("PRAGMA application_id;");
            int applicationId = result.First();
            if (applicationId != APPLICATION_ID)
            {
                throw new FormatException($"Unknown application id {applicationId}");
            }

            var metadata = await MetadataHelper.Read(db);
            metadata.Validate();

            return new SQLiteAlbum(db);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Unable to open {path} as an existing Pocket Album file", ex);
        }
    }

    public async Task<MetadataModel> GetMetadata()
    {
        return await MetadataHelper.Read(Connection);
    }

    private string GetWhere(FilterModel filter) {
        if (!filter.HasAny) {
            return "TRUE";
        }
        var conditions = new List<string>();
        if (filter.Year != null) {
            if (filter.Year.SingleValue) {
                conditions.Add($"y = {filter.Year.To}");
            }
            else
            {
                conditions.Add($"y >= {filter.Year.From} AND y <= {filter.Year.To}");
            }
        }
        var time = filter.TimeOfDay switch
        {
            FilterModel.TimesOfDay.Morning => "h >= 6 AND h < 12",
            FilterModel.TimesOfDay.Afternoon => "h >= 12 AND h < 18",
            FilterModel.TimesOfDay.Evening => "h >= 18",
            FilterModel.TimesOfDay.Night => "h < 6",
            _ => null
        };
        if (time != null)
        {
            conditions.Add(time);
        }
        return string.Join(" AND ", conditions);
    }

    private class YearsQuery
    {
        [Column("COUNT(*)")]
        public int Count { get; set; }
        [Column("y")]
        public int Year { get; set; }
    }

    public async Task<AlbumInfo> GetInfo(FilterModel filter)
    {
        string where = "WHERE " + GetWhere(filter);
        
        var years = await Connection.QueryAsync<YearsQuery>(
            $"SELECT COUNT(*), {filterQueries} FROM image {where} GROUP BY y");

        return new AlbumInfo()
        {
            ImageCount = years.Sum(y => y.Count),
            DateCount = (int)await QueryNumber(
                $"SELECT COUNT(DISTINCT DATE(created)), {filterQueries} " +
                $"FROM image {where}"),
            ThumbnailsSize = await QueryNumber(
                $"SELECT SUM(LENGTH(thumbnail)), {filterQueries} " +
                $"FROM image {where}"),
            ImagesSize = await QueryNumber(
                $"SELECT SUM(LENGTH(data)), {filterQueries} " +
                $"FROM image {where}"),
            Years = years
                .Select(y => new YearIndex(){
                    Year = y.Year, Count = y.Count, Crc = 0, Size = 0
                })
                .ToList()
        };
    }

    public async Task<List<ImageThumbnail>> GetImages(FilterModel filter, Interval paging)
    {
        long count = paging.To - paging.From + 1;

        string query = "SELECT \"id\", \"filename\", \"contentType\", \"created\", " +
            "\"width\", \"height\", \"size\", \"latitude\", \"longitude\", " +
            "\"crc\", \"thumbnail\", " + filterQueries + " FROM image " +
            $"WHERE {GetWhere(filter)} " +
            "ORDER BY created ASC " +
            $"LIMIT {paging.From}, {count}";

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
        await MetadataHelper.UpdatedNow(Connection);
    }

    public async Task<bool> ImageExists(string id)
    {
        var result = await Connection.QueryScalarsAsync<int>(
            "SELECT EXISTS(SELECT 1 " +
            "FROM image " +
            $"WHERE id=\"{id}\")");
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
