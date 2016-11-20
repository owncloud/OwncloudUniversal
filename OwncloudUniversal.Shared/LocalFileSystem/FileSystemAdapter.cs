using OwncloudUniversal.Model;
using OwncloudUniversal.Shared.Model;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using Windows.Storage.Provider;
using Windows.Storage.Search;
using OwncloudUniversal.Shared.Synchronisation;

namespace OwncloudUniversal.Shared.LocalFileSystem
{
    public class FileSystemAdapter : AbstractAdapter
    {
        private async Task _GetChangesFromSearchIndex(StorageFolder folder, long associationId,
            List<AbstractItem> result)
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
            options.SetPropertyPrefetch(PropertyPrefetchOptions.BasicProperties, prefetchedProperties);
            if (!folder.AreQueryOptionsSupported(options))
                throw new Exception($"Windows Search Index has to be enabled for {folder.Path}");

            var queryResult = folder.CreateFileQueryWithOptions(options);
            queryResult.ApplyNewQueryOptions(options);
            files.AddRange(await queryResult.GetFilesAsync());

            foreach (var file in files)
            {
                IDictionary<string, object> propertyResult = null;
                if (file.IsOfType(StorageItemTypes.File))
                    propertyResult = await ((StorageFile) file).Properties.RetrievePropertiesAsync(prefetchedProperties);
                else if (file.IsOfType(StorageItemTypes.Folder))
                    propertyResult =
                        await ((StorageFolder) file).Properties.RetrievePropertiesAsync(prefetchedProperties);
                var item = new LocalItem(new FolderAssociation {Id = associationId}, file, propertyResult);
                result.Add(item);
            }

            if (!IsBackgroundSync)
            {
                var unsynced =
                        AbstractItemTableModel.GetDefault().GetPostponedItems().Where(x => x.AdapterType == typeof(FileSystemAdapter));
                foreach (var abstractItem in unsynced)
                {
                    abstractItem.SyncPostponed = false;
                }
                result.AddRange(unsynced);
            }
        }

        private async Task _CheckLocalFolderRecursive(StorageFolder folder, long associationId,
            List<AbstractItem> result)
        {
            var files = await folder.GetItemsAsync();
            foreach (IStorageItem sItem in files)
            {
                if (sItem.IsOfType(StorageItemTypes.Folder))
                    await _CheckLocalFolderRecursive((StorageFolder) sItem, associationId, result);
                BasicProperties bp = await sItem.GetBasicPropertiesAsync();
                var item = new LocalItem(new FolderAssociation {Id = associationId}, sItem, bp);
                result.Add(item);
            }
        }

        public override async Task<AbstractItem> AddItem(AbstractItem item)
        {
            var folder = await _GetStorageFolder(item);
            IStorageItem storageItem;
            var displayName = _BuildFilePath(item).TrimEnd('\\');
            displayName = displayName.Substring(displayName.LastIndexOf('\\') + 1);
            if (item.IsCollection)
            {
                storageItem = await folder.CreateFolderAsync(displayName, CreationCollisionOption.OpenIfExists);
            }
            else
            {
                storageItem = await folder.TryGetItemAsync(displayName);
                if (storageItem == null)
                {
                    item = await LinkedAdapter.GetItem(item.EntityId);
                    storageItem = await folder.CreateFileAsync(displayName, CreationCollisionOption.OpenIfExists);
                    byte[] buffer = new byte[16*1024];
                    using (var stream = await ((StorageFile) storageItem).OpenStreamForWriteAsync())
                    using (item.ContentStream)
                    {
                        int read = 0;
                        while ((read = await item.ContentStream.ReadAsync(buffer, 0, buffer.Length)) > 0)
                        {
                            await stream.WriteAsync(buffer, 0, read);
                        }
                        FileUpdateStatus status =
                            await CachedFileManager.CompleteUpdatesAsync((StorageFile) storageItem);
                        if (status != FileUpdateStatus.Complete)
                            throw new Exception("File incomplete: " + status);
                    }
                }

            }
            BasicProperties bp = await storageItem.GetBasicPropertiesAsync();
            var targetItem = new LocalItem(item.Association, storageItem, bp);
            return targetItem;
        }

