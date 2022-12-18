using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Kemono.Helpers.Converters;

public class EnumToStringConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is not ElementTheme theme)
        {
            throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
        }

        return theme.ToString();
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string enumString)
        {
            return Enum.Parse<ElementTheme>(enumString);
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");
    }
}