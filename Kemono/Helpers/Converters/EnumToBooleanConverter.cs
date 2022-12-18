﻿using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Data;

namespace Kemono.Helpers.Converters;

public class EnumToBooleanConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language)
    {
        if (parameter is not string enumString)
        {
            throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
        }

        if (!Enum.IsDefined(typeof(ElementTheme), value))
        {
            throw new ArgumentException("ExceptionEnumToBooleanConverterValueMustBeAnEnum");
        }

        var enumValue = Enum.Parse(typeof(ElementTheme), enumString);

        return enumValue.Equals(value);
    }

    public object ConvertBack(object value, Type targetType, object parameter, string language)
    {
        if (parameter is string enumString)
        {
            return Enum.Parse(typeof(ElementTheme), enumString);
        }

        throw new ArgumentException("ExceptionEnumToBooleanConverterParameterMustBeAnEnumName");
    }
}