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
        private BackgroundSyncProcess _worker;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {

            taskInstance.Canceled += OnCanceled;
            _deferral = taskInstance.GetDeferral();

            try
            {
                var fileSystem = new FileSystemAdapter(true, null);
                var webDav = new WebDavAdapter(true, Configuration.ServerUrl, Configuration.Credential, fileSystem);
                fileSystem.LinkedAdapter = webDav;
                _worker = new BackgroundSyncProcess(fileSystem, webDav, true);
                await _worker.Run();
            }
            catch (Exception e)
            {
                await LogHelper.Write($"BackgroundTask Exception: {e.Message}" + Environment.NewLine +
                                      $"{e.StackTrace}" + Environment.NewLine +
                                      $"Status:{_worker.ExecutionContext.Status.ToString()}" + Environment.NewLine +
                                      $"File: {_worker.ExecutionContext.CurrentFileNumber}" + Environment.NewLine +
                                      $"of {_worker.ExecutionContext.TotalFileCount}");
            }
            finally
            {

                await LogHelper.Write("BackgroundTask finished");
                _deferral.Complete();
            }
        }

        private void OnCanceled(IBackgroundTaskInstance instance, BackgroundTaskCancellationReason reason)
        {
            if (_worker != null)
            {
                _worker.ExecutionContext.Status = ExecutionStatus.Stopped;
                Task.Run(() =>LogHelper.Write($"BackgroundTask canceled. Reason: {reason}, Status:{_worker.ExecutionContext.Status.ToString()} File: {_worker.ExecutionContext.CurrentFileNumber} of {_worker.ExecutionContext.TotalFileCount}"));
            }
        }


    }
}
