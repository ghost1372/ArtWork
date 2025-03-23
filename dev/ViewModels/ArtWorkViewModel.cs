using ArtWork.Database;
using ArtWork.Database.Tables;
using ArtWork.Models;

using CommunityToolkit.WinUI.Collections;

using Microsoft.UI.Xaml.Media.Animation;

namespace ArtWork.ViewModels;
public partial class ArtWorkViewModel : ObservableRecipient, ITitleBarAutoSuggestBoxAware
{
    [ObservableProperty]
    public partial ObservableCollection<Art> Artists { get; set; }

    [ObservableProperty]
    public partial AdvancedCollectionView ArtistsACV { get; set; }

    [ObservableProperty]
    public partial string MessageStatus { get; set; }

    [ObservableProperty]
    public partial object ListViewSelectedItem { get; set; }

    [ObservableProperty]
    public partial object CmbSelectedItem { get; set; } = "SimplifiedSig";

    public DataFilter Filter { get; set; }

    public ArtWorkViewModel()
    {
        OnComboBoxItemChanged();
    }

    [RelayCommand]
    private void OnComboBoxItemChanged()
    {
        IsActive = true;
        var item = CmbSelectedItem as ComboBoxItem;
        Filter = DataFilter.SimplifiedSig;
        if (item != null)
        {
            Filter = GeneralHelper.GetEnum<DataFilter>(item.Tag.ToString());
        }
        using var db = new ArtWorkDbContext();
        IQueryable<Art> uniqueItems = null;
        switch (Filter)
        {
            case DataFilter.SimplifiedSig:
                uniqueItems = db.Arts.Where(item => !string.IsNullOrEmpty(item.SimplifiedSig))
                    .GroupBy(item => item.SimplifiedSig)
                    .Select(group => group.FirstOrDefault());
                break;
            case DataFilter.Gallery:
                uniqueItems = db.Arts.Where(item => !string.IsNullOrEmpty(item.Gallery))
                    .GroupBy(item => item.Gallery)
                    .Select(group => group.FirstOrDefault());
                break;
            case DataFilter.City:
                uniqueItems = db.Arts.Where(item => !string.IsNullOrEmpty(item.City))
                    .GroupBy(item => item.City)
                    .Select(group => group.FirstOrDefault());
                break;
            case DataFilter.Country:
                uniqueItems = db.Arts.Where(item => !string.IsNullOrEmpty(item.Country))
                    .GroupBy(item => item.Country)
                    .Select(group => group.FirstOrDefault());
                break;
        }

        Artists = new(uniqueItems);
        var filterType = $"{Filter}";
        if (Filter == DataFilter.SimplifiedSig)
        {
            filterType = "Artist";
        }

        MessageStatus = $"Total {filterType}: {Artists.Count}";
        IsActive = false;
        ArtistsACV = new AdvancedCollectionView(Artists, true);
    }

    [RelayCommand]
    private void OnGoToDetailPage()
    {
        var item = ListViewSelectedItem as Art;
        if (item != null)
        {
            EntranceNavigationTransitionInfo entranceNavigation = new EntranceNavigationTransitionInfo();
            App.Current.GetJsonNavigationService.NavigateTo(typeof(ArtWorkDetailPage), new ArtWorkNavigationParameter(Filter, item), false, entranceNavigation);
        }
    }

    public void Search()
    {
        if (Artists != null && Artists.Any())
        {
            ArtistsACV.Filter = _ => true;
            ArtistsACV.Filter = ArtistFilter;
        }
    }

    public bool ArtistFilter(object item)
    {
        if (item == null)
            return false;

        var query = (Art)item;
        var txtSearch = MainWindow.Instance.GetAutoSuggestBox();
        switch (Filter)
        {
            case DataFilter.SimplifiedSig:
                return query.SimplifiedSig.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase);
            case DataFilter.Gallery:
                return query.Gallery.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase);
            case DataFilter.City:
                return query.City.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase);
            case DataFilter.Country:
                return query.Country.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase);
        }
        return false;
    }

    public void OnAutoSuggestBoxTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
    {
        Search();
    }

    public void OnAutoSuggestBoxQuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
    }
}
