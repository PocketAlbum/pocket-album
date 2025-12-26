using System;
using System.IO;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;
using SixLabors.ImageSharp;

namespace PocketAlbum.Studio.ViewModels;

public class ImportItem(string path, long size) : ObservableObject
{
    private readonly string path = path;

    public string Filename => Path.GetFileName(path);

    public long Size { get; } = size;

    public Exception? Exception { get; private set; }

    public ImportStates State { get; private set; } = ImportStates.PROCESSING;

    public string SizeString 
    {
        get
        {
            if (Size > 1000000)
            {
                return string.Format("{0:F1} MB", (decimal)Size / 1000000);
            }
            else if (Size > 1000)
            {
                return string.Format("{0:F1} kB", (decimal)Size / 1000);
            }
            else return Size + " B";
        }
    }

    public string Status
    {
        get
        {
            if (Exception is UnknownImageFormatException)
            {
                return "Failed to open as image";
            }
            else if (Exception is UnauthorizedAccessException)
            {
                return "Access denied";
            }
            else if (Exception != null)
            {
                return Exception.Message;
            }
            else
            {
                return GetStateString(State);
            }
        }
    }

    public Bitmap Icon => GetIcon(State);

    public static string GetStateString(ImportStates state)
    {
        return state switch
        {
            ImportStates.PROCESSING => "Processing",
            ImportStates.IMPORTED => "Imported",
            ImportStates.EXISTS => "Existing",
            _ => "Failed",
        };
    }

    private static string GetImageSource(ImportStates state)
    {
        return state switch
        {
            ImportStates.PROCESSING => "/Assets/MDI/help_black_24.png",
            ImportStates.IMPORTED => "/Assets/MDI/check_circle_green_24.png",
            ImportStates.EXISTS => "/Assets/MDI/check_circle_outline_green_24.png",
            _ => "/Assets/MDI/close_circle_red_24.png",
        };
    }

    public static Bitmap GetIcon(ImportStates state)
    {
        var uri = new Uri("avares://PocketAlbum.Studio" + GetImageSource(state));
            return new Bitmap(AssetLoader.Open(uri));
    }

    public void SetException(Exception exception)
    {
        Exception = exception;
        State = ImportStates.FAILED;
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(Icon));
    }

    public void SetState(ImportStates results)
    {
        State = results;
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(Icon));
    }

    public enum ImportStates
    {
        PROCESSING,
        IMPORTED,
        EXISTS,
        FAILED
    }
}