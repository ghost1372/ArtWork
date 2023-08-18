using ArtWork.Database.Tables;

namespace ArtWork.Models;
public class ArtWorkNavigationParameter
{
    public DataFilter DataFilter { get; set; }
    public Art Art { get; set; }

    public ArtWorkNavigationParameter(DataFilter dataFilter, Art art)
    {
        DataFilter = dataFilter;
        Art = art;
    }
}
