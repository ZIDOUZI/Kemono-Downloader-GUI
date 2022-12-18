using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Kemono.Helpers.Converters;

public class EnumToIntConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is not ElementTheme theme)
        {
            throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
        }

        return (int)theme;
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (parameter is int enumInt)
        {
            return (ElementTheme)enumInt;
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");
    }
}