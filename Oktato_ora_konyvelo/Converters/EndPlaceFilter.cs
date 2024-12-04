using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Oktato_ora_konyvelo.Classes;

namespace Oktato_ora_konyvelo.Converters;

public class EndPlaceFilter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        ObservableCollection<Place>? EndPlaces = (value as ObservableCollection<Place>);//Input data conversion
        return EndPlaces.Where(x=>x.IsEndPlace);//Filter to only end places from all places
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();//Unused
    }
}