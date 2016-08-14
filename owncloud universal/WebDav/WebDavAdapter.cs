using owncloud_universal.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace owncloud_universal.WebDav
{
    class WebDavAdapter : AbstractAdapter
    {
        public override async Task<AbstractItem> AddItem(AbstractItem remoteItem)
        {
            AbstractItem targetItem = null;
            if (remoteItem.IsCollection)
            {
                string path = _BuildRemoteFolderPath(remoteItem.Association, remoteItem.EntityId);
                string name = (await StorageFolder.GetFolderFromPathAsync(remoteItem.EntityId)).DisplayName;
                CreateFolderRecursive(remoteItem.Association, path, name);
                ConnectionManager.CreateFolder(path, name);
                var folder = await ConnectionManager.GetFolder(path);
                targetItem = folder.Where(x => x.DavItem.DisplayName == name).FirstOrDefault();
            }
            else
            {
                StorageFile file = await StorageFile.GetFileFromPathAsync(remoteItem.EntityId);
                var stream = await file.OpenStreamForReadAsync();
                var folderPath = _BuildRemoteFolderPath(remoteItem.Association, file.Path);
                await ConnectionManager.Upload(folderPath, stream, file.DisplayName);
                var folder = await ConnectionManager.GetFolder(folderPath);
                targetItem = folder.Where(x => x.DavItem.DisplayName == file.DisplayName).FirstOrDefault();

            }
            return targetItem;
        }

        private void CreateFolderRecursive(FolderAssociation association, string path, string folderName)
        {
            string[] pathParts = path.Split('/');
            string[] baseParts = GetAssociatedItem(association.RemoteFolderId).EntityId.Split('/');
            for (int i = baseParts.Length; i < pathParts.Length; i++)
            {
                var j = pathParts[i];
                var p = _BuildRemoteFolderPath(association, );
            }
            try
            {
                var localFolder = GetAssociatedItem(association.LocalFolderId);
                Uri baseUri = new Uri(localFolder.EntityId);
                var relative = path.Replace(baseUri.ToString(), "");
                relative = relative.Remove(relative.LastIndexOf('/')).TrimStart('/');
                //var relativeString = uri.Remove(uri.ToString());
                
                var folder = ConnectionManager.GetFolder(path);
            }
            catch (Exception)
            {
                var parent = _BuildRemoteFolderPath(association, path);
            }
        }

        public override async Task<AbstractItem> UpdateItem(AbstractItem item)
        {
            return await AddItem(item);
        }

        public override async Task DeleteItem(AbstractItem item)
        {
            var _item = (LocalItem)item;
            if (_item.IsCollection)
            {
                string path = _BuildRemoteFolderPath(_item.Association, _item.Path);
                string name = (await StorageFolder.GetFolderFromPathAsync(_item.Path)).DisplayName;
                ConnectionManager.DeleteFolder(_item.Path +'/'+ name);
                return;
            }
            else
            {
                ConnectionManager.DeleteFile(_BuildRemoteFilePath(_item.Association, _item.Path));
            }
        }

        public override async Task<List<AbstractItem>> GetAllItems(FolderAssociation association)
        {
            List<AbstractItem> items = new List<AbstractItem>();
            var remoteFolder = GetAssociatedItem(association.RemoteFolderId);
            await _CheckRemoteFolderRecursive(remoteFolder, items);
            return items;
        }
        private async Task _CheckRemoteFolderRecursive(AbstractItem folder, List<AbstractItem> result)
        {
            List<RemoteItem> items = await ConnectionManager.GetFolder(folder.EntityId);
            foreach (RemoteItem item in items)
            {
                if (item.IsCollection)
                    await _CheckRemoteFolderRecursive(item, result);
                result.Add(item);
            }
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
            //var relativeString = uri.Remove(uri.ToString());
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
