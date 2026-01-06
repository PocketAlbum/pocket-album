using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;

namespace PocketAlbum.Studio.Views;

public partial class ImageProgressWindow : Window
{

    public CancellationTokenSource cts = new CancellationTokenSource();
    public CancellationToken Cancellation => cts.Token;
    
    public ImageProgressWindow()
    {
        InitializeComponent();
    }

    public void CloseClick(object sender, RoutedEventArgs args)
    {
        Close();
    }

    public void CancelClick(object sender, RoutedEventArgs args)
    {
        cts.Cancel();
    }

    public void ShowMessage(string caption, string message, Icon icon)
    {
        Dispatcher.UIThread.Post(async () =>
        {
            await MessageBoxManager
                .GetMessageBoxStandard(caption, message, ButtonEnum.Ok, icon)
                .ShowAsync();
            Close();
        });
    }
}