namespace PocketAlbum.Models;

/// <summary>
/// Class holding filter values used when querying images from the album
/// </summary>
public class FilterModel
{
    /// <summary>
    /// Include images made in specified years
    /// </summary>
    public Interval? Year { get; set; }

    /// <summary>
    /// Include images within specified index, ordered chronologically
    /// </summary>
    public Interval? Index { get; set; }

    public bool Valid => Year != null || Index != null;
}
