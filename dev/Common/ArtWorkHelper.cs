using System.Globalization;
using System.Text;

using ArtWork.Database;
using ArtWork.Database.Tables;
using ArtWork.Models;

using Microsoft.EntityFrameworkCore;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;

using Nucs.JsonSettings;
using Nucs.JsonSettings.Autosave;
using Nucs.JsonSettings.Fluent;
using Nucs.JsonSettings.Modulation;
using Nucs.JsonSettings.Modulation.Recovery;

namespace ArtWork.Common;
public static class ArtWorkHelper
{
    public static ArtWorkConfig Settings = JsonSettings.Configure<ArtWorkConfig>()
                               .WithRecovery(RecoveryAction.RenameAndLoadDefault)
                               .WithVersioning(VersioningResultAction.RenameAndLoadDefault)
                               .LoadNow()
                               .EnableAutosave();

    public static string[] ImageFilesCacheForHomeLandingPage = null;

    public static Art GetArt(ArtWorkDbContext db, ArtWorkModel artWorkJson, string fileName, string directoryName, string simplifiedSig)
    {
        var fileFolderPath = Path.Combine(directoryName, fileName);

        var isNude = db.Nudes.Any(x => (Path.GetFileNameWithoutExtension(fileName)).Equals(x.FileNameWithoutExtension));

        return new Art
        {
            IsNude = isNude,
            FolderName = directoryName,
            FileName = fileName,
            FileFolderPath = fileFolderPath,
            City = NormalizeString(artWorkJson.city),
            Country = NormalizeString(artWorkJson.country),
            Gallery = NormalizeString(artWorkJson.gal),
            Latitude = artWorkJson.lat,
            Longitude = artWorkJson.@long,
            Sig = NormalizeString(artWorkJson.sig),
            SimplifiedSig = simplifiedSig,
            Title = NormalizeString(artWorkJson.title),
            Wikiartist = NormalizeString(artWorkJson.wikiartist)
        };
    }

    public static (string DirectoryName, string SimplifiedSig) GetDirectoryName(string wikiartist, string sig)
    {
        string simplifiedSig = string.Empty;

        int commaIndex = sig.IndexOf(',');
        if (commaIndex > 0)
        {
            simplifiedSig = sig?.Substring(0, commaIndex);
        }

        if (string.IsNullOrEmpty(simplifiedSig))
        {
            int questionMarkIndex = sig.IndexOf('?');
            if (questionMarkIndex > 0)
            {
                simplifiedSig = sig?.Substring(0, questionMarkIndex);
            }
        }

        simplifiedSig = simplifiedSig.Replace("(?)", "");
        simplifiedSig = simplifiedSig.Trim();

        if (string.IsNullOrEmpty(wikiartist))
        {
            wikiartist = simplifiedSig ?? "Unknown Artist";
        }

        var dirName = string.Join("", wikiartist.Split(Path.GetInvalidFileNameChars()));
        dirName = NormalizeString(dirName).Trim();
        return (dirName, NormalizeString(simplifiedSig) ?? dirName);
    }

    public static async Task AddNudesIntoDataBase()
    {
        using var db = new ArtWorkDbContext();
        var existNude = await db.Nudes.AnyAsync();
        if (!existNude)
        {
            var file = await FileLoaderHelper.GetPath(Constants.NudesPath);
            using StreamReader reader = new StreamReader(file);
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                await db.Nudes.AddAsync(new Database.Tables.Nude
                {
                    FileNameWithoutExtension = line
                });
            }
            await db.SaveChangesAsync();
        }
    }

    public static string NormalizeString(string input)
    {
        if (string.IsNullOrEmpty(input))
            return input;

        string normalizedString = input.Normalize(NormalizationForm.FormD);
        StringBuilder result = new StringBuilder();

        foreach (char c in normalizedString)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                result.Append(c);
        }

        return result.ToString();
    }

    public static void RemoveInCompleteFiles()
    {
        HashSet<string> imageFileNames = new HashSet<string>(Directory.EnumerateFiles(Settings.ArtWorkDirectory, "*.jpg", SearchOption.AllDirectories));
        HashSet<string> jsonFileNames = new HashSet<string>(Directory.EnumerateFiles(Settings.ArtWorkDirectory, "*.json", SearchOption.AllDirectories));

        var onlyImageFileName = imageFileNames.Select(Path.GetFileNameWithoutExtension);
        var onlyJsonFileName = jsonFileNames.Select(Path.GetFileNameWithoutExtension);

        var imageFilesToDelete = onlyImageFileName.Except(onlyJsonFileName);
        var jsonFilesToDelete = onlyJsonFileName.Except(onlyImageFileName);

        foreach (var item in imageFilesToDelete)
        {
            var removeItem = imageFileNames.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).Equals(item));
            if (removeItem != null)
            {
                File.Delete(removeItem);
            }
        }

        foreach (var item in jsonFilesToDelete)
        {
            var removeItem = jsonFileNames.FirstOrDefault(x => Path.GetFileNameWithoutExtension(x).Equals(item));
            if (removeItem != null)
            {
                File.Delete(removeItem);
            }
        }
    }

    public static T FindParent<T>(DependencyObject child) where T : DependencyObject
    {
        while (true)
        {
            DependencyObject parentObject = VisualTreeHelper.GetParent(child);

            if (parentObject == null)
            {
                return null;
            }

            if (parentObject is T parent)
            {
                return parent;
            }

            child = parentObject;
        }
    }

    public static Art GetArtFromRightClick(RightTappedRoutedEventArgs e)
    {
        FrameworkElement element = e.OriginalSource as FrameworkElement;
        if (element != null)
        {
            var itemContainer = FindParent<ItemContainer>(element);
            if (itemContainer != null)
            {
                return itemContainer.DataContext as Art;
            }
        }
        return null;
    }
}
