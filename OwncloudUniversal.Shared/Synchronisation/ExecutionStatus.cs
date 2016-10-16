using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwncloudUniversal.Shared.Synchronisation
{
    public enum ExecutionStatus
    {
        Active = 1,
        Finished = 2,
        Stopped = 3,
        Scanning = 4,
        Downloading = 5,
        Uploading = 6
    }
}
