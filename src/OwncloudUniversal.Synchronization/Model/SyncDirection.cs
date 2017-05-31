using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwncloudUniversal.Synchronization.Model
{
    public enum SyncDirection
    {
        DownloadOnly = 0,
        UploadOnly = 1,
        FullSync = 2
    }
}
