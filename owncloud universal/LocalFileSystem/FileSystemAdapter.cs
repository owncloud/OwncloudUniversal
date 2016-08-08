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
        private async Task ScanLocalFolder(StorageFolder localFolder, long associationId)
        {
            var files = await localFolder.GetItemsAsync();
            foreach (IStorageItem sItem in files)
            {
                BasicProperties bp = await sItem.GetBasicPropertiesAsync();
                LocalItem li = new LocalItem();
                li.FolderId = associationId;
                li.IsCollection = sItem is StorageFolder;
                li.LastModified = bp.DateModified.LocalDateTime;
                li.Path = sItem.Path;

                var itemsInDatabase = LocalItemTableModel.GetDefault().SelectByPath(li.Path, li.FolderId);
                if (itemsInDatabase.Count == 0)
                {
                    LocalItemTableModel.GetDefault().InsertItem(li);
                    if (sItem is StorageFolder)
                        await ScanLocalFolder((StorageFolder)sItem, associationId);
                    continue;
                }

                foreach (var itemInDatabase in itemsInDatabase)
                {
                    if (itemInDatabase.LastModified.Equals(li.LastModified))
                    {
                        LocalItemTableModel.GetDefault().UpdateItem(li, itemInDatabase.Id);
                        if (sItem is StorageFolder)
                            await ScanLocalFolder((StorageFolder)sItem, associationId);
                    }
                }
            }
        }
        private async Task CheckLocalFolderRecursive(StorageFolder folder, long associationId, List<AbstractItem> result)
        {
            var files = await folder.GetItemsAsync();
            foreach (IStorageItem sItem in files)
            {
                if (sItem.IsOfType(StorageItemTypes.Folder))
                    await CheckLocalFolderRecursive((StorageFolder)sItem, associationId, result);
                BasicProperties bp = await sItem.GetBasicPropertiesAsync();
                LocalItem li = new LocalItem();
                li.FolderId = associationId;
                li.IsCollection = sItem is StorageFolder;
                li.LastModified = bp.DateModified.LocalDateTime;
                li.Path = sItem.Path;
                result.Add(li);
            }
        }
        private async Task<List<LocalItem>> GetDataToUpload(FolderAssociation association)
        {
            List<LocalItem> result = new List<LocalItem>();
            StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(association.LocalFolder.Path);
            await CheckLocalFolderRecursive(localFolder, association.Id, result);
            return result;
        }

        public override async void AddItem(AbstractItem item)
        {
            var _item = (RemoteItem)item;
            var folder = await GetStorageFolder(_item);
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
            var folder = await GetStorageFolder(_item);
            await folder.CreateFileAsync(_item.DavItem.DisplayName, CreationCollisionOption.ReplaceExisting);
        }

        public override async void DeleteItem(AbstractItem item)
        {
            var _item = (RemoteItem)item;
            var folder = await GetStorageFolder(_item);
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

        public override async void GetAllItems(FolderAssociation association, out List<AbstractItem> items)
        {
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(association.LocalFolder.Path);
            items = new List<AbstractItem>();
            await CheckLocalFolderRecursive(folder, association.Id, items);
        }

        public override void UpdateIndexes()
        {
            throw new NotImplementedException();
        }

        private async Task<StorageFolder> GetStorageFolder(RemoteItem item)
        {
            string filePath = item.IsCollection ? BuildFolderPath(item) : BuildFilePath(item);
            string folderPath = Path.GetDirectoryName(filePath);
            return await StorageFolder.GetFolderFromPathAsync(folderPath);
        }

        private string BuildFilePath(RemoteItem item)
        {
            Uri serverUri = new Uri(Configuration.ServerUrl);
            Uri folderUri = new Uri(serverUri, item.Association.RemoteFolder.DavItem.Href);
            Uri fileUri = new Uri(serverUri, item.DavItem.Href);
            Uri relativeUri = folderUri.MakeRelativeUri(fileUri);
            string path = Uri.UnescapeDataString(relativeUri.ToString().Replace('/', '\\'));
            return item.Association.LocalFolder.Path + '\\' + path;
        }

        private string BuildFolderPath(RemoteItem item)
        {
            throw new NotImplementedException();
        }
    }
}
