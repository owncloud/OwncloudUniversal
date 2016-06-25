using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace owncloud_universal.Model
{
    class FolderAssociation
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
        public SyncDirection SyncDirection { get; set; }
        public LocalItem LocalFolder { get; set; }
        public RemoteItem RemoteFolder { get; set; }
    }
}
