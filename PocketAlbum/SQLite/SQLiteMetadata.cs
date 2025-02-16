using SQLite;

namespace PocketAlbum.SQLite;

[Table("meta")]
public class SQLiteMetadata
{
    [Column("key"), PrimaryKey, NotNull]
    public string? Key { get; set; }

    [Column("value"), NotNull]
    public string? Value { get; set; }
}
