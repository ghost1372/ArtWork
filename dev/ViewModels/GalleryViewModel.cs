using ArtWork.Database;
using ArtWork.Database.Tables;

using CommunityToolkit.WinUI.UI;

using Microsoft.EntityFrameworkCore;

namespace ArtWork.ViewModels;
public partial class GalleryViewModel : ObservableRecipient
{
    [ObservableProperty]
    private ObservableCollection<Art> arts;

    [ObservableProperty]
    private AdvancedCollectionView artsACV;

    [ObservableProperty]
    public bool isTitleSelected;

    [ObservableProperty]
    public bool isGallerySelected;

    [ObservableProperty]
    public bool isSigSelected;

    [ObservableProperty]
    public bool isCountrySelected;

    [ObservableProperty]
    public bool isCitySelected;

    [RelayCommand]
    private async void OnPageLoaded()
    {
        IsTitleSelected = true;
        IsSigSelected = true;
        IsGallerySelected = true;
        IsCountrySelected = true;
        IsCitySelected = true;

        using var db = new ArtWorkDbContext();
        var arts = db.Arts.Select(x=> new Art
        {
            Id = x.Id,
            FolderName = x.FolderName,
            FileName = x.FileName,
            FileFolderPath = Path.Combine(Settings.ArtWorkDirectory, x.FileFolderPath),
            Title = x.Title,
            Sig = x.Sig,
            SimplifiedSig = x.SimplifiedSig,
            Gallery = x.Gallery,
            City = x.City,
            Country = x.Country,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            Wikiartist = x.Wikiartist
        });

        Arts = new(arts);
        ArtsACV = new AdvancedCollectionView(Arts, true);
    }

    public async void Search()
    {
        var txtSearch = MainPage.Instance.GetAutoSuggestBox();
        using var db = new ArtWorkDbContext();
        var arts = await db.Arts.Select(x => new Art
        {
            Id = x.Id,
            FolderName = x.FolderName,
            FileName = x.FileName,
            FileFolderPath = Path.Combine(Settings.ArtWorkDirectory, x.FileFolderPath),
            Title = x.Title,
            Sig = x.Sig,
            SimplifiedSig = x.SimplifiedSig,
            Gallery = x.Gallery,
            City = x.City,
            Country = x.Country,
            Latitude = x.Latitude,
            Longitude = x.Longitude,
            Wikiartist = x.Wikiartist
        }).ToListAsync();

        var filteredArts = arts.Where(x =>
                (IsTitleSelected && !string.IsNullOrEmpty(x.Title) && x.Title.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)) ||
                (IsSigSelected && !string.IsNullOrEmpty(x.SimplifiedSig) && x.SimplifiedSig.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)) ||
                (IsGallerySelected && !string.IsNullOrEmpty(x.Gallery) && x.Gallery.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)) ||
                (IsCountrySelected && !string.IsNullOrEmpty(x.Country) && x.Country.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)) ||
                (IsCitySelected && !string.IsNullOrEmpty(x.City) && x.City.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)));

        Arts = new(filteredArts);
        ArtsACV = new AdvancedCollectionView(Arts, true);
    }
}
