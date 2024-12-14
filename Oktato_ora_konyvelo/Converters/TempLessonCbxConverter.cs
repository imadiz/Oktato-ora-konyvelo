using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Oktato_ora_konyvelo.Classes;
using Oktato_ora_konyvelo.ViewModels;

namespace Oktato_ora_konyvelo.Converters;

public class TempLessonCbxConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        //Itt a bejött value a Combobox-ban kiválasztott string.
        switch (value.ToString())
        {
            case "Alap":
                return LessonType.A;
            case "Városi":
                return LessonType.Fv;
            case "Országúti":
                return LessonType.Fo;
            case "Éjszakai":
                return LessonType.Fe;
            default:
                return null;
        }
    }
}