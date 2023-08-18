using ArtWork.Database;
using ArtWork.Database.Tables;
using ArtWork.Models;

using CommunityToolkit.WinUI.UI;

using Microsoft.UI.Xaml.Media.Animation;

namespace ArtWork.ViewModels;
public partial class ArtWorkViewModel : ObservableRecipient
{
    [ObservableProperty]
    private ObservableCollection<Art> artists;

    [ObservableProperty]
    private AdvancedCollectionView artistsACV;

    [ObservableProperty]
    private string messageStatus;

    [ObservableProperty]
    private object selectedItem = "SimplifiedSig";

    [ObservableProperty]
    private string displayMemberPath = "SimplifiedSig";

    public DataFilter Filter { get; set; }
    private IJsonNavigationViewService jsonNavigationViewService;

    public ArtWorkViewModel(IJsonNavigationViewService jsonNavigationViewService)
    {
        this.jsonNavigationViewService = jsonNavigationViewService;

        OnComboBoxItemChanged();
    }

    [RelayCommand]
    private void OnComboBoxItemChanged()
    {
        IsActive = true;
        var item = SelectedItem as ComboBoxItem;
        Filter = DataFilter.SimplifiedSig;
        if (item != null)
        {
            Filter = ApplicationHelper.GetEnum<DataFilter>(item.Tag.ToString());
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
        DisplayMemberPath = Filter.ToString();
        Artists = new(uniqueItems);
        MessageStatus = $"Total {Filter}: {Artists.Count}";
        IsActive = false;
        ArtistsACV = new AdvancedCollectionView(Artists, true);
    }

    [RelayCommand]
    private void OnGoToDetailPage(SelectionChangedEventArgs e)
    {
        var item = e.AddedItems[0] as Art;
        if (item != null)
        {
            EntranceNavigationTransitionInfo entranceNavigation = new EntranceNavigationTransitionInfo();
            jsonNavigationViewService.NavigateTo(typeof(ArtWorkDetailPage), new ArtWorkNavigationParameter(Filter, item), false, entranceNavigation);
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
        var txtSearch = MainPage.Instance.GetAutoSuggestBox();
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
}
