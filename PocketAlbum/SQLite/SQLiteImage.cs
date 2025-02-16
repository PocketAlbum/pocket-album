using SQLite;

namespace PocketAlbum.SQLite;

[Table("image")]
public class SQLiteImage
{
    [Column("id"), PrimaryKey, NotNull]
    public string? Id { get; set; }

    [Column("fileName"), NotNull]
    public string? Filename { get; set; }

    [Column("created"), NotNull]
    public DateTime Created { get; set; }

    [Column("contentType"), NotNull]
    public string? ContentType { get; set; }

    [Column("width"), NotNull]
    public int? Width { get; set; }

    [Column("height"), NotNull]
    public int? Height { get; set; }

    [Column("size"), NotNull]
    public ulong? Size { get; set; }

    [Column("latitude")]
    public double? Latitude { get; set; }

    [Column("longitude")]
    public double? Longitude { get; set; }

    [Column("thumbnail"), NotNull]
    public byte[]? Thumbnail { get; set; }

    [Column("data"), NotNull]
    public byte[]? Data { get; set; }

    [Column("crc"), NotNull]
    public uint? Crc { get; set;  }
}
