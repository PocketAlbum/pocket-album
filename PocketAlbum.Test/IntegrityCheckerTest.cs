using Moq;
using PocketAlbum.Models;

namespace PocketAlbum.Test;

public class IntegrityCheckerTest
{
    [Fact]
    public async Task InvalidYearsTest()
    {
        var album = new Mock<IAlbum>();
        album
            .Setup(_ => _.GetInfo(It.IsAny<FilterModel>()))
            .Returns(Task.FromResult(new AlbumInfo()
            {
                ImageCount = 5,
                DateCount = 3,
                ThumbnailsSize = 500,
                ImagesSize = 5000,
                Years = [
                    new YearIndex()
                    {
                        Year = 2000,
                        Count = 2,
                        Crc = 0,
                        Size = 0
                    },
                    new YearIndex()
                    {
                        Year = 2001,
                        Count = 4,
                        Crc = 0,
                        Size = 0
                    },
                    new YearIndex()
                    {
                        Year = 2002,
                        Count = 4,
                        Crc = 0,
                        Size = 0
                    }
                ]
            }));
        album
            .Setup(_ => _.GetYearIndex())
            .Returns(Task.FromResult(new List<YearIndex>()
            {
                new YearIndex()
                {
                    Year = 2000,
                    Count = 2,
                    Crc = 0x123456,
                    Size = 2000
                },
                new YearIndex()
                {
                    Year = 2001,
                    Count = 3,
                    Crc = 0x654321,
                    Size = 3000
                },
                new YearIndex()
                {
                    Year = 2003,
                    Count = 1,
                    Crc = 0x123123,
                    Size = 1000
                }
            }));

        var years = await IntegrityChecker.InvalidYears(album.Object);
        Assert.Equal([2001, 2002, 2003], years);
    }

    [Fact]
    public async Task CheckYearTest()
    {
        var album = new Mock<IAlbum>();
        album
            .Setup(_ => _.GetInfo(It.IsAny<FilterModel>()))
            .Returns(Task.FromResult(new AlbumInfo()
            {
                ImageCount = 2,
                DateCount = 1,
                ThumbnailsSize = 200,
                ImagesSize = 2000,
                Years = [
                    new YearIndex()
                    {
                        Year = 2001,
                        Count = 2,
                        Crc = 0,
                        Size = 0
                    }
                ]
            }));
        album
            .Setup(_ => _.List(It.IsAny<FilterModel>(), It.IsAny<Interval>()))
            .Returns(Task.FromResult(new List<ImageInfo>()
            {
                new ImageInfo()
                {
                    Id = "abc",
                    Filename = "abc.jpg",
                    ContentType = "image/jpeg",
                    Created = new DateTime(2001, 1, 1),
                    Width = 4000,
                    Height = 3000,
                    Size = 10,
                    Crc = 0xA684C7C6,
                },
                new ImageInfo()
                {
                    Id = "def",
                    Filename = "def.jpg",
                    ContentType = "image/jpeg",
                    Created = new DateTime(2001, 1, 1),
                    Width = 4000,
                    Height = 3000,
                    Size = 10,
                    Crc = 0x83DDB0B5,
                }
            }));
        await IntegrityChecker.CheckYear(album.Object, 2001);

        album.Verify(_ => _.StoreYearIndex(It.Is<YearIndex>(i => 
            i.Year == 2001 &&
            i.Count == 2 &&
            i.Size == 20 &&
            i.Crc == 0xD9D0CDA6)));
    }
}
