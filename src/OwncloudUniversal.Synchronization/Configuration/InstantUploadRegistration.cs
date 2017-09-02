using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Storage;

namespace OwncloudUniversal.Synchronization.Configuration
{
    public class InstantUploadRegistration
    {
        private const string TaskName = "owncloud-instantupload";
        private const string EntryPoint = "OwncloudUniversal.BackgroundTasks.InstantUploadTask";

        public bool Enabled
        {
            get { return BackgroundTaskRegistration.AllTasks.Any(task => task.Value.Name == TaskName); }
            set
            {
                if (value)
                {

                    var task = new Task(async () => await Register().ConfigureAwait(false));
                    task.RunSynchronously();
                }
                else
                {
                    Deregister();
                }
                Configuration.IsCameraUploadEnabled = value;
            }
        }

        private async Task Register()
        {
            BackgroundExecutionManager.RemoveAccess();
            var promise = await BackgroundExecutionManager.RequestAccessAsync();
            if (promise == BackgroundAccessStatus.DeniedByUser || promise == BackgroundAccessStatus.DeniedBySystemPolicy)
            {
                return;
            }

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

            var library = await StorageLibrary.GetLibraryAsync(KnownLibraryId.Pictures);
            var contentChangedTrigger = StorageLibraryContentChangedTrigger.Create(library);
            builder.SetTrigger(contentChangedTrigger);
            builder.IsNetworkRequested = true;
            builder.CancelOnConditionLoss = true;
            try
            {

                BackgroundTaskRegistration registration = builder.Register();
                Debug.WriteLine(registration.Name);
                Debug.WriteLine(registration.TaskId);
                Debug.WriteLine(registration.Trigger);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.Message);
                throw;
            }

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
