using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using OwncloudUniversal.Synchronization.Configuration;
using OwncloudUniversal.Synchronization.SQLite;

namespace OwncloudUniversal.BackgroundTasks
{
    public sealed class AppUpdateTask : IBackgroundTask
    {
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            if (Configuration.IsBackgroundTaskEnabled)
            {
                var reg = new BackgroundTaskConfiguration();
                reg.Enabled = true;
            }
            if (Configuration.IsCameraUploadEnabled)
            {
                var reg = new InstantUploadRegistration();
                reg.Enabled = true;
            }
            SQLiteClient.Init();
        }
    }
}
