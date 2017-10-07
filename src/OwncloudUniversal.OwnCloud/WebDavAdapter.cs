using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Windows.Networking.BackgroundTransfer;
using Windows.Security.Credentials;
using Windows.Storage;
using Windows.UI.Core;
using Windows.Web.Http;
using OwncloudUniversal.Model;
using OwncloudUniversal.OwnCloud.Model;
using OwncloudUniversal.Synchronization;
using OwncloudUniversal.Synchronization.Configuration;
using OwncloudUniversal.Synchronization.Model;
using OwncloudUniversal.Synchronization.Processing;
using ExecutionContext = OwncloudUniversal.Synchronization.Processing.ExecutionContext;

namespace OwncloudUniversal.OwnCloud
{
    public class WebDavAdapter : AbstractAdapter, IBackgroundSyncAdapter
    {
        private readonly WebDavClient _davClient;
        private readonly List<string> _existingFolders;

        public WebDavAdapter(bool isBackgroundSync, string serverUrl, NetworkCredential credential,
            AbstractAdapter linkedAdapter) : base(isBackgroundSync, linkedAdapter)
        {
            _davClient = new WebDavClient(new Uri(serverUrl, UriKind.RelativeOrAbsolute), credential);
            _existingFolders = new List<string>();
        }

        public override async Task<BaseItem> AddItem(BaseItem localItem)
        {
            BaseItem resultItem = null;
            if (localItem.IsCollection)
            {
                //build path and folder name
                string path = _BuildRemoteFolderPath(localItem.Association, localItem.EntityId);
                var folderName = localItem.EntityId.Substring(localItem.EntityId.LastIndexOf('\\')+1);

                //create folder and parent folders
                await CreateFolder(localItem.Association, localItem, folderName);
                //load the folder again to update the item properties
                var folder = await _davClient.ListFolder(new Uri(path, UriKind.RelativeOrAbsolute));
                resultItem = folder.FirstOrDefault(x => x.DisplayName == folderName);
                resultItem.Association = localItem.Association;
            }
            else
            {
                await CreateFolder(localItem.Association, localItem, Path.GetDirectoryName(localItem.EntityId));

                var folderPath = _BuildRemoteFolderPath(localItem.Association, localItem.EntityId);
                var filePath = _BuildRemoteFilePath(localItem.Association, localItem.EntityId);

                //if the file already exists dont upload it again
                var folder = await _davClient.ListFolder(new Uri(folderPath, UriKind.RelativeOrAbsolute));
                var existingItem = folder.FirstOrDefault(x => x.DisplayName == Path.GetFileName(localItem.EntityId));
                if (existingItem != null &&
                    existingItem.Size == localItem.Size)
                {
                    existingItem.Association = localItem.Association;
                    return existingItem;
                }
                var file = await StorageFile.GetFileFromPathAsync(localItem.EntityId);
                var progress = new Progress<HttpProgress>(OnHttpProgressChanged);
                resultItem = await _davClient.Upload(new Uri(filePath, UriKind.RelativeOrAbsolute), file, CancellationToken.None, progress);
                resultItem.Association = localItem.Association;
            }
            return resultItem;
        }

