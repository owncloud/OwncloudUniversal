using OwncloudUniversal.Shared.WebDav;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;
using OwncloudUniversal.Shared.Model;

namespace OwncloudUniversal.Shared.Model
{
    public class RemoteItem : AbstractItem
    {
        public RemoteItem(DavItem davItem)
        {
            DavItem = davItem;
        }
        public DavItem DavItem { get; private set; }
        public Symbol Symbol { get { return DavItem.IsCollection ? Symbol.Folder : Symbol.Page2; } }

        public override ulong Size
        {
            get
            {
                if (DavItem.ContentLength != null) return (ulong)DavItem.ContentLength.Value;
                return 0;
            }
        }
        public override bool IsCollection
        {
            get
            {
                return DavItem.IsCollection;
            }

            set
            {
                DavItem.IsCollection = value;
            }
        }
        public override string EntityId
        {
            get
            {
                return DavItem.Href;
            }

            set
            {
                DavItem.Href = value;
            }
        }
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