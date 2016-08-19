using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;

namespace OwncloudUniversal.BackgroundOperations
{
    class BackgroundSyncConfiguguration
    {
        public async void Register()
        {

            var taskName = "owncloud-backgroundSync";

            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == taskName)
                {
                    task.Value.Unregister(true);
                }
            }


            var builder = new BackgroundTaskBuilder();
            builder.Name = taskName;
            builder.TaskEntryPoint = "OwncloudUniversal.BackgroundSync.WebDavBackgroundSync";
            BackgroundExecutionManager.RemoveAccess();
            var promise = await BackgroundExecutionManager.RequestAccessAsync();

            builder.SetTrigger(new SystemTrigger(SystemTriggerType.UserAway, false));
            BackgroundTaskRegistration registration = builder.Register();
            
        }
    }
}
