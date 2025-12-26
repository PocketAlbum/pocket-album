using PocketAlbum.Models;

namespace PocketAlbum;

public class IntegrityChecker(IAlbum album)
{
    private readonly IAlbum album = album;

    public async Task CheckAllYears(Action<double>? progress = null)
    {
        var invalidYears = await InvalidYears(p => progress?.Invoke(p * 0.1));
        await CheckYears(invalidYears, p => progress?.Invoke(0.1 + (p * 0.9)));
    }

    private async Task CheckYears(List<int> years, Action<double>? progress = null)
    {
        for (int i = 0; i < years.Count; i++)
        {
            progress?.Invoke((double)i / years.Count);
            await CheckYear(years[i]);
        }
    }

    /// <summary>
    /// Perform a quick integrity check, returning years which failed verification.
    /// Values received as return value should be passed to function CheckYear to check
    /// each year and fix the index.
    /// </summary>
    /// <returns>list of years for which integrity check failed</returns>
    public async Task<List<int>> InvalidYears(Action<double>? progress = null)
    {
        var info = await album.GetInfo(new FilterModel());
        var index = await album.GetYearIndex();

        List<int> invalidYears = new List<int>();
        int cnt = 0;
        foreach (var yearMap in info.Years)
        {
            progress?.Invoke((double)cnt / info.Years.Count);
            cnt++;

            var i = index.FirstOrDefault(y => y.Year == yearMap.Year);
            if (i == null || i.Count != yearMap.Count)
            {
                invalidYears.Add(yearMap.Year);
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
        var filter = new FilterModel() { Year = new Interval(year) };
        var info = await album.GetInfo(filter);
        var index = info.Years.FirstOrDefault(y => y.Year == year);

        if (index == null || index.Count == 0)
        {
            // There are no images from this year, remove the index
            await album.RemoveYearIndex(year);
            return;
        }

        var images = await album.GetImages(filter, new Interval(0, index.Count));

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
