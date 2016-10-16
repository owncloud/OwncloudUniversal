using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OwncloudUniversal.Shared.Synchronisation
{
    public static class ExecutionContext
    { 
        public static ExecutionStatus Status { get; set; }
        public static string CurrentFileName { get; set; }
        public static int CurrentFileNumber { get; set; }
        public static int TotalFileCount { get; set; }
        public static ConnectionProfile ConnectionProfile { get; set; }

        public static void Init()
        {
            Status = ExecutionStatus.Stopped;
            CurrentFileName = string.Empty;
            CurrentFileNumber = 0;
            TotalFileCount = 0;
            ConnectionProfile = ConnectionProfile.WiFi;
        }

        public static string FileText => $"{CurrentFileNumber} / {TotalFileCount} Files";
    }
}
