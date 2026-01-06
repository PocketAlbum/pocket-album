using System;
using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using PocketAlbum.Studio.Core;
using static PocketAlbum.Studio.ViewModels.ImportItem;

namespace PocketAlbum.Studio.ViewModels;

partial class ImageProgressViewModel : ViewModelBase
{
    public required string ProcessName { get; init; }

    [ObservableProperty]
    private string? location;

    [ObservableProperty]
    private int total;

    [ObservableProperty]
    private int folders;

    [ObservableProperty]
    private bool done;

    public bool ShowProgressBar { get; init; }

    [ObservableProperty]
    public double progress;

    public ObservableCollection<ImportStatistics> Statistics { get; } = [];

    public ObservableStopwatch TimeSinceStart { get; } = new(TimeSpan.FromSeconds(1));

    public ObservableCollection<ImportItem> Items { get; } = [];

    [ObservableProperty]
    private bool more;

    public void AddItem(ImportItem item)
    {
        Dispatcher.UIThread.Post(() => {
            Items.Add(item);
            while (Items.Count > 12)
            {
                Items.RemoveAt(0);
                More = true;
            }
        });
    }

    public void MarkDone()
    {
        TimeSinceStart.Stop();
        Done = true;
    }

    internal void ReportStatistics(ImportStates state)
    {
        var existing = Statistics.FirstOrDefault(s => s.State == state);
        if (existing == null)
        {
            existing = new ImportStatistics(state);
            Dispatcher.UIThread.Post(() =>
            {
                Statistics.Add(existing);
            });
        }
        existing.Count++;
    }

    public partial class ImportStatistics(ImportStates state) : ObservableObject
    {
        public ImportStates State { get; init; } = state;

        [ObservableProperty]
        public int count;

        public Bitmap Icon => GetIcon(State);

        public string StateString => GetStateString(State);
    }
}