using PocketAlbum.SQLite;
using SixLabors.ImageSharp;

namespace PocketAlbum.Tool;

internal class Program
{
    static void Main(string[] args)
    {
        Console.WriteLine("Pocket Album Tool");
        if (args.Length == 0 || !args[0].EndsWith(".sqlite"))
        {
            Console.WriteLine("Provide path to SQLite file as the first argument");
        }
        if (!File.Exists(args[0]))
        {
            Console.WriteLine($"File named {args[0]} not found");
        }
        try
        {
            Run(args[0]).Wait();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }

    static async Task Run(string albumPath)
    {
        IAlbum album = await SQLiteAlbum.Open(albumPath);

        IntegrityChecker checker = new IntegrityChecker(album);
        await CheckIndex(checker);

        ImageImporter importer = new ImageImporter(album);
        await AddRecursively(importer, GetImagesLocation());

        Console.WriteLine("Import finished, starting to build index");

        await CheckIndex(checker);
    }

    static async Task CheckIndex(IntegrityChecker checker)
    {
        foreach (var year in await checker.InvalidYears())
        {
            Console.WriteLine("Indexing year " + year);
            await checker.CheckYear(year);
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
            Console.Write($"{Path.GetFileName(file)} ({FormatSize(size)})");
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

    static string FormatSize(long size)
    {
        if (size > 1000000)
        {
            return string.Format("{0:F1} MB", (decimal)size / 1000000);
        }
        else if (size > 1000)
        {
            return string.Format("{0:F1} kB", (decimal)size / 1000);
        }
        else return size + " B";
    }
}
