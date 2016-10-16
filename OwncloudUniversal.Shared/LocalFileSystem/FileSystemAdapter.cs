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
using OwncloudUniversal.Shared.Synchronisation;

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
            //details about filesystem queries using the indexer
            //https://msdn.microsoft.com/en-us/magazine/mt620012.aspx
            string timeFilter = "System.Search.GatherTime:>=" + Configuration.LastSync;
            options.ApplicationSearchFilter = timeFilter;
            var prefetchedProperties = new List<string> {"System.DateModified", "System.Size"};
            options.SetPropertyPrefetch(PropertyPrefetchOptions.None, prefetchedProperties);
            if (!folder.AreQueryOptionsSupported(options))
                throw new Exception($"Windows Search Index has to be enabled for {folder.Path}");

            var queryResult = folder.CreateFileQueryWithOptions(options);
            queryResult.ApplyNewQueryOptions(options);
            files.AddRange(await queryResult.GetFilesAsync());

            foreach (var file in files)
            {
                IDictionary<string, object> propertyResult = null;
                if (file.IsOfType(StorageItemTypes.File))
                    propertyResult = await ((StorageFile)file).Properties.RetrievePropertiesAsync(prefetchedProperties);
                else if (file.IsOfType(StorageItemTypes.Folder))
                    propertyResult = await ((StorageFolder)file).Properties.RetrievePropertiesAsync(prefetchedProperties);
                var item = new LocalItem(new FolderAssociation { Id = associationId }, file, propertyResult);
                result.Add(item);
            }

            if (!IsBackgroundSync)
            {
                var unsynced =
                    AbstractItemTableModel.GetDefault().GetPostponedItems().Where(x => x.EntityId.Contains("\\"));//TODO find a better way
                result.AddRange(unsynced.Select(abstractItem => new LocalItem(abstractItem) {SyncPostponed = false}));
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
            ExecutionContext.Status = ExecutionStatus.Scanning;
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
