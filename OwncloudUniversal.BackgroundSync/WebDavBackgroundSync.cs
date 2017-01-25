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
using OwncloudUniversal.WebDav;

namespace OwncloudUniversal.BackgroundSync
{
    public sealed class WebDavBackgroundSync : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            _deferral = taskInstance.GetDeferral();
            
            var fileSystem = new FileSystemAdapter(true, null);
            var webDav = new WebDavAdapter(true, Configuration.ServerUrl, Configuration.Credential, fileSystem);
            fileSystem.LinkedAdapter = webDav;
            var worker = new BackgroundSyncProcess(fileSystem, webDav, true);
            try
            {
                await worker.Run();
            }
            catch (Exception e)
            {
                await LogHelper.Write($"BackgroundTask Exception: {e.Message}" + Environment.NewLine +
                                      $"{e.StackTrace}" + Environment.NewLine +
                                      $"Status:{worker.ExecutionContext.Status.ToString()}" + Environment.NewLine +
                                      $"File: {worker.ExecutionContext.CurrentFileNumber}" + Environment.NewLine +
                                      $"of {worker.ExecutionContext.TotalFileCount}");
            }
            finally
            {

                await LogHelper.Write("BackgroundTask finished");
                _deferral.Complete();
            }

            taskInstance.Canceled += (instance, reason) =>
            {
                Task.Run(() => LogHelper.Write($"BackgroundTask canceled. Reason: {reason}, Status:{worker.ExecutionContext.Status.ToString()} File: {worker.ExecutionContext.CurrentFileNumber} of {worker.ExecutionContext.TotalFileCount}"));
            };


        }
    }
}
