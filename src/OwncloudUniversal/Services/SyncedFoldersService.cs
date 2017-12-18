using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.AccessCache;
using OwncloudUniversal.Model;
using OwncloudUniversal.Synchronization.Model;
using OwncloudUniversal.OwnCloud;
using OwncloudUniversal.OwnCloud.Model;

namespace OwncloudUniversal.Services
{
    public class SyncedFoldersService
    {

        public async Task<FolderAssociation> AddFolderToSyncAsync (StorageFolder folder, DavItem remoteFolderItem, bool allowInstantUpload = false)
        {
            StorageApplicationPermissions.FutureAccessList.Add(folder);
            var properties = await folder.Properties.RetrievePropertiesAsync(new List<string> { "System.DateModified" });

            FolderAssociation fa = new FolderAssociation
            {
                IsActive = true,
                LocalFolderId = 0,
                RemoteFolderId = 0,
                SyncDirection = SyncDirection.FullSync,
                LastSync = DateTime.MinValue
            };
            if (allowInstantUpload)
            {
                fa.SyncDirection = SyncDirection.UploadOnly;
                fa.SupportsInstantUpload = true;
            }
            FolderAssociationTableModel.GetDefault().InsertItem(fa);
            fa = FolderAssociationTableModel.GetDefault().GetLastInsertItem();

            BaseItem li = new LocalItem
            {
                IsCollection = true,
                LastModified = ((DateTimeOffset)properties["System.DateModified"]).LocalDateTime,
                EntityId = folder.Path,
                Association = fa,
            };
            var testFolder = await StorageFolder.GetFolderFromPathAsync(folder.Path);
            if (testFolder.Path != folder.Path)
            {
                li.EntityId = testFolder.Path;
            }
            ItemTableModel.GetDefault().InsertItem(li);
            li = ItemTableModel.GetDefault().GetLastInsertItem();

            remoteFolderItem.Association = fa;
            ItemTableModel.GetDefault().InsertItem(remoteFolderItem);
            var ri = ItemTableModel.GetDefault().GetLastInsertItem();

            fa.RemoteFolderId = ri.Id;
            fa.LocalFolderId = li.Id;
            FolderAssociationTableModel.GetDefault().UpdateItem(fa, fa.Id);
            var link = new LinkStatus(li,ri);
            LinkStatusTableModel.GetDefault().InsertItem(link);
            return fa;
        }

        public List<FolderAssociation> GetConfiguredFolders()
        {
            return FolderAssociationTableModel.GetDefault().GetAllItems().ToList();
        }

        public List<FolderAssociation> GetAllSyncedFolders()
        {
            return FolderAssociationTableModel.GetDefault().GetAllItems().ToList();
        }

        public List<FolderAssociation> GetInstantUploadAssociations()
        {
            return FolderAssociationTableModel.GetDefault().GetAllItems().ToList().Where(x => x.SupportsInstantUpload).ToList();
        }

        public void RemoveInstantUploadAssociations()
        {
            foreach (var association in GetInstantUploadAssociations())
            {
                FolderAssociationTableModel.GetDefault().DeleteItem(association.Id);
                LinkStatusTableModel.GetDefault().DeleteLinksFromAssociation(association);
                ItemTableModel.GetDefault().DeleteItem(association.RemoteFolderId);
                ItemTableModel.GetDefault().DeleteItem(association.LocalFolderId);
                ItemTableModel.GetDefault().DeleteItemsFromAssociation(association);
            }
        }

        public void RemoveFromSyncedFolders(FolderAssociation association)
        {
            ItemTableModel.GetDefault().DeleteItemsFromAssociation(association);
            LinkStatusTableModel.GetDefault().DeleteLinksFromAssociation(association);
            FolderAssociationTableModel.GetDefault().DeleteItem(association.Id);
        }
    }
}
