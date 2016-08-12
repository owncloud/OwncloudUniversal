using owncloud_universal.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace owncloud_universal.LocalFileSystem
{
    class FileSystemAdapter : AbstractAdapter
    {
        private async Task _CheckLocalFolderRecursive(StorageFolder folder, long associationId, List<AbstractItem> result)
        {
            var files = await folder.GetItemsAsync();
            foreach (IStorageItem sItem in files)
            {
                if (sItem.IsOfType(StorageItemTypes.Folder))
                    await _CheckLocalFolderRecursive((StorageFolder)sItem, associationId, result);
                BasicProperties bp = await sItem.GetBasicPropertiesAsync();
                LocalItem li = new LocalItem();
                li.Association = new FolderAssociation { Id = associationId };
                li.IsCollection = sItem is StorageFolder;
                li.LastModified = bp.DateModified.LocalDateTime;
                li.Path = sItem.Path;
                result.Add(li);
            }
        }
        private async Task<List<AbstractItem>> GetDataToUpload(FolderAssociation association)
        {
            List<AbstractItem> result = new List<AbstractItem>();
            StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(association.LocalFolder.Path);
            await _CheckLocalFolderRecursive(localFolder, association.Id, result);
            return result;
        }

        public override async void AddItem(AbstractItem item)
        {
            var _item = (RemoteItem)item;
            var folder = await _GetStorageFolder(_item);
            if (item.IsCollection)
            {
                var f = await folder.CreateFolderAsync(_item.DavItem.DisplayName, CreationCollisionOption.OpenIfExists);
            }
            else
            {
                var file = await folder.CreateFileAsync(_item.DavItem.DisplayName, CreationCollisionOption.OpenIfExists);
                var result = await ConnectionManager.Download(_item.DavItem.Href, file);
            }
        }

        public override async void UpdateItem(AbstractItem item)
        {
            var _item = (RemoteItem)item;
            if (item.IsCollection)
            {
                //nothing to do
                return;
            }
            var folder = await _GetStorageFolder(_item);
            await folder.CreateFileAsync(_item.DavItem.DisplayName, CreationCollisionOption.ReplaceExisting);
        }

        public override async void DeleteItem(AbstractItem item)
        {
            var _item = (RemoteItem)item;
            var folder = await _GetStorageFolder(_item);
            if (_item.IsCollection)
            {
                await folder.DeleteAsync(StorageDeleteOption.Default);
            }
            else
            {
                var file = await folder.GetFileAsync(_item.DavItem.DisplayName);
                await file.DeleteAsync(StorageDeleteOption.Default);
            }
        }

        public override async Task<List<AbstractItem>> GetAllItems(FolderAssociation association)
        {
            List<AbstractItem> items = new List<AbstractItem>();
            StorageFolder folder = StorageFolder.GetFolderFromPathAsync(association.LocalFolder.Path).GetResults();
            items = new List<AbstractItem>();
            await _CheckLocalFolderRecursive(folder, association.Id, items);
            return items;
        }

        private async Task<StorageFolder> _GetStorageFolder(RemoteItem item)
        {
            string filePath = item.IsCollection ? _BuildFolderPath(item) : _BuildFilePath(item);
            string folderPath = Path.GetDirectoryName(filePath);
            return await StorageFolder.GetFolderFromPathAsync(folderPath);
        }

        private string _BuildFilePath(RemoteItem item)
        {
            Uri serverUri = new Uri(Configuration.ServerUrl);
            Uri folderUri = new Uri(serverUri, item.Association.RemoteFolder.DavItem.Href);
            Uri fileUri = new Uri(serverUri, item.DavItem.Href);
            Uri relativeUri = folderUri.MakeRelativeUri(fileUri);
            string path = Uri.UnescapeDataString(relativeUri.ToString().Replace('/', '\\'));
            return item.Association.LocalFolder.Path + '\\' + path;
        }

        private string _BuildFolderPath(RemoteItem item)
        {
            Uri serverUri = new Uri(Configuration.ServerUrl);
            Uri folderUri = new Uri(serverUri, item.Association.RemoteFolder.DavItem.Href);
            return Uri.UnescapeDataString(folderUri.ToString().Replace('/', '\\'));            
        }
    }
}
