using PocketAlbum.Models;
using PocketAlbum.SQLite;
using SixLabors.ImageSharp;

namespace PocketAlbum.Tool;

internal class Program
{
    static void Main(string[] args)
    {
        args = ["/home/jaka/Downloads/album.sqlite"];
        Console.WriteLine("Pocket Album Tool");
        if (args.Length == 0 || !args[0].EndsWith(".sqlite"))
        {
            Console.WriteLine("Provide path to SQLite file as the first argument");
        }
        if (!File.Exists(args[0]))
        {
            Console.WriteLine($"File named {args[0]} not found");
        }

        Run(args[0]).Wait();
    }

    static async Task Run(string albumPath)
    {
        try
        {
            IAlbum album;
            if (File.Exists(albumPath))
            {
                album = await SQLiteAlbum.Open(albumPath);
            }
            else
            {
                var name = GetName();
                album = await SQLiteAlbum.Create(albumPath, MetadataModel.Create(name));
            }
            
            await CheckIndex(album);

            ImageImporter importer = new ImageImporter(album);
            await AddRecursively(importer, GetImagesLocation());

            Console.WriteLine("Import finished, starting to build index");

            await CheckIndex(album);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.Message);
            Console.WriteLine(e.InnerException);
            Console.WriteLine(e.StackTrace);
        }
    }

    static string GetName()
    {
        string? name;
        do
        {
            Console.WriteLine("Creating new album, enter name:");
            name = Console.ReadLine();
        }
        while (string.IsNullOrWhiteSpace(name));
        return name;
    }

    static async Task CheckIndex(IAlbum album)
    {
        foreach (var year in await IntegrityChecker.InvalidYears(album))
        {
            Console.WriteLine("Indexing year " + year);
            await IntegrityChecker.CheckYear(album, year);
        }
    }

    static string GetImagesLocation()
    {
        string? path = null;
        while (path == null || !Directory.Exists(path))
        {
            Console.Write("Provide the images location: ");
            path = Console.ReadLine();
        }
        return path;
    }

    static async Task AddRecursively(ImageImporter importer, string path)
    {
        Console.WriteLine($"Directory {path}");
        var files = Directory.EnumerateFiles(path);
        foreach (var file in files)
        {
            long size = new FileInfo(file).Length;
            Console.Write($"{Path.GetFileName(file)} ({Utilities.FormatSize(size)})");
            try
            {
                await importer.Import(file);
                Console.WriteLine($" - Imported");
            }
            catch (UnknownImageFormatException)
            {
                Console.WriteLine($" - Failed to open as image");
            }
            catch (Exception e)
            {
                Console.WriteLine($" - {e.Message}");
            }
        }

        var directories = Directory.EnumerateDirectories(path);
        foreach (var dir in directories)
        {
            await AddRecursively(importer, dir);
        }
    }
}
