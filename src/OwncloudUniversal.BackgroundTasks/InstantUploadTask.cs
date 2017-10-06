using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Background;
using Windows.Networking.BackgroundTransfer;
using OwncloudUniversal.OwnCloud;
using OwncloudUniversal.Synchronization;
using OwncloudUniversal.Synchronization.Configuration;
using OwncloudUniversal.Synchronization.LocalFileSystem;
using OwncloudUniversal.Synchronization.Processing;

namespace OwncloudUniversal.BackgroundTasks
{
    public sealed class InstantUploadTask : IBackgroundTask
    {
        BackgroundTaskDeferral _deferral;
        InstantUploadProcess _worker;
        public async void Run(IBackgroundTaskInstance taskInstance)
        {
            taskInstance.Canceled += OnCanceled;
            _deferral = taskInstance.GetDeferral();

            try
            {
                var fileSystem = new FileSystemAdapter(true, null);
                var webDav = new WebDavAdapter(true, Configuration.ServerUrl, Configuration.Credential, fileSystem);
                fileSystem.LinkedAdapter = webDav;
                _worker = new InstantUploadProcess(fileSystem, webDav, true);
                await _worker.Run();
            }
            catch (Exception e)
            {
                await LogHelper.Write($"InstantUploadProcess Exception: {e.Message}" + Environment.NewLine +
                                      $"{e.StackTrace}" + Environment.NewLine +
                                      $"Status:{ExecutionContext.Instance.Status.ToString()}" + Environment.NewLine +
                                      $"File: {ExecutionContext.Instance.CurrentFileNumber}" + Environment.NewLine +
                                      $"of {ExecutionContext.Instance.TotalFileCount}");
            }
            finally
            {

                await LogHelper.Write("InstantUploadProcess finished");
                _deferral.Complete();
            }
        }

        private void OnCanceled(IBackgroundTaskInstance instance, BackgroundTaskCancellationReason reason)
        {
            if (_worker != null)
            {
                ExecutionContext.Instance.Status = ExecutionStatus.Stopped;
                ExecutionContext.Instance.TransferOperation.CancellationTokenSource.Cancel();
                Task.Run(() => LogHelper.Write($"InstantUploadProcess canceled. Reason: {reason}, Status:{ExecutionContext.Instance.Status.ToString()} File: {ExecutionContext.Instance.CurrentFileNumber} of {ExecutionContext.Instance.TotalFileCount}"));
            }
        }
    }
}
