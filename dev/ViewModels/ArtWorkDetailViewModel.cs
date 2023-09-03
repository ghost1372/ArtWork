using ArtWork.Database;
using ArtWork.Database.Tables;
using ArtWork.Models;

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
        var artParameter = parameter as ArtWorkNavigationParameter;
        IQueryable<Art> items = null;
        using var db = new ArtWorkDbContext();
        switch (artParameter.DataFilter)
        {
            case DataFilter.SimplifiedSig:
                items = db.Arts.Where(x => x.SimplifiedSig.Equals(artParameter.Art.SimplifiedSig));
                break;
            case DataFilter.Gallery:
                items = db.Arts.Where(x => x.Gallery.Equals(artParameter.Art.Gallery));
                break;
            case DataFilter.City:
                items = db.Arts.Where(x => x.City.Equals(artParameter.Art.City));
                break;
            case DataFilter.Country:
                items = db.Arts.Where(x => x.Country.Equals(artParameter.Art.Country));
                break;
        }
        
        Arts = new(items);
        ArtsACV = new AdvancedCollectionView(Arts, true);
    }
}
