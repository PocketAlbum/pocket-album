using PocketAlbum.Models;
using System.Security.Cryptography;
using System.Text;

namespace PocketAlbum;

public class IntegrityChecker
{
    private IAlbum album;

    public IntegrityChecker(IAlbum album)
    {
        this.album = album;
    }

    /// <summary>
    /// Perform a quick integrity check, returning years which failed verification.
    /// Values received as return value should be passed to function CheckYear to check
    /// each year and fix the index.
    /// </summary>
    /// <returns>list of years for which integrity check failed</returns>
    public async Task<List<int>> InvalidYears()
    {
        var info = await album.GetInfo();
        var index = await album.GetYearIndex();

        List<int> invalidYears = new List<int>();
        foreach (var year in info.Years)
        {
            var i = index.FirstOrDefault(y => y.Year == year);
            if (i == null)
            {
                invalidYears.Add(year);
            }
            else
            {
                index.Remove(i);
            }
        }

        invalidYears.AddRange(index.Select(y => y.Year));

        return invalidYears.Order().ToList();
    }

    /// <summary>
    /// Calculates checksum of all images from specific year and updates the index
    /// </summary>
    /// <param name="year">year</param>
    /// <returns></returns>
    public async Task CheckYear(int year)
    {
        var images = await album.GetImages(year);

        if (images.Count == 0)
        {
            // There are no images from this year, remove the index
            await album.RemoveYearIndex(year);
            return;
        }

        var checksums = new MemoryStream(GenerateChecksums(images.Select(i => i.Info)));
        var yearChecksumBytes = await SHA256.HashDataAsync(checksums);
        var yearChecksum = Utilities.ByteArrayToString(yearChecksumBytes);

        await album.StoreYearIndex(new YearIndex {
            Year = year,
            Count = images.Count,
            Checksum = yearChecksum
        });
    }

    private byte[] GenerateChecksums(IEnumerable<ImageInfo> images)
    {
        StringBuilder sb = new StringBuilder();
        foreach (var img in images.OrderBy(i => i.Filename))
        {
            sb.Append(img.Id);
            sb.Append("  ");
            sb.Append(img.Filename);
            sb.Append('\n');
        }
        return Encoding.ASCII.GetBytes(sb.ToString());
    }
}
