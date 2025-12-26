using System.Threading;
using Avalonia.Controls;
using Avalonia.Interactivity;

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
}