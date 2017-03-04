using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using OwncloudUniversal.Model;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.WebDav;
using OwncloudUniversal.WebDav.Model;

namespace OwncloudUniversal.Converters
{
    public class ItemToSyncStatusConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var davItem = value as BaseItem;
            if (davItem != null)
            {
                var dbItem = ItemTableModel.GetDefault().GetItem(davItem);
                if (dbItem == null)
                {
                    foreach (var folderAssociation in FolderAssociationTableModel.GetDefault().GetAllItems())
                    {
                        if (davItem.EntityId.Contains(folderAssociation.RemoteFolderFolderPath))
                            return "/Assets/SyncIcons/ic_synchronizing.png";//not in index yet
                    }
                    return null;
                }

                var link = LinkStatusTableModel.GetDefault().GetItem(dbItem);
                if (link == null)
                    return "/Assets/SyncIcons/ic_synchronizing.png";//indexed but not synced

                var linkedItemId = dbItem.Id == link.SourceItemId ? link.TargetItemId : link.SourceItemId;
                var linkedItem = ItemTableModel.GetDefault().GetItem(linkedItemId);
                if(linkedItem.ChangeNumber != link.ChangeNumber)
                    return "/Assets/SyncIcons/ic_synchronizing.png";


                if (davItem.ChangeKey == dbItem.ChangeKey && link.ChangeNumber == dbItem.ChangeNumber)
                    return "/Assets/SyncIcons/ic_synced.png";
                
                return "/Assets/SyncIcons/ic_synchronizing.png";
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return null;
        }
    }
}
