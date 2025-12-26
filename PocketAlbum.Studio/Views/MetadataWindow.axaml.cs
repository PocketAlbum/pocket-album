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
            if (string.IsNullOrWhiteSpace(mvm.Title))
            {
                return;
            }
            Close(new MetadataModel()
            {
                Created = DateTime.Now,
                Updated = DateTime.Now,
                Id = Guid.NewGuid(),
                Name = mvm.Title,
                Description = mvm.Description,
                Version = "PocketAlbum 1.0"
            });
        }
    }
}