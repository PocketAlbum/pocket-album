
using System;
using System.ComponentModel;
using Avalonia.Threading;

namespace PocketAlbum.Studio.Core;

class ObservableStopwatch : INotifyPropertyChanged
{
    public DateTime StartTime { get; init; } = DateTime.Now;

    public DateTime? StopTime { get; private set; }

    public TimeSpan Elapsed => (StopTime ?? DateTime.Now) - StartTime;

    private DispatcherTimer timer = new DispatcherTimer();

    public ObservableStopwatch(TimeSpan notificationInterval)
    {
        timer.Interval = notificationInterval;
        timer.Tick += TimerTick;
        timer.Start();
    }

    private void TimerTick(object? sender, EventArgs e)
    {
        PropertyChanged?
            .Invoke(this, new PropertyChangedEventArgs(nameof(Elapsed)));
            
        PropertyChanged?
            .Invoke(this, new PropertyChangedEventArgs(nameof(ElapsedString)));
    }

    internal void Stop()
    {
        timer.Stop();
        StopTime = DateTime.Now;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public string ElapsedString => Elapsed.ToString(@"hh\:mm\:ss");
}
