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
                //UploadItems(item);
            }
        }

        private async void UpdateItemInfos(FolderAssociation association)
        {
            var localFolder = await StorageFolder.GetFolderFromPathAsync(association.LocalPath);
            





            /*
             * 
             * 
             * var localFolder = await StorageFolder.GetFolderFromPathAsync(association.LocalItem.Path);
            var remoteFolder = await ConnectionManager.GetFolderInfo(association.RemoteItem.DavItem.Href);
            if (remoteFolder.Count != 1)
                throw new Exception();
            var properties = await localFolder.Properties.RetrievePropertiesAsync(new List<string> { "System.DateModified" });
            association.LocalItem.LastModified = ((DateTimeOffset)properties["System.DateModified"]).LocalDateTime;
            association.RemoteItem = remoteFolder[0];

            RemoteItemTableModel.GetDefault().UpdateItem(association.RemoteItem, association.RemoteItem.Id);
            LocalItemTableModel.GetDefault().UpdateItem(association.LocalItem, association.LocalItem.Id);
            FolderAssociationTableModel.GetDefault().UpdateItem(association, association.Id);   
            
             */
        }

        private async void UploadItems(FolderAssociation item)
        {
            var folder = await StorageFolder.GetFolderFromPathAsync(item.LocalItem.Path);               
            var files = await folder.GetFilesAsync();
            var list = await ConnectionManager.GetFolder(item.RemoteItem.DavItem.Href);

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
                await ConnectionManager.Upload(item.RemoteItem.DavItem.Href, stream, file.Name);
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
