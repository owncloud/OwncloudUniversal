using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace owncloud_universal.Model
{
    public class LocalItem : AbstractItem
    {
        public long FolderId { get; set; }
        public DateTime? LastModified { get; set; }        
        public string Path { get; set; }
        public long RemoteItemId { get; set; }
        public override string ChangeKey
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

    }
}