        private async void OnHttpProgressChanged(HttpProgress httpProgress)
        {
            if (Windows.ApplicationModel.Core.CoreApplication.Views.Count > 0)
            {
                await Windows.ApplicationModel.Core.CoreApplication.MainView.CoreWindow.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    ExecutionContext.Instance.TransferOperation = new TransferOperationInfo(httpProgress, null);
                    if(httpProgress.Stage == HttpProgressStage.ReceivingContent)
                        ExecutionContext.Instance.Status = ExecutionStatus.Receiving;
                    if (httpProgress.Stage == HttpProgressStage.SendingContent)
                        ExecutionContext.Instance.Status = ExecutionStatus.Sending;
                });
            }
        }

        public override async Task<BaseItem> UpdateItem(BaseItem item)
        {
            if (item.IsCollection)
            {
                var path = _BuildRemoteFilePath(item.Association, item.EntityId);
                return await GetItemInfos(path);
            }
            BaseItem targetItem = null;
            var progress = new Progress<HttpProgress>(OnHttpProgressChanged);
            var folderPath = _BuildRemoteFilePath(item.Association, item.EntityId);
            await CreateFolder(item.Association, item, Path.GetDirectoryName(item.EntityId));
            var file = await StorageFile.GetFileFromPathAsync(item.EntityId);
            targetItem = await _davClient.Upload(new Uri(folderPath, UriKind.RelativeOrAbsolute), file, CancellationToken.None, progress);
            targetItem.Association = item.Association;
            return targetItem;
        }

        private async Task<BaseItem> GetItemInfos(string entityId)
        {
            var items = await _davClient.ListFolder(new Uri(entityId, UriKind.RelativeOrAbsolute));
            var item = items.FirstOrDefault();
            return item;

        }

        public override async Task DeleteItem(BaseItem item)
        {
            var link = LinkStatusTableModel.GetDefault().GetItem(item);
            if (link == null)
            {
                await LogHelper.Write($"LinkStatus could not be found: EntityId: {item.EntityId} Id: {item.Id}");
                return;
            }
            var davId = item.Id == link.SourceItemId ? link.TargetItemId : link.SourceItemId;

            var davItem = ItemTableModel.GetDefault().GetItem(davId);
            if(davItem == null)
                return;
            if(await _davClient.Exists(new Uri(davItem.EntityId, UriKind.RelativeOrAbsolute)))
                await _davClient.Delete(new Uri(davItem.EntityId, UriKind.RelativeOrAbsolute));
        }

        public override async Task<List<BaseItem>> GetUpdatedItems(FolderAssociation association)
        {
            List<BaseItem> items = new List<BaseItem>();
            var folder = ItemTableModel.GetDefault().GetItem(association.RemoteFolderId);
            await _CheckRemoteFolderRecursive(folder, items);
            return items;
        }

        public async Task<List<BaseItem>> GetAllItems(Uri url)
        {
            List<DavItem> items = new List<DavItem>();
            items = await _davClient.ListFolder(url);
            return items.ToList<BaseItem>();
        }

        private async Task CreateFolder(FolderAssociation association, BaseItem localItem, string name)
        {
            //adds the folder and if necessesary the parent folder
            var uri = new Uri(Configuration.ServerUrl);
            var remoteBaseFolder = uri.LocalPath;
            var path = _BuildRemoteFolderPath(association, localItem.EntityId);
            path = WebUtility.UrlDecode(path.Replace(remoteBaseFolder, "").TrimEnd('/'));
            var folders = path.Split('/');
            if (localItem.IsCollection)
            {
                folders = (path + "/" + name).Split('/');
            }
            var currentFolder = remoteBaseFolder.TrimEnd('/');
            foreach (string f in folders)
            {
                string folderName = Uri.EscapeDataString(f);
                if (_existingFolders.Contains(currentFolder + '/' + folderName))//this should speed up the inital sync
                {
                    currentFolder += '/' + folderName;
                    continue;
                }
                //var folderContent = await _davClient.ListFolder(new Uri(currentFolder, UriKind.RelativeOrAbsolute));
                if (!await _davClient.Exists(new Uri(currentFolder + '/' + folderName, UriKind.RelativeOrAbsolute)))
                {
                    await _davClient.CreateFolder(new Uri(currentFolder + '/' + folderName, UriKind.RelativeOrAbsolute));
                }
                _existingFolders.Add(currentFolder + '/' + folderName);
                currentFolder += '/' + folderName;
            }
        }

        private async Task _CheckRemoteFolderRecursive(BaseItem folder, List<BaseItem> result)
        {
            List<DavItem> items = await _davClient.ListFolder(new Uri(folder.EntityId, UriKind.RelativeOrAbsolute));
            foreach (DavItem item in items)
            {
                if (!ChangekeyHasChanged(item)) continue;
                if (item.IsCollection && item.Href != folder.EntityId)
                {
                    await _CheckRemoteFolderRecursive(item, result);
                }
                if(!item.IsCollection && item.Size > Convert.ToUInt64(Configuration.MaxDownloadSize *1024 *1024))
                    continue;
                if(item.EntityId == folder.EntityId)
                    continue;
                result.Add(item);
            }
        }

        private bool ChangekeyHasChanged(BaseItem item)
        {
            var i = ItemTableModel.GetDefault().GetItem(item);
            return i == null || i.ChangeKey != item.ChangeKey;
        }

        private string _BuildRemoteFilePath(FolderAssociation association, string path)
        {
            var localFolder = GetAssociatedItem(association.LocalFolderId);
            Uri baseUri = new Uri(localFolder.EntityId);
            Uri fileUri = new Uri(path);
            Uri relativeUri = baseUri.MakeRelativeUri(fileUri);
            string uri = relativeUri.ToString();
            var relativeString = uri.Substring(uri.IndexOf('/') + 1);
            var remoteFolder = GetAssociatedItem(association.RemoteFolderId);
            return remoteFolder.EntityId + (relativeString);
        }

        private string _BuildRemoteFolderPath(FolderAssociation association, string path)
        {
            var localFolder = GetAssociatedItem(association.LocalFolderId);
            Uri baseUri = new Uri(localFolder.EntityId);
            Uri fileUri = new Uri(path);
            var relative = fileUri.ToString().Replace(baseUri.ToString(), "");
            if(relative != "" && relative != "/")
                relative = relative.Remove(relative.LastIndexOf('/')).TrimStart('/');
            var remoteFolder = GetAssociatedItem(association.RemoteFolderId);
            var result = remoteFolder.EntityId + (relative);
            return result;
        }

        private BaseItem GetAssociatedItem(long id)
        {
            return ItemTableModel.GetDefault().GetItem(id);
        }

        public override async Task<List<BaseItem>> GetDeletedItemsAsync(FolderAssociation association)
        {
            List<BaseItem> items = new List<BaseItem>();
            var folder = ItemTableModel.GetDefault().GetItem(association.RemoteFolderId);
            var existingItems = ItemTableModel.GetDefault().GetFilesForFolder(association, this.GetType()).ToList();
            await _GetDeletedItemsAsync(folder, existingItems, items);
            return items;
        }

        private async Task _GetDeletedItemsAsync(BaseItem folder, List<BaseItem> itemIndex, List<BaseItem> result)
        {
            List<DavItem> folderItems = await _davClient.ListFolder(new Uri(folder.EntityId, UriKind.RelativeOrAbsolute));
            //get all items that should be in a specific folder (according to the database)
            var existingItems = itemIndex.Where(x => x.EntityId.Contains(folder.EntityId));
            foreach (var item in existingItems)
            {
                if (item.EntityId == folder.EntityId) continue;
                if (GetParentFolderHref(item.EntityId) != folder.EntityId.TrimEnd('/')) continue;
                //check if the items saved in the db are still in the remote folder
                var folderItem = folderItems.FirstOrDefault(x => x.EntityId == item.EntityId);
                if (item.IsCollection && folderItem != null)
                {
                    //if a subfolder has changed, search there too
                    if (ChangekeyHasChanged(folderItem))
                        await _GetDeletedItemsAsync(folderItem, itemIndex, result);
                }
                else if (folderItem == null)
                {
                    //if it is not, the file has been deleted on remote side
                    result.Add(item);
                }
            }
        }

        private string GetParentFolderHref(string href)
        {
            href = href.TrimEnd('/');
            href = href.Substring(0, href.LastIndexOf('/'));
            return href;
        }

        public override string BuildEntityId(BaseItem item)
        {
            var localFolder = GetAssociatedItem(item.Association.LocalFolderId);
            string relPath = item.EntityId.Replace(localFolder.EntityId, "");
            string entitiyId = "/";
            foreach (var folder in relPath.Split('\\'))
            {
                if (string.IsNullOrWhiteSpace(folder))
                    continue;

                entitiyId += Uri.EscapeDataString(folder);
                entitiyId = entitiyId.Replace("%28", "(");
                entitiyId = entitiyId.Replace("%29", ")");
                entitiyId += "/";
            }
            var remoteFolder = GetAssociatedItem(item.Association.RemoteFolderId);
            var result = remoteFolder.EntityId.TrimEnd('/') + entitiyId.TrimEnd('/') + (item.IsCollection ? "/" : string.Empty);
            return result;
        }

        public AbstractAdapter GetAdapter()
        {
            return this;
        }

        public async Task CreateDownload(BaseItem davItem, IStorageItem targetItem)
        {
            var server = new Uri(Configuration.ServerUrl, UriKind.RelativeOrAbsolute);
            var itemUri = new Uri(davItem.EntityId, UriKind.RelativeOrAbsolute);
            var uri = new Uri(server, itemUri);
            var progress = new Progress<HttpProgress>(OnHttpProgressChanged);
            await _davClient.Download(uri, (StorageFile) targetItem, CancellationToken.None, progress);
        }
    }
}
