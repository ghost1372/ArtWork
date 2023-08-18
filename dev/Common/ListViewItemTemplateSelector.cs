namespace ArtWork.Common;
public class ListViewItemTemplateSelector : DataTemplateSelector
{
    public DataTemplate SimplifiedSigTemplate { get; set; }
    public DataTemplate GalleryTemplate { get; set; }
    public DataTemplate CityTemplate { get; set; }
    public DataTemplate CountryTemplate { get; set; }

    protected override DataTemplate SelectTemplateCore(object item)
    {
        var instance = ArtWorkPage.Instance;
        if (instance != null && instance.ViewModel != null)
        {
            switch (instance.ViewModel.Filter)
            {
                case Models.DataFilter.SimplifiedSig:
                    return SimplifiedSigTemplate;
                case Models.DataFilter.Gallery:
                    return GalleryTemplate;
                case Models.DataFilter.City:
                    return CityTemplate;
                case Models.DataFilter.Country:
                    return CountryTemplate;
            }
        }
        return base.SelectTemplateCore(item);
    }
}
