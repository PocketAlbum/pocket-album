using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using System.Security.Cryptography;
using System.Text;

namespace PocketAlbum;

public class ImageImporter
{
    private readonly IAlbum album;
    private readonly ImportSettings settings;

    public ImageImporter(IAlbum album, ImportSettings? settings = null)
    {
        this.album = album;
        this.settings = settings ?? new ImportSettings();
    }

    public async Task Import(string path)
    {
        using (var stream = new FileStream(path, FileMode.Open))
        {
            var shaBytes = await SHA256.HashDataAsync(stream);
            var checksum = ByteArrayToString(shaBytes);

            if (await album.ImageExists(checksum))
            {
                throw new ImportException("Image already exists");
            }

            try
            {
                stream.Seek(0, SeekOrigin.Begin);
                var imageInfo = Image.Identify(stream);
            }
            catch (InvalidImageContentException)
            {
                throw new ImportException("Invalid image content");
            }
            catch (UnknownImageFormatException)
            {
                throw new ImportException("Unknown image format");
            }

            stream.Seek(0, SeekOrigin.Begin);
            using (var image = Image.Load(stream))
            {
                var exif = image.Metadata?.ExifProfile?.Values;

                if (exif == null)
                {
                    throw new ImportException("Image does not contain exif metadata");
                }

                var coordinates = TryParseCoordinates(exif);

                var imported = new ImageInfo()
                {
                    Id = checksum,
                    Filename = Path.GetFileName(path),
                    Created = exif.GetCreated(),
                    Width = image.Size.Width,
                    Height = image.Size.Height,
                    Size = stream.Length,
                    Latitude = coordinates?.lat,
                    Longitude = coordinates?.lon
                };

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

    private static string ByteArrayToString(byte[] array)
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < array.Length; i++)
        {
            builder.Append($"{array[i]:X2}");
        }
        return builder.ToString();
    }

    public class ImportException : Exception
    {
        public ImportException(string? message) : base(message)
        {
        }
    }
}
