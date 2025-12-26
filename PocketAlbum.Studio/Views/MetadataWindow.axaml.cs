using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using PocketAlbum.Models;
using PocketAlbum.Studio.ViewModels;

namespace PocketAlbum.Studio.Views;

public partial class MetadataWindow : Window
{
    public MetadataWindow()
    {
        InitializeComponent();
    }

    public void SaveClick(object sender, RoutedEventArgs args)
    {
        if (DataContext is MetadataViewModel mvm)
        {
            if (string.IsNullOrWhiteSpace(mvm.Name))
            {
                return;
            }
            Close(new MetadataModel()
            {
                Created = DateTime.Now,
                Updated = DateTime.Now,
                Id = Guid.NewGuid(),
                Name = mvm.Name,
                Description = mvm.Description,
                Version = PocketAlbum.VersionName
            });
        }
    }
}