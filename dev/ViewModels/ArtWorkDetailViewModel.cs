using ArtWork.Database;
using ArtWork.Database.Tables;

using CommunityToolkit.WinUI.UI;

namespace ArtWork.ViewModels;
public partial class ArtWorkDetailViewModel : ObservableRecipient, INavigationAware
{
    [ObservableProperty]
    private ObservableCollection<Art> arts;

    [ObservableProperty]
    private AdvancedCollectionView artsACV;

    public void OnNavigatedFrom()
    {
    }

    public void OnNavigatedTo(object parameter)
    {
        var simplifiedSig = parameter as string;
        using var db = new ArtWorkDbContext();
        var items = db.Arts.Where(x => x.SimplifiedSig.Equals(simplifiedSig));
        Arts = new(items);
        ArtsACV = new AdvancedCollectionView(Arts, true);
    }
}
