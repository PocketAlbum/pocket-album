using System;
using System.Collections.ObjectModel;
using Microsoft.Extensions.DependencyInjection;
using PocketAlbum.Studio.Services;
using Sharpcaster.Models;

namespace PocketAlbum.Studio.ViewModels;

internal class CastViewModel(CastingService service) : ViewModelBase
{
    public CastViewModel() : this(App.GetServices())
    { }

    private CastViewModel(IServiceProvider serviceProvider) : this(
        serviceProvider.GetRequiredService<CastingService>())
    { }

    public CastingService Service { get; } = service;

    public ObservableCollection<ChromecastReceiver> Receivers { get; } = [];

    public ChromecastReceiver? SelectedReceiver { get; set; }

    public async void RefreshList()
    {
        Receivers.Clear();
        var receivers = await Service.FindReceivers();
        foreach (var receiver in receivers)
        {
            Receivers.Add(receiver);
        }
    }
}
