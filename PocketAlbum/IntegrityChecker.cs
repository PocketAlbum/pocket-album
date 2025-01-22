using PocketAlbum.Models;

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
    /// Calculates CRCs of all images from specific year and updates the index
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

        var crcs = images
            .OrderBy(i => i.Info.Id)
            .Select(i => (i.Info.Crc, i.Info.Size))
            .Aggregate((img1, img2) => {
                var newCrc = CrcUtilities.CombineCrc32(img1.Crc, img2.Crc, img2.Size);
                return (newCrc, img1.Size + img2.Size);
            });

        await album.StoreYearIndex(new YearIndex {
            Year = year,
            Count = images.Count,
            Crc = crcs.Crc,
            Size = crcs.Size
        });
    }
}
