using PocketAlbum.Server.Services;
using PocketAlbum.SQLite;

namespace PocketAlbum.Server;

public class Program
{
    public static void Main(string[] args)
    {
        RunServer(args).Wait();
    }

    private static async Task RunServer(string[] args,
        List<IAlbum>? preloadedAlbums = null)
    {
        var builder = WebApplication.CreateSlimBuilder(args);

        var albums = new Dictionary<Guid, IAlbum>();
        foreach (var album in preloadedAlbums ?? [])
        {
            var metadata = await album.GetMetadata();
            albums.TryAdd(metadata.Id, album);
        }

        var sources = builder.Configuration.GetSection("Albums").Get<string[]>();
        foreach (var source in sources ?? [])
        {
            try
            {
                var album = await LoadAlbumFromSource(source);
                var metadata = await album.GetMetadata();
                albums.TryAdd(metadata.Id, album);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unable to load album from {source}: {ex.Message}");
            }
        }

        builder.Services.AddSingleton(new AlbumService(albums));
        builder.Services.AddControllers();

        var app = builder.Build();

        app.MapControllers();

        app.Run();
    }

    private static async Task<IAlbum> LoadAlbumFromSource(string source)
    {
        if (File.Exists(source))
        {
            if (source.EndsWith(".sqlite"))
            {
                return await SQLiteAlbum.Open(source);
            }
            else throw new IOException("Only SQLITE albums supported");
        }
        else throw new IOException("File doesn't exist");
    }
}
