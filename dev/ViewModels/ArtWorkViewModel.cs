using ArtWork.Database;
using ArtWork.Database.Tables;

namespace ArtWork.ViewModels;
public partial class ArtWorkViewModel : ObservableRecipient
{
    [ObservableProperty]
    private ObservableCollection<Art> artists = new ObservableCollection<Art>();

    public ArtWorkViewModel()
    {
        using var db = new ArtWorkDbContext();

        var uniqueItems = db.Arts.GroupBy(item => item.SimplifiedSig)
                               .Select(group => group.FirstOrDefault());
        Artists = new(uniqueItems);
    }
}
