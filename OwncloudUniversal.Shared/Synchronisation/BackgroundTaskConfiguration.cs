using System;
using System.Linq;
using Windows.ApplicationModel.Background;

namespace OwncloudUniversal.Shared.Synchronisation
{
    public class BackgroundTaskConfiguration
    {
        private const string TaskName = "owncloud-backgroundSync";
        private const string EntryPoint = "OwncloudUniversal.BackgroundSync.WebDavBackgroundSync";

        public bool Enabled
        {
            get { return BackgroundTaskRegistration.AllTasks.Any(task => task.Value.Name == TaskName); }
            set
            {
                if(value)
                    Register();
                else
                {
                    Deregister();
                }
            }
        }

        private async void Register()
        { 
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == TaskName)
                {
                    task.Value.Unregister(true);
                }
            }
            var builder = new BackgroundTaskBuilder();
            builder.Name = TaskName;
            builder.TaskEntryPoint = EntryPoint;
            BackgroundExecutionManager.RemoveAccess();
            var promise = await BackgroundExecutionManager.RequestAccessAsync();

            var maintenanceTrigger = new MaintenanceTrigger(15, false);
            builder.SetTrigger(maintenanceTrigger);
            BackgroundTaskRegistration registration = builder.Register();
            
        }

        private void Deregister()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == TaskName)
                {
                    task.Value.Unregister(true);
                }
            }
        }
    }

}
