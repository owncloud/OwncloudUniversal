using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;
using OwncloudUniversal.Shared.LocalFileSystem;
using OwncloudUniversal.Shared.Model;

namespace OwncloudUniversal.Shared.Model
{
    public class LocalItem : AbstractItem
    {
        public LocalItem() { }

        public LocalItem(FolderAssociation association,IStorageItem storageItem, BasicProperties basicProperties)
        {
            Association = association;
            IsCollection = storageItem is StorageFolder;
            ChangeKey = SQLite.DateTimeHelper.DateTimeSQLite(basicProperties.DateModified.UtcDateTime);
            EntityId = storageItem.Path;
            ChangeNumber = 0;
            Size = basicProperties.Size;
        }

        public LocalItem(FolderAssociation association, IStorageItem storageItem, IDictionary<string, object> properties )
        {
            Association = association;
            IsCollection = storageItem is StorageFolder;
            var s = properties["System.DateModified"];
            ChangeKey = SQLite.DateTimeHelper.DateTimeSQLite(((DateTimeOffset)properties["System.DateModified"]).UtcDateTime);
            EntityId = storageItem.Path;
            ChangeNumber = 0;
            Size = (ulong)properties["System.Size"];
        }

        public DateTime? LastModified { get; set; }        
        public string Path { get; set; }
        public override ulong Size { get; set; }

        public override string EntityId
        {
            get
            {
                return Path;
            }

            set
            {
                Path = value;
            }
        }

        public override string ChangeKey
        {
            get
            {
                return SQLite.DateTimeHelper.DateTimeSQLite(LastModified);
            }

            set
            {
                LastModified = Convert.ToDateTime(value);
            }
        }

        public override Type AdapterType => typeof(FileSystemAdapter);
    }
}