        public override async Task<AbstractItem> UpdateItem(AbstractItem item)
        {
            return await AddItem(item);
        }

        public override Task DeleteItem(AbstractItem item)
        {
            throw new NotImplementedException();
        }

        public override async Task<AbstractItem> GetItem(string entityId)
        {
            var file = await StorageFile.GetFileFromPathAsync(entityId);
            BasicProperties bp = await file.GetBasicPropertiesAsync();
            var s = await LocalItem.CreateAsync(file, bp);
            return s;
        }

        public override async Task<List<AbstractItem>> GetUpdatedItems(FolderAssociation association)
        {
            List<AbstractItem> items = new List<AbstractItem>();
            var item = GetAssociatedItem(association.LocalFolderId);
            StorageFolder folder = await StorageFolder.GetFolderFromPathAsync(item.EntityId);
            await _GetChangesFromSearchIndex(folder, association.Id, items);
            //await _CheckLocalFolderRecursive(folder, association.Id, items);
            return items;
        }

        private async Task<StorageFolder> _GetStorageFolder(AbstractItem item)
        {
            try
            {
                string filePath = item.IsCollection ? _BuildFolderPath(item) : _BuildFilePath(item);
                string folderPath = Path.GetDirectoryName(filePath);

                var currentFolder = await StorageFolder.GetFolderFromPathAsync(item.Association.LocalFolderPath);
                string[] folders = folderPath.Replace(currentFolder.Path, "").TrimStart('\\').Split('\\');

                foreach (var folder in folders)
                {
                    if(string.IsNullOrWhiteSpace(folder))
                        continue;
                    IStorageItem tmp = null;
                    tmp = await currentFolder.TryGetItemAsync(folder) ?? await currentFolder.CreateFolderAsync(folder);
                    currentFolder = (StorageFolder)tmp;
                }
                return currentFolder;
            }
            catch (Exception)
            {
                Debug.WriteLine(item.EntityId);
            }
            
            return null;
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
            if(relativefileUri.Contains('/'))
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

        public FileSystemAdapter(bool isBackgroundSync, AbstractAdapter linkedAdapter) : base(isBackgroundSync, linkedAdapter)
        {
        }

        public override async Task<List<AbstractItem>> GetDeletedItemsAsync(FolderAssociation association)
        {
            List<AbstractItem> result = new List<AbstractItem>();
            //get the storagefolder
            var localFolderItem = GetAssociatedItem(association.LocalFolderId);
            StorageFolder sFolder = await StorageFolder.GetFolderFromPathAsync(localFolderItem.EntityId);

            var sItems = new List<IStorageItem>();
            var options = new QueryOptions();
            options.FolderDepth = FolderDepth.Deep;
            options.IndexerOption = IndexerOption.OnlyUseIndexer;
            string timeFilter = "System.Search.GatherTime:>=" + DateTime.MinValue;
            options.ApplicationSearchFilter = timeFilter;
            if (!sFolder.AreQueryOptionsSupported(options))
                throw new Exception($"Windows Search Index has to be enabled for {sFolder.Path}");

            var itemQuery = sFolder.CreateItemQueryWithOptions(options);
            sItems.AddRange(await itemQuery.GetItemsAsync());
            
            //get all the files and folders from the db that were inside the folder at the last time
            var existingItems =
                AbstractItemTableModel.GetDefault()
                    .GetAllItems()
                    .Where(x => x.EntityId.Contains(localFolderItem.EntityId));

            foreach (var existingItem in existingItems)
            {
                if(existingItem.EntityId == sFolder.Path) continue;
                //if a file with that path is in the list, the file has not been deleted
                if(sItems.FirstOrDefault(x => x.Path == existingItem.EntityId) != null) continue;
                result.Add(existingItem);
            }

            return result;
        }

    }
}
