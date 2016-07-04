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
                var upload = await GetDataToUpload(item);
                await UploadItems(item, upload);

                var download = await GetDataToDownload(item);
                await DownloadItems(item, download);
            }
        }

        private async Task<List<LocalItem>> GetDataToUpload(FolderAssociation association)
        {
            List<LocalItem> result = new List<LocalItem>();
            StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(association.LocalFolder.Path);
            await CheckLocalFolderRecursive(localFolder, association.Id, result);
            return result;
        }
        private async Task CheckLocalFolderRecursive(StorageFolder folder, long associationId, List<LocalItem> result)
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
                var itemsInDatabase = LocalItemTableModel.GetDefault().SelectByPath(li.Path, li.FolderId);
                if (itemsInDatabase.Count == 0)
                    result.Add(li);
                else foreach (var item in itemsInDatabase)
                    {
                        if (item.LastModified > li.LastModified)
                            result.Add(li);
                    }
            }
        }

        private async Task<List<RemoteItem>> GetDataToDownload(FolderAssociation association)
        {
            var result = new List<RemoteItem>();
            await CheckRemoteFolderRecursive(association, result);
            return result;
        }
        private async Task CheckRemoteFolderRecursive(FolderAssociation association, List<RemoteItem> result)
        {
            List<RemoteItem> items = await ConnectionManager.GetFolder(association.RemoteFolder.DavItem.Href);
            foreach (RemoteItem item in items)
            {
                if (item.DavItem.IsCollection)
                    await CheckRemoteFolderRecursive(association, result);

                var foundItems = RemoteItemTableModel.GetDefault().SelectByPath(item.DavItem.Href, item.FolderId);
                if (foundItems.Count == 0)
                    result.Add(item);
                else foreach (var foundItem in foundItems)
                    {
                        if (foundItem.DavItem.Etag != item.DavItem.Etag)
                            result.Add(item);
                    }
            }
        }
        private async Task UpdateItemInfos(FolderAssociation association)
        {
            StorageFolder localFolder = await StorageFolder.GetFolderFromPathAsync(association.LocalFolder.Path);
            await ScanLocalFolder(localFolder, association.Id);
            await ScanRemoteFolder(association.RemoteFolder, association.Id);

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
                if (foundItems.Count == 0)
                {
                    RemoteItemTableModel.GetDefault().InsertItem(ri);
                    if (!ri.DavItem.IsCollection) continue;
                    await ScanRemoteFolder(item, associationId);
                    continue;
                }

                foreach (var foundItem in foundItems)
                {
                    if (foundItem.DavItem.Etag != ri.DavItem.Etag)
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
                        Debug.Write(string.Format("Updating Database {0}", itemInDatabase.Id));
                        if (sItem is StorageFolder)
                            await ScanLocalFolder((StorageFolder)sItem, associationId);
                    }
                }
            }
        }
        private async Task UploadItems(FolderAssociation association,  List<LocalItem> items)
        {
            foreach (var item in items)
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(item.Path);
                var stream = await file.OpenStreamForReadAsync();
                await ConnectionManager.Upload(BuildRemotePath(association, file.Path), stream, file.DisplayName);
            }
        }

        private async Task DownloadItems(FolderAssociation assciation, List<RemoteItem> items)
        {
            foreach (var item in items)
            {
                string filePath = BuildLocalPath(assciation, item.DavItem.Href);
                string folderPath = Path.GetDirectoryName(filePath);
                var folder = await StorageFolder.GetFolderFromPathAsync(folderPath);
                var file = await folder.CreateFileAsync(filePath);
                var result = await ConnectionManager.Download(item.DavItem.Href, file);
            }
        }

        private string BuildLocalPath(FolderAssociation association, string href)
        {
            Uri serverUri = new Uri(Configuration.ServerUrl);
            Uri folderUri = new Uri(serverUri, association.RemoteFolder.DavItem.Href);
            Uri fileUri = new Uri(serverUri, href);
            Uri relativeUri = folderUri.MakeRelativeUri(fileUri);
            string path = Uri.UnescapeDataString(relativeUri.ToString().Replace('/', '\\'));
            return association.LocalFolder.Path + '\\' +  path;
        }
        private string BuildRemotePath(FolderAssociation association, string path)
        {
            Uri baseUri = new Uri(association.LocalFolder.Path);
            Uri fileUri = new Uri(path);
            Uri relativeUri = baseUri.MakeRelativeUri(fileUri);
            string uri = relativeUri.ToString();
            var relativeString = uri.Substring(uri.IndexOf('/') + 1);
            return association.RemoteFolder.DavItem.Href + relativeString;
        }
    }
}