using System;
using Windows.ApplicationModel.Background;

namespace OwncloudUniversal.Shared.Synchronisation
{
    public class BackgroundTaskConfiguration
    {
        private const string TaskName = "owncloud-backgroundSync";
        private const string EntryPoint = "OwncloudUniversal.BackgroundSync.WebDavBackgroundSync";
        public async void Register(ApplicationTrigger trigger = null)
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

            if (trigger != null)
            {
                builder.SetTrigger(trigger);
            }
            else
            {
                builder.SetTrigger(new SystemTrigger(SystemTriggerType.UserAway, false));
            }
                
            BackgroundTaskRegistration registration = builder.Register();
            
        }

        public bool IsRegistered()
        {
            foreach (var task in BackgroundTaskRegistration.AllTasks)
            {
                if (task.Value.Name == TaskName)
                {
                    return true;
                }
            }
            return false;
        }
    }

}
