using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace OwncloudUniversal.Shared.Model
{
    public class FolderAssociation
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
        public SyncDirection SyncDirection { get; set; }
        public long LocalFolderId { get; set; }
        public long RemoteFolderId { get; set; }

        public string LocalFolderPath => AbstractItemTableModel.GetDefault().GetItem(LocalFolderId).EntityId;
        public string RemoteFolderFolderPath => AbstractItemTableModel.GetDefault().GetItem(RemoteFolderId).EntityId;
    }
}
