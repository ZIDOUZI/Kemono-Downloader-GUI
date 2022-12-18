using System.Drawing;
using Microsoft.UI.Xaml.Data;
using Microsoft.UI.Xaml.Media;

namespace Kemono.Helpers.Converters;

public class ColorBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is Color color)
        {
            return new SolidColorBrush(new Windows.UI.Color {A = color.A, R = color.R, G = color.G, B = color.B});
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (parameter is SolidColorBrush brush)
        {
            return Color.FromArgb(brush.Color.A, brush.Color.R, brush.Color.G, brush.Color.B);
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");
    }
}