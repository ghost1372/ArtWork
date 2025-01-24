using ArtWork.Database;
using ArtWork.Database.Tables;

using Microsoft.EntityFrameworkCore;

namespace ArtWork.ViewModels;
public partial class GalleryViewModel : ObservableRecipient, ITitleBarAutoSuggestBoxAware
{
    [ObservableProperty]
    public partial ObservableCollection<Art> Arts { get; set; }

    [ObservableProperty]
    public partial bool IsTitleSelected { get; set; }

    [ObservableProperty]
    public partial bool IsGallerySelected { get; set; }

    [ObservableProperty]
    public partial bool IsSigSelected { get; set; }

    [ObservableProperty]
    public partial bool IsCountrySelected { get; set; }

    [ObservableProperty]
    public partial bool IsCitySelected { get; set; }

    [RelayCommand]
    private void OnPageLoaded()
    {
        IsTitleSelected = true;
        IsSigSelected = true;
        IsGallerySelected = true;
        IsCountrySelected = true;
        IsCitySelected = true;

        if (Arts == null)
        {
            GetArts(Settings.IsShowNudes, false);
        }
    }

    public void GetArts(bool isShowNudes, bool isShowOnlyNudes)
    {
        using var db = new ArtWorkDbContext();
        IQueryable<Art> arts = db.Arts;
        if (!isShowNudes)
        {
            arts = arts.Where(x => x.IsNude == false);
        }

        if (isShowNudes && isShowOnlyNudes)
        {
            arts = arts.Where(x => x.IsNude == true);
        }

        arts = arts.Select(x => new Art
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

        arts = arts.OrderBy(x => EF.Functions.Random());
        Arts = new(arts);
    }

    public async void Search()
    {
        var txtSearch = MainPage.Instance.GetAutoSuggestBox();
        using var db = new ArtWorkDbContext();
        IQueryable<Art> arts = db.Arts;

        if (!Settings.IsShowNudes)
        {
            arts = arts.Where(x => x.IsNude == false);
        }

        if (Settings.IsShowNudes && Settings.IsShowOnlyNudes)
        {
            arts = arts.Where(x => x.IsNude == true);
        }

        var artsList = await arts.Select(x => new Art
        {
            Id = x.Id,
            IsNude = x.IsNude,
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

        var filteredArts = artsList.Where(x =>
                (IsTitleSelected && !string.IsNullOrEmpty(x.Title) && x.Title.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)) ||
                (IsSigSelected && !string.IsNullOrEmpty(x.SimplifiedSig) && x.SimplifiedSig.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)) ||
                (IsGallerySelected && !string.IsNullOrEmpty(x.Gallery) && x.Gallery.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)) ||
                (IsCountrySelected && !string.IsNullOrEmpty(x.Country) && x.Country.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)) ||
                (IsCitySelected && !string.IsNullOrEmpty(x.City) && x.City.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase)));

        Arts = new(filteredArts);
    }

    public void OnAutoSuggestBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        Search();
    }

    public void OnAutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
    }
}
