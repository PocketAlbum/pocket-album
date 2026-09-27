using System.Collections.Concurrent;

namespace PocketAlbum.Server.Services;

public class PublicService
{
    readonly ConcurrentDictionary<Guid, byte[]> sharedContent = new();

    public Guid Share(byte[] image)
    {
        var shareId = Guid.NewGuid();
        sharedContent[shareId] = image;
        return shareId;
    }

    public void Unshare(Guid shareId)
    {
        sharedContent.Remove(shareId, out _);
    }

    public byte[] GetContent(Guid shareId)
    {
        return sharedContent[shareId];
    }
}