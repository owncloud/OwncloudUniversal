using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using owncloud_universal.Model;
using owncloud_universal.WebDav;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Search;
using Windows.UI.Popups;
using Windows.Storage.FileProperties;
using System.Diagnostics;

namespace owncloud_universal
{
    class SyncWorker
    {
        public async Task Run()
        {
            var items = FolderAssociationTableModel.GetDefault().GetAllItems();
            foreach (var item in items)
            {
                await UpdateItemInfos(item);
            }
        }

        private async Task UpdateItemInfos(FolderAssociation association)
        {
            StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(association.LocalFolder.Path);
            await ScanLocalFolder(localFolder, association.Id);
            await ScanRemoteFolder(association.RemoteFolder, association.Id);
            GetDataToUpdate(association);





            //var properties = await localFolder.Properties.RetrievePropertiesAsync(new List<string> { "System.DateModified" });
            //association.LocalFolder.LastModified = ((DateTimeOffset)properties["System.DateModified"]).LocalDateTime;

            //association.RemoteItem.DavItem.LastModified = remoteFolder
        }

        private async Task ScanRemoteFolder(RemoteItem remoteFolder, long associationId)
        {
            List<RemoteItem> items = await ConnectionManager.GetFolder(remoteFolder.DavItem.Href);
            foreach (RemoteItem item in items)
            {
                RemoteItem ri = new RemoteItem(item.DavItem);
                ri.FolderId = associationId;
                var foundItems = RemoteItemTableModel.GetDefault().SelectByPath(ri.DavItem.Href, ri.FolderId);
                if(foundItems.Count == 0)
                {
                    RemoteItemTableModel.GetDefault().InsertItem(ri);
                    if (!ri.DavItem.IsCollection) continue;
                        await ScanRemoteFolder(item, associationId);
                    continue;
                }
                    
                foreach (var foundItem in foundItems)
                {
                    if(foundItem.DavItem.Etag != ri.DavItem.Etag)
                    {
                        RemoteItemTableModel.GetDefault().UpdateItem(ri, foundItem.Id);
                        Debug.Write(string.Format("Updating Database {0}", foundItem.Id));
                        if (!ri.DavItem.IsCollection) continue;
                            await ScanRemoteFolder(item, associationId);
                    }
                }

            }
        }

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
                if(itemsInDatabase.Count == 0)
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
                        Debug.Write(string.Format("Updating Database {0}", itemInDatabase.Id));
                        if (sItem is StorageFolder)
                            await ScanLocalFolder((StorageFolder)sItem, associationId);
                    }
                }
            }
        }

        private async void UploadItems(FolderAssociation item)
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(item.LocalFolder.Path);               
            var files = await folder.GetFilesAsync();
            var list = await ConnectionManager.GetFolder(item.RemoteFolder.DavItem.Href);

            foreach (StorageFile file in files)
            {
                bool upload = true;
                foreach (var remoteItem in list)
                {
                    if (remoteItem.DavItem.DisplayName == file.Name)
                        upload = false;
                }
                if (!upload)
                    continue;
                var stream = await file.OpenStreamForReadAsync();
                await ConnectionManager.Upload(item.RemoteFolder.DavItem.Href, stream, file.Name);
            }
            MessageDialog d = new MessageDialog("Sync finished");
            await d.ShowAsync();
        }

        private Dictionary<LocalItem, RemoteItem> GetDataToUpdate(FolderAssociation associtation)
        {
            var localItems = LocalItemTableModel.GetDefault().GetAllItems();
            var remoteItems = RemoteItemTableModel.GetDefault().GetAllItems();
            foreach(RemoteItem ri in remoteItems)
            {
                var path = ri.GetRelativePath(associtation.RemoteFolder.DavItem.Href);
            }
            foreach (var li in localItems)
            {
                Debug.WriteLine(li.GetRelavtivePath(associtation.LocalFolder.Path));
            }
            return null;
        }

    }
}
