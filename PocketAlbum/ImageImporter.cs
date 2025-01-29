using PocketAlbum.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using System.IO;
using System.IO.Hashing;
using System.Security.Cryptography;

namespace PocketAlbum;

public class ImageImporter
{
    private readonly IAlbum album;
    private readonly ImportSettings settings;
    private readonly List<int> years = new List<int>();

    public ImageImporter(IAlbum album, ImportSettings? settings = null)
    {
        this.album = album;
        this.settings = settings ?? new ImportSettings();
    }

    private static async Task<IImageFormat> DetectFormat(Stream stream)
    {
        try
        {
            stream.Seek(0, SeekOrigin.Begin);
            return await Image.DetectFormatAsync(stream);
        }
        catch (InvalidImageContentException)
        {
            throw new ImportException("Invalid image content");
        }
        catch (UnknownImageFormatException)
        {
            throw new ImportException("Unknown image format");
        }
    }

    public async Task Import(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open))
        {
            var shaBytes = await SHA256.HashDataAsync(stream);
            var hash = Utilities.ByteArrayToString(shaBytes);
            
            if (await album.ImageExists(hash))
            {
                throw new ImportException("Image already exists");
            }
            
            var format = await DetectFormat(stream);

            stream.Seek(0, SeekOrigin.Begin);
            using (var image = Image.Load(stream))
            {
                var exif = image.Metadata?.ExifProfile?.Values;

                if (exif == null)
                {
                    throw new ImportException("Image does not contain exif metadata");
                }

                var crc = new Crc32();
                stream.Seek(0, SeekOrigin.Begin);
                crc.Append(stream);

                var coordinates = TryParseCoordinates(exif);

                var imported = new Models.ImageInfo()
                {
                    Id = hash,
                    Filename = Path.GetFileName(path),
                    ContentType = format.DefaultMimeType,
                    Created = exif.GetCreated(),
                    Width = image.Size.Width,
                    Height = image.Size.Height,
                    Size = (ulong)stream.Length,
                    Latitude = coordinates?.lat,
                    Longitude = coordinates?.lon,
                    Crc = crc.GetCurrentHashAsUInt32()
                };

                if (!years.Contains(imported.Created.Year))
                {
                    await album.RemoveYearIndex(imported.Created.Year);
                    years.Add(imported.Created.Year);
                }

                image.Mutate(i => i.AutoOrient());

                var data = await image
                    .Clone(i => i.Resize(settings.ImageWidth, 0)
                    .AutoOrient())
                    .Encode(settings.ImageSize);

                var thumbnail = await image
                    .Clone(i => i.Resize(settings.ThumbnailWidth, 0)
                    .AutoOrient())
                    .Encode(settings.ThumbnailSize);

                await album.Insert(imported, thumbnail, data);
            }
        }
    }
 
    private (double lat, double lon)? TryParseCoordinates(IReadOnlyList<IExifValue> exif)
    {
        var lat = exif.TryGetCoordinate("GPSLatitude");
        var lon = exif.TryGetCoordinate("GPSLongitude");

        return lat != null && lon != null ? (lat.Value, lon.Value) : null;
    }

    public class ImportException : Exception
    {
        public ImportException(string? message) : base(message)
        {
        }
    }
}
