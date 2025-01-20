using System.Text;

namespace PocketAlbum;

internal static class Utilities
{
    public static string ByteArrayToString(byte[] array)
    {
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < array.Length; i++)
        {
            builder.Append($"{array[i]:x2}");
        }
        return builder.ToString();
    }
}
