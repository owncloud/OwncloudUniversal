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
        public RemoteItem(DavItem davItem)
        {
            DavItem = davItem;
        }
        public long Id { get; set; }
        public long FolderId { get; set; }
        public DavItem DavItem { get; private set; }
        public Symbol Symbol { get {return DavItem.IsCollection ? Symbol.Folder : Symbol.Page2; }}
        public string GetRelativePath(string basePath)
        {
            return DavItem.Href.Remove(0, basePath.Length);
        }
    }
}