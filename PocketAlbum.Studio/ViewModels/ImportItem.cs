using System.IO;

namespace PocketAlbum.Studio.ViewModels;

public class ImportItem
{
    private readonly string path;

    public string Filename => Path.GetFileName(path);

    public ImportItem(string path)
    {
        this.path = path;
    }
}