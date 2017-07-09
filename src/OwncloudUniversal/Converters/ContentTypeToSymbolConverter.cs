using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media.Imaging;

namespace OwncloudUniversal.Converters
{
    class ContentTypeToSymbolUriConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                string contentType = (string) value;
                string basepath = "ms-appx:///Assets/FileTypes/";
                var name = Utils.MimetypeIconUtil.GetIconName(contentType);
                var uri = new Uri(basepath + name, UriKind.RelativeOrAbsolute);
                return new BitmapImage(uri);
            }
            return new BitmapImage();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return string.Empty;
        }
    }
}
