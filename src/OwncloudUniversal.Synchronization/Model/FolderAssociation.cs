using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace OwncloudUniversal.Synchronization.Model
{
    public class FolderAssociation
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
        public SyncDirection SyncDirection { get; set; }
        public long LocalFolderId { get; set; }
        public long RemoteFolderId { get; set; }
        public DateTime LastSync { get; set; }
        public string LocalFolderPath => ItemTableModel.GetDefault().GetItem(LocalFolderId)?.EntityId;
        public string RemoteFolderFolderPath => ItemTableModel.GetDefault().GetItem(RemoteFolderId)?.EntityId;
        public bool SupportsInstantUpload { get; set; }
    }
}
