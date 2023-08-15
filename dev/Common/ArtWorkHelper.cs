using ArtWork.Database;

using Microsoft.EntityFrameworkCore;

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

    public static (string DirectoryName, string SimplifiedSig) GetDirectoryName(string wikiartist, string sig)
    {
        string simplifiedSig = string.Empty;

        int index = sig.IndexOf(',');
        if (index > 0)
        {
            simplifiedSig = sig?.Substring(0, index);
        }

        if (string.IsNullOrEmpty(wikiartist))
        {
            wikiartist = simplifiedSig ?? "Unknown Artist";
        }

        return (string.Join("", wikiartist.Split(Path.GetInvalidFileNameChars())), simplifiedSig);
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
}
