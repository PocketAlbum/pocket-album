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
    /// Include images taken at specified time of day
    /// </summary>
    public TimesOfDay? TimeOfDay { get; set; }

    public bool HasAny => Year != null || TimeOfDay != null;

    public enum TimesOfDay
    {
        Morning,
        Afternoon,
        Evening,
        Night
    }
}
