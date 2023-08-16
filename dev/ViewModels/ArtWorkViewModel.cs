using ArtWork.Database;
using ArtWork.Database.Tables;

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

    private IJsonNavigationViewService jsonNavigationViewService;

    public ArtWorkViewModel(IJsonNavigationViewService jsonNavigationViewService)
    {
        this.jsonNavigationViewService = jsonNavigationViewService;

        using var db = new ArtWorkDbContext();

        var uniqueItems = db.Arts.GroupBy(item => item.SimplifiedSig)
                               .Select(group => group.FirstOrDefault());
        Artists = new(uniqueItems);
        ArtistsACV = new AdvancedCollectionView(Artists, true);
        MessageStatus = $"Total Artists: {Artists.Count}";
    }

    [RelayCommand]
    private void OnGoToDetailPage(SelectionChangedEventArgs e)
    {
        var item = e.AddedItems[0] as Art;
        if (item != null)
        {
            EntranceNavigationTransitionInfo entranceNavigation = new EntranceNavigationTransitionInfo();
            jsonNavigationViewService.NavigateTo(typeof(ArtWorkDetailPage), item.SimplifiedSig, false, entranceNavigation);
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
        return query.SimplifiedSig.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase);
    }
}
