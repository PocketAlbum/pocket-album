using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using PocketAlbum.Studio.ViewModels;
using PocketAlbum.Studio.Views;
using static PocketAlbum.ImageImporter;

namespace PocketAlbum.Studio.Core;

public class RecursiveFilesImporter 
{
    private readonly string initialPath;
    private readonly IAlbum album;
    private readonly ImageProgressViewModel progressModel;
    private readonly ImageProgressWindow progressWindow;

    public RecursiveFilesImporter(string path, IAlbum album)
    {
        initialPath = path;
        this.album = album;
        progressModel = new ImageProgressViewModel()
        {
            ProcessName = "Importing images",
            Location = initialPath
        };
        progressWindow = new ImageProgressWindow()
        {
            DataContext = progressModel,
            Title = "Image import"
        };
    }

    internal async Task Start(Window owner)
    {
        var task = Task.Run(async () =>
        {
            try {
                ImageImporter importer = new ImageImporter(album);
                await AddRecursively(importer, initialPath);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                progressModel.MarkDone();
            }
        });
        await progressWindow.ShowDialog(owner);
    }

    async Task AddRecursively(ImageImporter importer, string path)
    {
        progressModel.Folders++;
        progressModel.Location = path;
        List<string> extensions = [];
        extensions.AddRange([".jpg", ".jpeg", ".jfif", ".pjpeg", ".pjp"]);
        extensions.AddRange([".heif", ".heic", ".heifs", ".heics"]);
        extensions.AddRange([".tif", ".tiff"]);
        extensions.AddRange([".png"]);

        var files = Directory.EnumerateFiles(path);
        foreach (var file in files.Where(f => extensions.Any(e => f.EndsWith(e))))
        {
            progressWindow.Cancellation.ThrowIfCancellationRequested();

            long size = new FileInfo(file).Length;
            var item = new ImportItem(Path.GetFileName(file), (ulong)size);
            progressModel.Total++;
            progressModel.AddItem(item);
            try
            {
                await importer.Import(file);
                item.SetState(ImportItem.ImportStates.IMPORTED);
                progressModel.ReportStatistics(ImportItem.ImportStates.IMPORTED);
            }
            catch (ImageExistsException)
            {
                item.SetState(ImportItem.ImportStates.EXISTS);
                progressModel.ReportStatistics(ImportItem.ImportStates.EXISTS);
            }
            catch (Exception e)
            {
                item.SetException(e);
                progressModel.ReportStatistics(ImportItem.ImportStates.FAILED);
            }
        }

        var directories = Directory.EnumerateDirectories(path);
        foreach (var dir in directories)
        {
            await AddRecursively(importer, dir);
        }
    }
}
