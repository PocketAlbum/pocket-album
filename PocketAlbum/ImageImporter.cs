using PocketAlbum.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.Processing;
using System.IO.Hashing;
using System.Security.Cryptography;

namespace PocketAlbum;

public class ImageImporter
{
    private readonly IAlbum album;
    private readonly ImportSettings settings;
    private readonly List<int> years = new List<int>();
    
    private record SizeTarget(Size Dimensions, long Size);

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
                throw new ImageExistsException();
            }
            
            var format = await DetectFormat(stream);

            stream.Seek(0, SeekOrigin.Begin);
            using var image = Image.Load(stream);

            var exif = (image.Metadata?.ExifProfile?.Values) ?? 
                throw new ImportException("Image does not contain exif metadata");

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

            var resized = ResizeImage(image.Size);

            var data = await image
                .Clone(i => i.Resize(resized.image.Dimensions))
                .Encode(resized.image.Size);

            var thumbnail = await image
                .Clone(i => i.Resize(resized.thumbnail.Dimensions))
                .Encode(resized.thumbnail.Size);

            await album.Insert(imported, thumbnail, data);
        }
    }

    private (SizeTarget image, SizeTarget thumbnail) ResizeImage(Size originalSize)
    {
        int longerDim = Math.Max(originalSize.Width, originalSize.Height);
        int shorterDim = Math.Min(originalSize.Width, originalSize.Height);
        float ratio = (float)longerDim / shorterDim;

        float sizeScale = GetSizeScale(ratio);

        // Shrink image dimensions to target
        float targetScale = (float)settings.ImageWidth / longerDim;
        var imageSize = Math.Min(sizeScale * targetScale, 1f) * originalSize;

        targetScale = (float)settings.ThumbnailWidth / longerDim;
        var thumbnailSize = Math.Min(targetScale, 1f) * originalSize;

        return new (
            new SizeTarget((Size)imageSize, (long)(settings.ImageSize * sizeScale)), 
            new SizeTarget((Size)thumbnailSize, settings.ThumbnailSize));
    }

    private static float GetSizeScale(float ratio)
    {
        if (ratio == 2)
        {
            // Sphere 360 degree image covers full sphere compared to about 8% of average
            // regular image, therefore increase target size by 12.5 times.
            return 12.5f;
        }
        else if (ratio > 2)
        {
            // Classical panorama image, calculate scale ratio relative to average regular
            // image with ratio 1.4.
            return ratio / 1.4f;
        }
        else
        {
            // Regular image
            return 1f;
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

    public class ImageExistsException : ImportException
    {
        public ImageExistsException() : base("Image exists")
        {
        }
    }
}
