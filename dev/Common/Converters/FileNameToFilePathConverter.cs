using Microsoft.UI.Xaml.Data;

namespace ArtWork.Common;
public class FileNameToFilePathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value == null) return null;
        var fileName = (string)value;
        var uri = new Uri(Path.Combine(Settings.ArtWorkDirectory, fileName));
        return uri;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
