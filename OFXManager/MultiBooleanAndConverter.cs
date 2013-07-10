using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Data;
using System.Globalization;

namespace OFXManager
{
    class MultiBooleanAndConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            for (int i=0;i<values.Length;i++)
            {
                try
                {
                    if (!System.Convert.ToBoolean(values[i])) return false;
                }

                catch (InvalidCastException)
                {
                    return false;
                }


            }


            return true;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new InvalidOperationException("MultiBooleanAndConverter can only be used OneWay.");
        }
       
    }
}
