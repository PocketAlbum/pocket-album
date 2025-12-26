using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using PocketAlbum.Models;

namespace PocketAlbum.Studio.ViewModels;

public class ObservableAlbum : IReadOnlyList<GalleryItem>
{
    const int BLOCK_SIZE = 128;
    private readonly IAlbum album;
    private readonly FilterModel filter;
    private readonly AlbumInfo info;

    public int Count => info.ImageCount;

    public GalleryItem this[int index] => new GalleryItem(this, index);

    private ObservableAlbum(IAlbum album, FilterModel filter, AlbumInfo info)
    {
        this.album = album;
        this.filter = filter;
        this.info = info;
    }

    public static async Task<ObservableAlbum> FromAlbum(IAlbum album, FilterModel filter)
    {
        var info = await album.GetInfo(filter);
        return new ObservableAlbum(album, filter, info);
    }

    public IEnumerator<GalleryItem> GetEnumerator()
    {
        for (int i = 0; i < Count; i++)
            yield return this[i];
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    private readonly ConcurrentDictionary<int, Task<List<ImageThumbnail>>> futures = new();

    private Task<List<ImageThumbnail>> GetOrCreateTask(int block)
    {
        return futures.GetOrAdd(block, b =>
        {
            Console.WriteLine($"Loading block {b}, cache size {futures.Count}");
            return Task.Run(() => LoadImages(b));
        });
    }

    private async Task<List<ImageThumbnail>> LoadImages(int block)
    {
        try
        {
            int first = block * BLOCK_SIZE;
            int last = (block + 1) * BLOCK_SIZE - 1;

            return await album.GetImages(filter, new Interval(first, last));
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to load images: {e}");
            throw;
        }
    }

    public async Task<ImageThumbnail> GetImage(int number)
    {
        int block = number / BLOCK_SIZE;
        var list = await GetOrCreateTask(block);

        int index = number - (block * BLOCK_SIZE);
        if (index >= list.Count) {
            throw new KeyNotFoundException($"Image #{number} not found");
        }

        return list[index];
    }

    public async Task<byte[]> GetData(string id)
    {
        return await album.GetData(id);
    }
}