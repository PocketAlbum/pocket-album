using SQLite;

namespace PocketAlbum.SQLite;

[Table("index")]
internal class SQLiteYearIndex
{
    [Column("year"), PrimaryKey, NotNull]
    public int? Year { get; set; }

    [Column("count"), NotNull]
    public int? Count { get; set; }

    [Column("crc"), NotNull]
    public uint? Crc { get; set; }

    [Column("size"), NotNull]
    public ulong? Size { get; set; } 
}
