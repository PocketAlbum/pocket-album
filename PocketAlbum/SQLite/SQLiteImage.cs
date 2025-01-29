using SQLite;

namespace PocketAlbum.SQLite;

[Table("Image")]
public class SQLiteImage
{
    [PrimaryKey, NotNull]
    public string? Id { get; set; }

    [NotNull]
    public string? Filename { get; set; }

    [NotNull]
    public DateTime Created { get; set; }

    [NotNull]
    public string? ContentType { get; set; }

    [NotNull]
    public int? Width { get; set; }

    [NotNull]
    public int? Height { get; set; }

    [NotNull]
    public ulong? Size { get; set; }

    public double? Latitude { get; set; }

    public double? Longitude { get; set; }

    [NotNull]
    public byte[]? Thumbnail { get; set; }

    [NotNull]
    public byte[]? Data { get; set; }

    [NotNull]
    public uint? Crc { get; set;  }
}
