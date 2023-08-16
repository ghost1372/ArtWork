using ArtWork.Database;
using ArtWork.Database.Tables;

using CommunityToolkit.WinUI.UI;

namespace ArtWork.ViewModels;
public partial class ArtWorkViewModel : ObservableRecipient
{
    [ObservableProperty]
    private ObservableCollection<Art> artists;

    [ObservableProperty]
    private AdvancedCollectionView artistsACV;

    [ObservableProperty]
    private string messageStatus; 

    public ArtWorkViewModel()
    {
        using var db = new ArtWorkDbContext();

        var uniqueItems = db.Arts.GroupBy(item => item.SimplifiedSig)
                               .Select(group => group.FirstOrDefault());
        Artists = new(uniqueItems);
        ArtistsACV = new AdvancedCollectionView(Artists, true);
        MessageStatus = $"Total Artists: {Artists.Count}";
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
        var query = (Art)item;
        var txtSearch = MainPage.Instance.GetAutoSuggestBox();
        return query.SimplifiedSig.Contains(txtSearch.Text, StringComparison.OrdinalIgnoreCase);
    }
}
