using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwncloudUniversal.Synchronization.Model
{
    public class SyncHistoryEntry
    {
        public long Id { get; set; }

        public string Message { get; set; }

        public SyncResult Result { get; set; }

        public string EntityId { get; set; }

        public DateTime CreateDate { get; set; }

        public string ContentType { get; set; }
    }
}
