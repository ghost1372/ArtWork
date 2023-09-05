using System.Text.Json;

using ArtWork.Database;
using ArtWork.Models;

namespace ArtWork.ViewModels;
public partial class DataBaseViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string titleStatus;

    [ObservableProperty]
    private string messageStatus;

    [ObservableProperty]
    private int progressValue;

    [ObservableProperty]
    private int progressMaxValue;

    public DataBaseViewModel()
    {
        IsActive = true;
    }

    [RelayCommand]
    private async Task OnReBuildDataBase()
    {
        IsActive = false;
        RemoveInCompleteFiles();
        IEnumerable<string> jsonFiles = Directory.EnumerateFiles(Settings.ArtWorkDirectory, "*.json", SearchOption.AllDirectories);
        ProgressMaxValue = jsonFiles.Count();

        TitleStatus = $"Total Json Files: {ProgressMaxValue}";
        using var db = new ArtWorkDbContext();
        await db.Database.EnsureDeletedAsync();
        await Task.Delay(1000);
        await db.Database.EnsureCreatedAsync();
        await Task.Delay(1000);
        await AddNudesIntoDataBase();

        ProgressValue = 0;
        foreach (var item in jsonFiles)
        {
            ProgressValue++;
            using FileStream openStream = File.OpenRead(item);
            var artWorkJson = await JsonSerializer.DeserializeAsync<ArtWorkModel>(openStream);
            var fileName = $"{Path.GetFileNameWithoutExtension(item)}.jpg";

            var dir = GetDirectoryName(artWorkJson?.wikiartist, artWorkJson?.sig);

            var art = GetArt(db, artWorkJson, fileName, dir.DirectoryName, dir.SimplifiedSig);

            await db.Arts.AddAsync(art);
            MessageStatus = $"{ProgressValue} Items Added to the database.";
        }

        await db.SaveChangesAsync();
        IsActive = true;
        ProgressValue = 0;
    }

    [RelayCommand]
    private void OnNormalizeFolders()
    {
        IsActive = false;
        ProgressValue = 0;
        string[] allDirectories = Directory.GetDirectories(Settings.ArtWorkDirectory);
        ProgressMaxValue = allDirectories.Count();
        TitleStatus = $"Total Directories: {ProgressMaxValue}";
        foreach (string directoryPath in allDirectories)
        {
            ProgressValue++;
            var oldPath = directoryPath;
            var newPath = NormalizeString(directoryPath);
            if (!oldPath.Equals(newPath))
            {
                Directory.Move(oldPath, newPath);
            }
        }
        MessageStatus = $"{ProgressValue} Folder(s) Normalized.";
        IsActive = true;
        ProgressValue = 0;
    }
}
