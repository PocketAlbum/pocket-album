using System.Text.RegularExpressions;

namespace PocketAlbum.Models;

public class MetadataModel
{
    public required Guid Id { get; init; }
    public required string Version {  get; init; }
    public required string Name { get; init; }
    public string? Description {  get; init; }
    public required DateTime Created { get; init; }
    public required DateTime Updated { get; init; }

    public static MetadataModel Create(string name)
    {
        return new MetadataModel
        {
            Id = Guid.NewGuid(),
            Version = "PocketAlbum 1.0",
            Name = name,
            Description = null,
            Created = DateTime.Now,
            Updated = DateTime.Now
        };
    }

    public void Validate()
    {
        Regex formatRegex = new Regex("PocketAlbum (\\d+\\.\\d+)");
        var match = formatRegex.Match(Version);
        if (!match.Success)
        {
            throw new InvalidDataException($"Version {Version} is not recognized");
        }
        var version = System.Version.Parse(match.Groups[1].Value);
        if (version.Major != 1)
        {
            throw new InvalidDataException($"Unsupported version {version}");
        }

        if (string.IsNullOrEmpty(Name))
        {
            throw new InvalidDataException("No album name found");
        }
    }
}
