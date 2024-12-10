using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Oktato_ora_konyvelo.Classes;

namespace Oktato_ora_konyvelo.Converters;

public class StudentLessonsConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value is null ? null : (value as Student).StudentLessons;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}