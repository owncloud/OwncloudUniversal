using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace OwncloudUniversal.Converters
{
    class ContentTypeToSymbolUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                string contentType = (string) value;
                string basepath = "/Assets/FileTypes/";
                var name = Utils.MimetypeIconUtil.GetIconName(contentType);
                return basepath + name;
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return string.Empty;
        }
    }
}
