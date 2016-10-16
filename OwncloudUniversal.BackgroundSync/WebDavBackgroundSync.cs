using OwncloudUniversal.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.UI.Notifications;
using OwncloudUniversal.Shared.LocalFileSystem;
using OwncloudUniversal.Shared.Synchronisation;
using OwncloudUniversal.Shared.WebDav;

namespace OwncloudUniversal.BackgroundSync
{
    public sealed class WebDavBackgroundSync : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            taskInstance.Canceled += (instance, reason) =>
            {
                var toastXml = ToastNotificationManager.GetTemplateContent(ToastTemplateType.ToastText01);
                var toastElements = toastXml.GetElementsByTagName("text");
                toastElements[0].InnerText = reason.ToString();
                ToastNotificationManager.CreateToastNotifier().Show(new ToastNotification(toastXml));
            };
            SyncWorker s = new SyncWorker(new FileSystemAdapter(true), new WebDavAdapter(true), true);
            try
            {
                Configuration.CurrentlyActive = true;
                await s.Run();
                Configuration.CurrentlyActive = false;
            }
            catch
            {
                Configuration.CurrentlyActive = false;
            }
            _deferral.Complete();
        }
    }
}
