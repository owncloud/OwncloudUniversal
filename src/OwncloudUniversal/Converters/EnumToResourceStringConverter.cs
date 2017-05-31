using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace OwncloudUniversal.Converters
{
    class EnumToResourceStringConverter :IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            { return null; }

            return App.ResourceLoader.GetString(value.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            var str = (string)value;

            foreach (var enumValue in Enum.GetValues(targetType))
            {
                if (str == App.ResourceLoader.GetString(enumValue.ToString()))
                { return enumValue; }
            }
            throw new ArgumentException(str);
        }
    }
}
