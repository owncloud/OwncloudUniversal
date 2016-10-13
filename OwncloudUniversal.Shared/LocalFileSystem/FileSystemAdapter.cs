using OwncloudUniversal.Model;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.Shared.WebDav;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Search;

namespace OwncloudUniversal.Shared.LocalFileSystem
{
    public class FileSystemAdapter : AbstractAdapter
    {
        private async Task _CheckLocalFolderRecursive(StorageFolder folder, long associationId, List<AbstractItem> result)
        {
            var files = new List<IStorageItem>();
            var options = new QueryOptions();
            options.FolderDepth = FolderDepth.Deep;
            options.IndexerOption = IndexerOption.UseIndexerWhenAvailable;
            //details about filesystem queries
            //https://msdn.microsoft.com/en-us/magazine/mt620012.aspx
            string timeFilter = "System.Search.GatherTime:>=" +
                                Configuration.LastSync;
            options.ApplicationSearchFilter = timeFilter;
            if (folder.AreQueryOptionsSupported(options))
            {
                var queryResult = folder.CreateFileQueryWithOptions(options);
                queryResult.ApplyNewQueryOptions(options);
                files.AddRange(await queryResult.GetFilesAsync());
            }
            else
            {
                var items = await folder.GetItemsAsync();
                files.AddRange(items);
                foreach (IStorageItem sItem in items)
                {
                    if (sItem.IsOfType(StorageItemTypes.Folder))
                        await _CheckLocalFolderRecursive((StorageFolder)sItem, associationId, result);
                }
            }
            if (!IsBackgroundSync)
            {
                var unsynced = AbstractItemTableModel.GetDefault().GetUnsyncedItems().Where(x => x.EntityId.Contains("\\"));
                result.AddRange(unsynced.Select(abstractItem => new LocalItem(abstractItem)));
            }
            foreach (var file in files)
            {
                BasicProperties bp = await file.GetBasicPropertiesAsync();
                var item = new LocalItem(new FolderAssociation { Id = associationId }, file, bp);
                result.Add(item);
            }
        }

        public override async Task<AbstractItem> AddItem(AbstractItem item)
        {
            var folder = await _GetStorageFolder(item);
            IStorageItem storageItem;
            var displayName = _BuildFilePath(item).TrimEnd('\\');
            displayName = displayName.Substring(displayName.LastIndexOf('\\') +1);
            if (item.IsCollection)
            {
                storageItem = await folder.CreateFolderAsync(displayName, CreationCollisionOption.OpenIfExists);
            }
            else
            {
                storageItem = await folder.TryGetItemAsync(displayName);
                if (storageItem == null)
                {
                    storageItem = await folder.CreateFileAsync(displayName, CreationCollisionOption.OpenIfExists);
                    await ConnectionManager.Download(item.EntityId, (StorageFile)storageItem);
                }

            }
            var targetItem = new LocalItem();
            BasicProperties bp = await storageItem.GetBasicPropertiesAsync();
            targetItem = new LocalItem(item.Association, storageItem, bp);
            return targetItem;
        }

        public override async Task<AbstractItem> UpdateItem(AbstractItem item)
        {
            var folder = await _GetStorageFolder(item);
            AbstractItem targetItem = null;
            if (item.IsCollection)
            {
                var bp = await folder.GetBasicPropertiesAsync();
                targetItem = new LocalItem(item.Association, folder, bp);
            }
            else
            {
                var file = await folder.CreateFileAsync(((RemoteItem)item).DavItem.DisplayName, CreationCollisionOption.ReplaceExisting);
                await ConnectionManager.Download(item.EntityId, file);
                var bp = await file.GetBasicPropertiesAsync();
                targetItem = new LocalItem(item.Association, file, bp);
            }
            return targetItem; 
        }

        public override async Task DeleteItem(AbstractItem item)
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
            var item = GetAssociatedItem(association.LocalFolderId);
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(item.EntityId);
            await _CheckLocalFolderRecursive(folder, association.Id, items);
            return items;
        }

        private async Task<StorageFolder> _GetStorageFolder(AbstractItem item)
        {
            string filePath = item.IsCollection ? _BuildFolderPath(item) : _BuildFilePath(item);
            string folderPath = Path.GetDirectoryName(filePath);
            await Task.Run(() => Directory.CreateDirectory(folderPath));
            return await StorageFolder.GetFolderFromPathAsync(folderPath);
        }

        private string _BuildFilePath(AbstractItem item)
        {
            var folderUri = new Uri(GetAssociatedItem(item.Association.LocalFolderId).EntityId);          
            var remoteFolder = GetAssociatedItem(item.Association.RemoteFolderId);
            var relativefileUri = item.EntityId.Replace(remoteFolder.EntityId, ""); 
            string path = Uri.UnescapeDataString(relativefileUri.ToString().Replace('/', '\\'));
            var result = folderUri.LocalPath + '\\' + path;
            return folderUri.LocalPath + '\\' + path;
        }

        private string _BuildFolderPath(AbstractItem item)
        {
            var folderUri = new Uri(GetAssociatedItem(item.Association.LocalFolderId).EntityId);
            var remoteFolder = GetAssociatedItem(item.Association.RemoteFolderId);
            var relativefileUri = item.EntityId.Replace(remoteFolder.EntityId, "");
            relativefileUri = relativefileUri.Remove(relativefileUri.LastIndexOf("/"));
            string relativePath = Uri.UnescapeDataString(relativefileUri.ToString().Replace('/', '\\'));
            var absoltuePath = folderUri.LocalPath + '\\' + relativePath;
            //var result = absoltuePath.Remove(absoltuePath.LastIndexOf("\\"));
            return absoltuePath;
        }

        private AbstractItem GetAssociatedItem(long id)
        {
            return AbstractItemTableModel.GetDefault().GetItem(id);
        }

        public FileSystemAdapter(bool isBackgroundSync) : base(isBackgroundSync)
        {
        }
    }
}
