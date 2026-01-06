using PocketAlbum.Models;

namespace PocketAlbum;

public class AlbumComparator(IAlbum album1, IAlbum album2)
{
    private readonly IAlbum album1 = album1;
    private readonly IAlbum album2 = album2;

    public async IAsyncEnumerable<Result> Compare(
        Action<double>? progress = null,
        bool ignoreIdMismatch = false)
    {
        if (!ignoreIdMismatch)
        {
            await VerifyId();
        }
        
        var years1 = await album1.GetYearIndex();
        var years2 = await album2.GetYearIndex();

        var allYears = FullOuterJoin(
            years1, years2, y => y.Year, y => y.Year, 
            (a, b, y) => new { album1 = a, album2 = b, year = y });

        var conflictYears = allYears
            .Where(y => y.album1?.Crc != y.album2?.Crc)
            .ToList();

        int i = 0;
        foreach (var year in conflictYears)
        {
            Action<double>? p = null;
            if (progress != null)
            {
                p = v => progress.Invoke((double)(v + i) / conflictYears.Count);
            }

            await foreach (var result in CompareYear(year.year, p))
            {
                yield return result;
            }
            i++;
        }
    }

    private async Task VerifyId()
    {
        var meta1 = await album1.GetMetadata();
        var meta2 = await album2.GetMetadata();

        if (meta1.Id != meta2.Id)
        {
            throw new InvalidDataException("Album id mismatch");
        }
    }

    private async IAsyncEnumerable<Result> CompareYear(int year, 
        Action<double>? progress = null)
    {
        var filter = new FilterModel(){ Year = new Interval(year) };
        HashSet<string> album1Images = new HashSet<string>();

        var info1 = await album1.GetInfo(filter);
        var info2 = await album2.GetInfo(filter);

        int total = info1.ImageCount + info2.ImageCount;

        for (int i = 0; i < info1.ImageCount; i += 1024)
        {
            progress?.Invoke((double)i / total);

            var images1 = await album1.List(filter, new Interval(i, i + 1023));
            foreach (var image1 in images1)
            {
                album1Images.Add(image1.Id);
                ImageInfo? image2 = null;
                try {
                    image2 = await album2.GetImageInfo(image1.Id);
                }
                catch { }
                yield return new Result(year, image1, image2);
            }
        }

        for (int i = 0; i < info2.ImageCount; i += 1024)
        {
            progress?.Invoke((double)(i + info1.ImageCount) / total);

            var images2 = await album2.List(filter, new Interval(i, i + 1023));
            foreach (var image2 in images2.Where(i => !album1Images.Contains(i.Id)))
            {
                ImageInfo? image1 = null;
                try {
                    image1 = await album1.GetImageInfo(image2.Id);
                }
                catch { }
                yield return new Result(year, image1, image2);
            }
        }
    }

    public async Task Resolve(Result result)
    {
        ImageInfo info = result.Image1 ?? result.Image2!;
        IAlbum srcAlbum = album1;
        IAlbum dstAlbum = album2;
        if (result.Image1 == null)
        {
            srcAlbum = album2;
            dstAlbum = album1;
        }
        var thumb = await srcAlbum.GetImageThumbnail(info.Id);
        var data = await srcAlbum.GetImageData(info.Id);

        await dstAlbum.Insert(info, thumb, data);
    }

    public class Result(int year, ImageInfo? image1, ImageInfo? image2)
    {
        public int Year { get; } = year;
        public ImageInfo? Image1 { get; } = image1;
        public ImageInfo? Image2 { get; } = image2;

        public bool Conflict => Image1 == null || Image2 == null;
    }

    internal static IList<TR> FullOuterJoin<TA, TB, TK, TR>(
        IEnumerable<TA> a,
        IEnumerable<TB> b,
        Func<TA, TK> selectKeyA, 
        Func<TB, TK> selectKeyB,
        Func<TA, TB, TK, TR> projection,
        TA? defaultA = default, 
        TB? defaultB = default,
        IEqualityComparer<TK>? cmp = null)
    {
        cmp ??= EqualityComparer<TK>.Default;
        var alookup = a.ToLookup(selectKeyA, cmp);
        var blookup = b.ToLookup(selectKeyB, cmp);
 
        var keys = new HashSet<TK>(alookup.Select(p => p.Key), cmp);
        keys.UnionWith(blookup.Select(p => p.Key));
 
        var join = from key in keys
                   from xa in alookup[key].DefaultIfEmpty(defaultA)
                   from xb in blookup[key].DefaultIfEmpty(defaultB)
                   select projection(xa, xb, key);
 
        return join.ToList();
    }
}