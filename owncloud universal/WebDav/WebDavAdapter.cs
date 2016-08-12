using owncloud_universal.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace owncloud_universal.WebDav
{
    class WebDavAdapter : AbstractAdapter
    {
        public override async void AddItem(AbstractItem item)
        {
            var _item = (LocalItem)item;
            if (_item.IsCollection)
            {
                string path = _BuildRemoteFolderPath(_item.Association, _item.Path);
                string name = (await StorageFolder.GetFolderFromPathAsync(_item.Path)).DisplayName;
                ConnectionManager.CreateFolder(path, name);
                return;
            }
            StorageFile file = await StorageFile.GetFileFromPathAsync(_item.Path);
            var stream = await file.OpenStreamForReadAsync();
            await ConnectionManager.Upload(_BuildRemoteFilePath(_item.Association, file.Path), stream, file.DisplayName);
            var remoteItem = ConnectionManager.GetFolder(_BuildRemoteFilePath(_item.Association, file.Path));

            LocalItemTableModel.GetDefault().UpdateItem(_item, _item.EntityId);
        }

        public override void UpdateItem(AbstractItem item)
        {
            AddItem(item);
        }

        public override async void DeleteItem(AbstractItem item)
        {
            var _item = (LocalItem)item;
            if (_item.IsCollection)
            {
                string path = _BuildRemoteFolderPath(_item.Association, _item.Path);
                string name = (await StorageFolder.GetFolderFromPathAsync(_item.Path)).DisplayName;
                ConnectionManager.DeleteFolder(_item.Path +'/'+ name);
                return;
            }
            else
            {
                ConnectionManager.DeleteFile(_BuildRemoteFilePath(_item.Association, _item.Path));
            }
        }

        public override async Task<List<AbstractItem>> GetAllItems(FolderAssociation association)
        {
            List<AbstractItem> items = new List<AbstractItem>();
            await _CheckRemoteFolderRecursive(association, items);
            return items;
        }
        private async Task _CheckRemoteFolderRecursive(FolderAssociation association, List<AbstractItem> result)
        {
            List<RemoteItem> items = await ConnectionManager.GetFolder(association.RemoteFolder.DavItem.Href);
            foreach (RemoteItem item in items)
            {
                if (item.DavItem.IsCollection)
                    await _CheckRemoteFolderRecursive(association, result);
                result.Add(item);
            }
        }
        private string _BuildRemoteFilePath(FolderAssociation association, string path)
        {
            Uri baseUri = new Uri(association.LocalFolder.Path);
            Uri fileUri = new Uri(path);
            Uri relativeUri = baseUri.MakeRelativeUri(fileUri);
            string uri = relativeUri.ToString();
            var relativeString = uri.Substring(uri.IndexOf('/') + 1);
            return association.RemoteFolder.DavItem.Href + relativeString;
        }
        private string _BuildRemoteFolderPath(FolderAssociation association, string path)
        {
            Uri baseUri = new Uri(association.LocalFolder.Path);
            Uri fileUri = new Uri(path);
            Uri relativeUri = baseUri.MakeRelativeUri(fileUri);
            string uri = relativeUri.ToString();
            var relativeString = uri.Remove(uri.LastIndexOf('/'));
            return association.RemoteFolder.DavItem.Href + relativeString;
        }
    }
}
