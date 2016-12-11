using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.WebDav;
using OwncloudUniversal.WebDav.Model;

namespace OwncloudUniversal.Converters
{
    public class ItemToSyncStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var davItem = value as AbstractItem;
            if (davItem != null)
            {
                if (davItem.IsSynced)
                    return davItem.IsCollection ? Symbol.SyncFolder : Symbol.Page2;
                return davItem.IsCollection ? Symbol.Folder : Symbol.Page;

            }
            
            return Symbol.Help;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
