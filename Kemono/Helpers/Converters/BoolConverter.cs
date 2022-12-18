using Microsoft.UI.Xaml.Data;

namespace Kemono.Helpers.Converters;

public class BoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) => value is false;

    public object ConvertBack(object value, Type targetType, object parameter, string language) => value is false;
}