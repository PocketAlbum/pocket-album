namespace PocketAlbum.Server.Services;

public class AlbumService(Dictionary<Guid, IAlbum> albums)
{
    public IReadOnlyDictionary<Guid, IAlbum> Albums => albums.AsReadOnly();

    private Dictionary<Guid, IAlbum> albums = albums;
}
