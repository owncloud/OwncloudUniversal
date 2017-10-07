using System.Threading;
using Windows.Web.Http;

namespace OwncloudUniversal.Synchronization.Model
{
    public class TransferOperationInfo
    {
        public TransferOperationInfo(HttpProgress httpProgress, CancellationTokenSource cancellationTokenSource)
        {
            HttpProgress = httpProgress;
            CancellationTokenSource = cancellationTokenSource;
        }

        public HttpProgress HttpProgress { get; }
        public CancellationTokenSource CancellationTokenSource { get; }
    }
}
