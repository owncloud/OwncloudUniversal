using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace owncloud_universal.Model
{
    public class FolderAssociation
    {
        public long Id { get; set; }
        public bool IsActive { get; set; }
        public SyncDirection SyncDirection { get; set; }
        public AbstractItem LocalFolder { get; set; }
        public AbstractItem RemoteFolder { get; set; }
    }
}
