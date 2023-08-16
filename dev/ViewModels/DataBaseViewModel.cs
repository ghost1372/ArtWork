using System.Globalization;
using System.Text;
using System.Text.Json;

using ArtWork.Database;
using ArtWork.Database.Tables;
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
        TitleStatus = $"Total Json Files: {jsonFiles.Count()}";
        ProgressMaxValue = jsonFiles.Count();
        using var db = new ArtWorkDbContext();
        await db.Database.EnsureDeletedAsync();
        await Task.Delay(1000);
        await db.Database.EnsureCreatedAsync();
        await Task.Delay(1000);

        int index = 0;
        foreach (var item in jsonFiles)
        {
            index++;
            ProgressValue = index;
            using FileStream openStream = File.OpenRead(item);
            var artWorkJson = await JsonSerializer.DeserializeAsync<ArtWorkModel>(openStream);
            var dir = GetDirectoryName(artWorkJson?.wikiartist, artWorkJson?.sig);

            var folderName = dir.DirectoryName;
            var fileName = $"{Path.GetFileNameWithoutExtension(item)}.jpg";
            var fileFolderPath = Path.Combine(folderName, fileName);

            var art = new Art
            {
                FolderName = folderName,
                FileName = fileName,
                FileFolderPath = fileFolderPath,
                City = NormalizeString(artWorkJson.city),
                Country = NormalizeString(artWorkJson.country),
                Gallery = NormalizeString(artWorkJson.gal),
                Latitude = artWorkJson.lat,
                Longitude = artWorkJson.@long,
                Sig = NormalizeString(artWorkJson.sig),
                SimplifiedSig = dir.SimplifiedSig,
                Title = NormalizeString(artWorkJson.title),
                Wikiartist = NormalizeString(artWorkJson.wikiartist)
            };
            await db.Arts.AddAsync(art);
            MessageStatus = $"{index} Items Added to the database.";
        }

        await db.SaveChangesAsync();
        await AddNudesIntoDataBase();
        IsActive = true;
        ProgressValue = 0;
    }
}
