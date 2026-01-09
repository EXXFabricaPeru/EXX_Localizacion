using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Data;

namespace exxis_localizacion.util
{
    public class RegistroDictionaryConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var campos = value as Dictionary<string,object>;
            if (campos != null && parameter != null)
            {
                var valor = campos[parameter.ToString()];
                if (valor != null)
                    return valor;
                return "";
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
