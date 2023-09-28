using ArtWork.Database;
using ArtWork.Database.Tables;
using ArtWork.Models;

using Microsoft.EntityFrameworkCore;

namespace ArtWork.ViewModels;
public partial class ArtWorkDetailViewModel : ObservableRecipient, INavigationAware
{
    [ObservableProperty]
    private ObservableCollection<Art> arts;

    private ArtWorkNavigationParameter artWorkNavigationParameter;

    public void OnNavigatedFrom()
    {
    }

    public void OnNavigatedTo(object parameter)
    {
        artWorkNavigationParameter = parameter as ArtWorkNavigationParameter;

        if (Arts == null)
        {
            GetArts(Settings.IsShowNudes);
        }
    }

    public void GetArts(bool isShowNudes)
    {
        IQueryable<Art> items = null;
        using var db = new ArtWorkDbContext();
        switch (artWorkNavigationParameter.DataFilter)
        {
            case DataFilter.SimplifiedSig:
                items = db.Arts.Where(x => x.SimplifiedSig.Equals(artWorkNavigationParameter.Art.SimplifiedSig));
                break;
            case DataFilter.Gallery:
                items = db.Arts.Where(x => x.Gallery.Equals(artWorkNavigationParameter.Art.Gallery));
                break;
            case DataFilter.City:
                items = db.Arts.Where(x => x.City.Equals(artWorkNavigationParameter.Art.City));
                break;
            case DataFilter.Country:
                items = db.Arts.Where(x => x.Country.Equals(artWorkNavigationParameter.Art.Country));
                break;
        }

        if (!isShowNudes)
        {
            items = items.Where(x => x.IsNude == false);
        }

        items = items.OrderBy(x => EF.Functions.Random());
        Arts = new(items);
    }
}
