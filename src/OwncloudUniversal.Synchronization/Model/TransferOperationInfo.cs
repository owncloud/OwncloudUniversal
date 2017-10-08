using System;
using System.Threading;
using Windows.Storage;
using Windows.Web.Http;

namespace OwncloudUniversal.Synchronization.Model
{
    public class TransferOperationInfo
    {
        public TransferOperationInfo() { }
        public TransferOperationInfo(HttpProgress httpProgress, CancellationTokenSource cancellationTokenSource)
        {
            HttpProgress = httpProgress;
            CancellationTokenSource = cancellationTokenSource;
        }

        public string DisplayName { get; set; }
        public StorageFile File { get; set; }
        public HttpProgress HttpProgress { get; set; }
        public CancellationTokenSource CancellationTokenSource { get; set; }
    }
}
