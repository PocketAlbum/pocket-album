using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using System.Globalization;

namespace PocketAlbum;

internal static class ImageSharpExtensions
{
    public static async Task<byte[]> Encode(this Image source, long maxSize)
    {
        for (int quality = 80; quality >= 10; quality -= 10)
        {
            var encoder = new JpegEncoder()
            {
                Quality = quality,
                SkipMetadata = true
            };
            using (var stream = new MemoryStream())
            {
                await source.SaveAsJpegAsync(stream, encoder);
                if (stream.Length < maxSize || quality == 10)
                {
                    return stream.ToArray();
                }
            }
        }
        throw new Exception("Unable to compress the image to desired size");
    }

    public static double? TryGetCoordinate(this IReadOnlyList<IExifValue> exif,
        string name)
    {
        var value = exif
                    .FirstOrDefault(e => e.Tag.ToString() == name)?
                    .GetValue();
        var refValue = exif
                    .FirstOrDefault(e => e.Tag.ToString() == name + "Ref")?
                    .GetValue();
        if (value == null || refValue == null)
        {
            return null;
        }
        if (value is Rational[] rationalArray && refValue is string refString)
        {
            decimal result = 0;
            for (int i = 0; i < Math.Min(3, rationalArray.Length); i++)
            {
                decimal part = (decimal)rationalArray[i].Numerator 
                    / rationalArray[i].Denominator;
                result += part / (decimal)Math.Pow(60, i);
            }
            if (refString.Equals("S", StringComparison.InvariantCultureIgnoreCase) ||
                refString.Equals("W", StringComparison.InvariantCultureIgnoreCase))
            {
                result *= -1;
            }
            return (double)result;
        }
        return null;
    }

    public static DateTime GetCreated(this IReadOnlyCollection<IExifValue> exif)
    {
        var value = exif
            .FirstOrDefault(e => e.Tag.ToString() == "DateTimeOriginal")?
            .GetValue();
        if (value == null)
        {
            throw new Exception(
                "Image doesn't contains creation date and time");
        }
        return DateTime.ParseExact(value.ToString()!, 
            "yyyy:MM:dd HH:mm:ss",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None);
    }
}
