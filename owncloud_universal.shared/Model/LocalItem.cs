using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.Storage.FileProperties;

namespace OwncloudUniversal.Shared.Model
{
    public class LocalItem : AbstractItem
    {
        public LocalItem() { }

        public LocalItem(FolderAssociation association,IStorageItem storageItem, BasicProperties basicProperties)
        {
            Association = association;
            IsCollection = storageItem is StorageFolder;
            ChangeKey = SQLite.DateTimeHelper.DateTimeSQLite(basicProperties.DateModified.LocalDateTime);
            EntityId = storageItem.Path;
            ChangeNumber = 0;
            
        }
        public DateTime? LastModified { get; set; }        
        public string Path { get; set; }

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
    }
}
