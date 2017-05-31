using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Data;
using OwncloudUniversal.WebDav.Model;

namespace OwncloudUniversal.Converters
{
    class InvertedItemToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is DavItem)
                if ((value as DavItem).IsCollection)
                    if (!(value as DavItem).IsSynced)
                        return Visibility.Visible;
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value as Visibility? == Visibility.Collapsed;
        }
    }
}
