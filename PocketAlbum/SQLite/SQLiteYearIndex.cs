using SQLite;

namespace PocketAlbum.SQLite;

[Table("YearIndex")]
internal class SQLiteYearIndex
{
    [PrimaryKey, NotNull]
    public int? Year { get; set; }

    [NotNull]
    public int? Count { get; set; }

    [NotNull]
    public string? Checksum { get; set; }
}
