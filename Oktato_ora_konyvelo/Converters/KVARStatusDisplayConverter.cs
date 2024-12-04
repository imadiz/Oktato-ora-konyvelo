using Avalonia.Data.Converters;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Oktato_ora_konyvelo.Converters
{
    public class KVARStatusDisplayConverter : IValueConverter
    {
        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool KVARStatus = System.Convert.ToBoolean(value);
            
            if (KVARStatus)
                return "Rögzítve";
            else
                return "Nincs rögzítve";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}