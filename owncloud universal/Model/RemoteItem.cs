using owncloud_universal.WebDav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace owncloud_universal.Model
{
    public class RemoteItem
    {
        public long Id { get; set; }
        public long AssocaitionId { get; set; }
        public RemoteItem(DavItem davItem)
        {
            DavItem = davItem;
        }

        public DavItem DavItem { get; private set; }
        public Symbol Symbol { get {return DavItem.IsCollection ? Symbol.Folder : Symbol.Page2; }}
    }
}
