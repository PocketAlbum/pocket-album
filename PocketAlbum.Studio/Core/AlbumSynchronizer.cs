using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia.Controls;
using MsBox.Avalonia.Enums;
using PocketAlbum.Studio.ViewModels;
using PocketAlbum.Studio.Views;

namespace PocketAlbum.Studio.Core;

public class AlbumSynchronizer
{
    private readonly ImageProgressViewModel progressModel;
    private readonly ImageProgressWindow progressWindow;
    private readonly IAlbum album1;
    private readonly IAlbum album2;

    public AlbumSynchronizer(IAlbum album1, IAlbum album2)
    {
        this.album1 = album1;
        this.album2 = album2;
        
        progressModel = new ImageProgressViewModel()
        {
            ProcessName = "Synchronizing albums",
            Location = "Checking integrity",
            ShowProgressBar = true
        };
        progressWindow = new ImageProgressWindow()
        {
            DataContext = progressModel,
            Title = "Album synchronization",
        };
    }

    internal async Task Start(Window owner)
    {
        var task = Task.Run(async () =>
        {
            try {
                await VerifyIntegrity();
                var years = await Compare();
                await VerifyIntegrity(years);
            }
            catch (Exception e)
            {
                progressWindow.ShowMessage("Error", e.Message, Icon.Error);
                return;
            }
            finally
            {
                progressModel.MarkDone();
            }
            progressWindow.ShowMessage("Success", "Synchronization finished", Icon.Success);
        });
        await progressWindow.ShowDialog(owner);
    }

    private async Task VerifyIntegrity()
    {
        Action<double> progress = p =>
        {
            progressModel.Progress = p * 0.1;
        };
        await IntegrityChecker.CheckAllYears(album2, progress);
    }

    private async Task<List<int>> Compare()
    {
        AlbumComparator comparator = new AlbumComparator(album1, album2);
        int year = 0;
        Action<double> progress = p =>
        {
            progressModel.Progress = (p * 0.8) + 0.1;
        };
        List<int> years = new List<int>();
        await foreach(var result in comparator.Compare(progress, true))
        {
            if (year != result.Year)
            {
                progressModel.Location = "Synchronizing year " + result.Year;
                year = result.Year;
                years.Add(year);
            }
            
            var imageInfo = result.Image1 ?? result.Image2 
                ?? throw new ArgumentException("Both image infos are null");
            var item = new ImportItem(imageInfo.Filename, imageInfo.Size);
            if (!result.Conflict)
            {
                item.SetState(ImportItem.ImportStates.EXISTS);
            }
            progressModel.Total++;
            progressModel.AddItem(item);

            if (result.Conflict)
            {
                await comparator.Resolve(result);
                item.SetState(ImportItem.ImportStates.IMPORTED);
            }
        }
        return years;
    }

    private async Task VerifyIntegrity(List<int> years)
    {
        int i = 0;
        foreach (int y in years)
        {
            progressModel.Progress = ((double)i / years.Count * 0.1) + 0.9;
            await IntegrityChecker.CheckYear(album1, y);
            await IntegrityChecker.CheckYear(album2, y);
            i++;
        }
    }
}