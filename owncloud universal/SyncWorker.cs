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

namespace owncloud_universal
{
    class SyncWorker
    {
        public void Run()
        {
            var items = FolderAssociationTableModel.GetDefault().GetAllItems();
            foreach (var item in items)
            {
                UpdateItemInfos(item);
            }
        }

        private async void UpdateItemInfos(FolderAssociation association)
        {
            StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(association.LocalFolder.Path);
            await ScanLocalFolder(localFolder, association.Id);
            await ScanRemoteFolder(association.RemoteFolder, association.Id);
            var properties = await localFolder.Properties.RetrievePropertiesAsync(new List<string> { "System.DateModified" });
            association.LocalFolder.LastModified = ((DateTimeOffset)properties["System.DateModified"]).LocalDateTime;

            //association.RemoteItem.DavItem.LastModified = remoteFolder
        }

        private async Task ScanRemoteFolder(RemoteItem remoteFolder, long associationId)
        {
            List<RemoteItem> items = await ConnectionManager.GetFolder(remoteFolder.DavItem.Href);
            foreach (RemoteItem item in items)
            {
                RemoteItem ri = new RemoteItem(item.DavItem);
                ri.FolderId = associationId;
                RemoteItemTableModel.GetDefault().InsertItem(ri);
                if (!ri.DavItem.IsCollection) continue;
                    await ScanRemoteFolder(item, associationId);
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


                LocalItemTableModel.GetDefault().InsertItem(li);
                if (sItem is StorageFolder)
                    await ScanLocalFolder((StorageFolder)sItem, associationId);
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

        /*private async void  GetAll(FolderAssociation item)
        {
            StorageFolder folder;
            try
            {
                folder = await StorageFolder.GetFolderFromPathAsync(item.LocalItem.Path);
            }
            catch (UnauthorizedAccessException)
            {
                return;
            }
            var files = await folder.GetFilesAsync(CommonFileQuery.DefaultQuery);
            var list = await ConnectionManager.GetFolder(item.RemoteItem.DavItem.Href);

        }*/

        private void GetDeleted()
        {
            
        }
    }
}
