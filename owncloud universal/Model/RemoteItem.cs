using owncloud_universal.WebDav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace owncloud_universal.Model
{
    public class RemoteItem : AbstractItem
    {
        public RemoteItem(DavItem davItem)
        {
            DavItem = davItem;
        }
        public DavItem DavItem { get; private set; }
        public override string ChangeKey
        {
            get
            {
                return DavItem.Etag;
            }

            set
            {
                DavItem.Etag = value;
            }
        }
    }
}