using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Windows.Storage;
using OwncloudUniversal.Shared;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.Shared.Synchronisation;

namespace OwncloudUniversal.WebDav
{
    public class WebDavAdapter : AbstractAdapter
    {
        private readonly WebDavClient _davClient;
        public WebDavAdapter(bool isBackgroundSync, string serverUrl, NetworkCredential credential, AbstractAdapter linkedAdapter) : base(isBackgroundSync, linkedAdapter)
        {
            _davClient = new WebDavClient(new Uri(serverUrl, UriKind.RelativeOrAbsolute), credential);
        }
        public override async Task<AbstractItem> AddItem(AbstractItem localItem)
        {
            AbstractItem resultItem = null;
            if (localItem.IsCollection)
            {
                //build path and folder name
                string path = _BuildRemoteFolderPath(localItem.Association, localItem.EntityId);
                var folderName = (await StorageFolder.GetFolderFromPathAsync(localItem.EntityId)).DisplayName;

                //create folder and parent folders
                await CreateFolder(localItem.Association, localItem, folderName);
                //load the folder again to update the item properties
                var folder = await _davClient.ListFolder(new Uri(path, UriKind.RelativeOrAbsolute));
                resultItem = folder.FirstOrDefault(x => x.DisplayName == folderName);
                resultItem.Association = localItem.Association;
            }
            else
            {
                localItem = await LinkedAdapter.GetItem(localItem.EntityId);
                var folderPath = _BuildRemoteFolderPath(localItem.Association, localItem.EntityId);
                var filePath = _BuildRemoteFilePath(localItem.Association, localItem.EntityId);
                //if the file already exists dont upload it again
                var folder = await _davClient.ListFolder(new Uri(folderPath, UriKind.RelativeOrAbsolute));
                var existingItem = folder.FirstOrDefault(x => x.DisplayName == Path.GetFileName(localItem.EntityId));
                if (existingItem != null)
                {
                    existingItem.Association = localItem.Association;
                    return existingItem;
                }
                await CreateFolder(localItem.Association, localItem, Path.GetDirectoryName(localItem.EntityId));
                resultItem = await _davClient.Upload(new Uri(filePath, UriKind.RelativeOrAbsolute), localItem.ContentStream);
                resultItem.Association = localItem.Association;
            }
            return resultItem;
        }

        public override async Task<AbstractItem> UpdateItem(AbstractItem item)
        {
            if (item.IsCollection)
                return null;//TODO
            AbstractItem targetItem = null;
            item = await LinkedAdapter.GetItem(item.EntityId);
            var folderPath = _BuildRemoteFilePath(item.Association, item.EntityId);
            using (var stream = item.ContentStream)
            {
                await CreateFolder(item.Association, item, Path.GetDirectoryName(item.EntityId));
                targetItem = await _davClient.Upload(new Uri(folderPath, UriKind.RelativeOrAbsolute), stream);
            }
            targetItem.Association = item.Association;
            return targetItem;
        }

        public override async Task<AbstractItem> GetItem(string entityId)
        {
            var items = await _davClient.ListFolder(new Uri(entityId, UriKind.RelativeOrAbsolute));
            var item = items.Count == 1 ? items.FirstOrDefault() : null;
            if(item != null)
                item.ContentStream = await _davClient.Download(new Uri(entityId, UriKind.RelativeOrAbsolute));
            return item;

        }

        public override Task DeleteItem(AbstractItem item)
        {
            throw new NotImplementedException();
        }

        public override async Task<List<AbstractItem>> GetUpdatedItems(FolderAssociation association)
        {
            List<AbstractItem> items = new List<AbstractItem>();
            var folder = AbstractItemTableModel.GetDefault().GetItem(association.RemoteFolderId);
            await _CheckRemoteFolderRecursive(folder, items);
            return items;
        }

        public async Task<List<AbstractItem>> GetAllItems(Uri url)
        {
            List<DavItem> items = new List<DavItem>();
            items = await _davClient.ListFolder(url);
            return items.ToList<AbstractItem>();
        }

        private async Task CreateFolder(FolderAssociation association, AbstractItem localItem, string name)
        {
            //adds the folder and if necessesary the parent folder
            var remoteBaseFolder = GetAssociatedItem(association.RemoteFolderId).EntityId;
            var path = _BuildRemoteFolderPath(association, localItem.EntityId);
            path = path.Replace(remoteBaseFolder, "").TrimEnd('/');
            var folders = path.Split('/');
            if (localItem.IsCollection)
            {
                folders = (path + "/" + name).Split('/');
            }
            var currentFolder = remoteBaseFolder.TrimEnd('/');
            foreach (string folderName in folders)
            {
                var folderContent = await _davClient.ListFolder(new Uri(currentFolder, UriKind.RelativeOrAbsolute));
                if (folderContent.Count(x => x.DisplayName == folderName && x.IsCollection) == 0 && !string.IsNullOrWhiteSpace(folderName))
                        await _davClient.CreateFolder(new Uri(currentFolder + '/' + folderName, UriKind.RelativeOrAbsolute));
                currentFolder += '/' + folderName;
            }
        }

        private async Task _CheckRemoteFolderRecursive(AbstractItem folder, List<AbstractItem> result)
        {
            List<DavItem> items = await _davClient.ListFolder(new Uri(folder.EntityId, UriKind.RelativeOrAbsolute));
            foreach (DavItem item in items)
            {
                if (!ChangekeyHasChanged(item)) continue;
                if (item.IsCollection && item.Href != folder.EntityId)
                {
                    await _CheckRemoteFolderRecursive(item, result);
                }
                if(item.Size > Convert.ToUInt64(Configuration.MaxDownloadSize *1024 *1024))
                    continue;
                result.Add(item);
            }
        }

        private bool ChangekeyHasChanged(AbstractItem item)
        {
            var i = AbstractItemTableModel.GetDefault().GetItem(item);
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

        

        private AbstractItem GetAssociatedItem(long id)
        {
            return AbstractItemTableModel.GetDefault().GetItem(id);
        }

    }
}
