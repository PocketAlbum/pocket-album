namespace PocketAlbum.Models;

public class Interval
{
    public long From { get; }
    public long To { get; }

    public bool SingleValue => From == To;

    public Interval(long value) : this(value, value)
    {
    }

    public Interval(long from, long to)
    {
        if (to < from)
        {
            throw new ArgumentException("Value 'from' must be less or equal to 'to'");
        }
        From = from;
        To = to;
    }
}
