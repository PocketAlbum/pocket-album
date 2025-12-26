using Avalonia.Data.Converters;

namespace PocketAlbum.Studio.Views;

public static class Converters 
{
    public static FuncValueConverter<ulong, string> FormatSize { get; } = 
        new FuncValueConverter<ulong, string>(s => Utilities.FormatSize((long)s));
}