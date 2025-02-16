using PocketAlbum.Models;
using SQLite;

namespace PocketAlbum.SQLite;

internal class MetadataHelper
{
    internal static async Task<MetadataModel> Read(SQLiteAsyncConnection db)
    {
        var rows = await db.Table<SQLiteMetadata>().ToListAsync();
        return new MetadataModel()
        {
            Id = Get(rows, "id", Guid.NewGuid()),
            Version = Get(rows, "version", string.Empty),
            Name = Get(rows, "name", string.Empty),
            Description = Get(rows, "description", string.Empty),
            Created = Get(rows, "created", DateTime.Now),
            Updated = Get(rows, "updated", DateTime.Now)
        };
    }

    internal static async Task Write(SQLiteAsyncConnection db, MetadataModel metadata)
    {
        await Put(db, "id", metadata.Id.ToString());
        await Put(db, "version", metadata.Version);
        await Put(db, "name", metadata.Name);
        await Put(db, "description", metadata.Description ?? "");
        await Put(db, "created", metadata.Created);
        await Put(db, "updated", metadata.Updated);
    }

    internal static async Task UpdatedNow(SQLiteAsyncConnection db)
    {
        await Put(db, "updated", DateTime.Now);
    }

    private static T Get<T>(List<SQLiteMetadata> rows, string key, T defaultValue)
    {
        var row = rows.FirstOrDefault(r => r.Key == key);
        if (row == null || row.Value == null)
        {
            return defaultValue;
        }

        try
        {
            if (typeof(T) == typeof(string))
            {
                return (T)(object)row.Value;
            }
            else if (typeof(T) == typeof(int))
            {
                return (T)(object)int.Parse(row.Value);
            }
            else if (typeof(T) == typeof(Guid))
            {
                return (T)(object)Guid.Parse(row.Value);
            }
            else if (typeof(T) == typeof(DateTime))
            {
                return (T)(object)DateTime.Parse(row.Value);
            }
        }
        catch
        {
            return defaultValue;
        }
        throw new NotImplementedException("Unsupported metadata type " + typeof(T));
    }

    private static async Task Put(SQLiteAsyncConnection db, string key,
        string value)
    {
        await db.InsertOrReplaceAsync(new SQLiteMetadata { Key = key, Value = value });
    }

    private static async Task Put(SQLiteAsyncConnection db, string key,
        DateTime value)
    {
        await db.InsertOrReplaceAsync(new SQLiteMetadata {
            Key = key, Value = value.ToString("yyyy-MM-ddTHH:mm:ss.fff")
        });
    }
}
