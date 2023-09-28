using Microsoft.UI.Xaml.Data;

namespace ArtWork.Common;
public class FileFolderPathToRealPathConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (value != null && value is string fileFolderPath)
        {
            return @$"Path: ({Settings.ArtWorkDirectory}\{fileFolderPath})";
        }
        return value;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        throw new NotImplementedException();
    }
}
