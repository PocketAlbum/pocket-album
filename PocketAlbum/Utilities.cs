using System.Text;

namespace PocketAlbum;

public static class Utilities
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

    public static string FormatSize(long size)
    {
        if (size > 1000000000)
        {
            return string.Format("{0:F1} GB", (decimal)size / 1000000000);
        }
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
