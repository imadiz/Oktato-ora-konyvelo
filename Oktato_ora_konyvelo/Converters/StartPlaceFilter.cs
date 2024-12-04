using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Avalonia.Data.Converters;
using Oktato_ora_konyvelo.Classes;

namespace Oktato_ora_konyvelo.Converters;

public class StartPlaceFilter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        ObservableCollection<Place>? StartPlaces = value as ObservableCollection<Place>;//Input data conversion
        return StartPlaces.Where(x=>x.IsStartPlace);//Filter to only start places from all places
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();//Unused
    }
}