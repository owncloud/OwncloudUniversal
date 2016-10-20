using System;
using System.Collections.Generic;
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
            _davClient = new WebDavClient(new Uri(serverUrl), credential);
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
                var folder = await _davClient.ListFolder(new Uri(path));
                resultItem = folder.FirstOrDefault(x => x.DisplayName == folderName);
                resultItem.Association = localItem.Association;
            }
            else
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(localItem.EntityId);
                var folderPath = _BuildRemoteFolderPath(localItem.Association, file.Path);
                try
                {
                    //if the file already exists dont upload it again
                    var folder = await _davClient.ListFolder(new Uri(folderPath));
                    var existingItem = folder.Where(x => x.DisplayName == file.Name).FirstOrDefault();
                    if (existingItem != null)
                    {
                        existingItem.Association = localItem.Association;
                        return existingItem;
                    }
                }
                catch
                {

                }
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    await CreateFolder(localItem.Association, localItem, Path.GetDirectoryName(file.Path));
                    await _davClient.Upload(new Uri(folderPath), stream);
                    resultItem = await _davClient.Upload(new Uri(folderPath), stream);
                }


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
                targetItem = await _davClient.Upload(new Uri(folderPath), stream);
            }
            targetItem.Association = item.Association;
            return targetItem;
        }

        public override async Task<AbstractItem> GetItem(string entityId)
        {
            var items = await _davClient.ListFolder(new Uri(entityId, UriKind.RelativeOrAbsolute));
            return items.Count == 1 ? items.FirstOrDefault() : null;
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
            var path = _BuildRemoteFolderPath(association, localItem.EntityId).Replace(remoteBaseFolder, "");
            var folders = path.Split('/');
            if (localItem.IsCollection)
            {
                folders = (path + "/" + name).Split('/');
            }
            var currentFolder = remoteBaseFolder.TrimEnd('/');
            for (int i = 0; i < folders.Length; i++)
            {
                try
                {
                    var folderContent = await _davClient.ListFolder( new Uri(currentFolder));
                    if (folderContent.Count(x => x.DisplayName == folders[i] && x.IsCollection) == 0)
                        if (!string.IsNullOrWhiteSpace(folders[i]))
                            await _davClient.CreateFolder(new Uri(currentFolder + folders[i]));//TODO fix this
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e.Message);
                    //wenn ordner schon existiert weiter machen
                }
                currentFolder += '/' + folders[i];
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
            return remoteFolder.EntityId + relativeString;
        }

        private string _BuildRemoteFolderPath(FolderAssociation association, string path)
        {
            var localFolder = GetAssociatedItem(association.LocalFolderId);
            Uri baseUri = new Uri(localFolder.EntityId);
            Uri fileUri = new Uri(path);
            var relative = fileUri.ToString().Replace(baseUri.ToString(), "");
            relative = relative.Remove(relative.LastIndexOf('/')).TrimStart('/');
            var remoteFolder = GetAssociatedItem(association.RemoteFolderId);
            var result = remoteFolder.EntityId + relative;
            return result;
        }

        

        private AbstractItem GetAssociatedItem(long id)
        {
            return AbstractItemTableModel.GetDefault().GetItem(id);
        }

    }
}
