using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace OwncloudUniversal.Converters
{
    class BoolToCollectionSymbolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool) return (bool) value ? Symbol.Folder : Symbol.Page2;
            return Symbol.Help;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return value as Symbol? == Symbol.SyncFolder;
        }
    }
}
