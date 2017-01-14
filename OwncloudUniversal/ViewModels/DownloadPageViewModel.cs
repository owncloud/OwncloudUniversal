using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.Networking.BackgroundTransfer;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.UI.Xaml.Navigation;
using OwncloudUniversal.Services;
using OwncloudUniversal.Shared.Model;
using OwncloudUniversal.WebDav.Model;
using Template10.Mvvm;

namespace OwncloudUniversal.ViewModels
{
    public class DownloadPageViewModel : ViewModelBase
    {
        private decimal _percent = 0;
        private CancellationTokenSource _token;
        private StorageFile _file;
        private ulong _bytesInTotal;
        private ulong _bytesReceived;
        private bool _downloadCompleted;
        public DavItem TargetItem { get; private set; }
        public int Percent => (int)_percent;
        public string PercentText => _percent + " %";
        public ICommand OpenFileCommand { get; private set; }

        public DownloadPageViewModel()
        {
            OpenFileCommand = new DelegateCommand(async () => await OpenFile());
        }

        public ulong BytesInTotal
        {
            get { return _bytesInTotal; }
            set
            {
                _bytesInTotal = value;
                RaisePropertyChanged("BytesInTotal");
            }
        }

        public ulong BytesReceived
        {
            get { return _bytesReceived; }
            set
            {
                _bytesReceived = value;
                RaisePropertyChanged("BytesReceived");
            }
        }

        public bool DownloadCompleted
        {
            get { return _downloadCompleted; }
            private set
            {
                _downloadCompleted = value;
                RaisePropertyChanged("DownloadCompleted");
            }
        }


        public override async Task OnNavigatedToAsync(object parameter, NavigationMode mode,
            IDictionary<string, object> state)
        {
            var item = parameter as DavItem;
            if (item != null)
                TargetItem = item;
            RaisePropertyChanged("TargetItem");
            _token = new CancellationTokenSource();
            await PickFile();
            StartDownload();

            await base.OnNavigatedToAsync(parameter, mode, state);
        }

        private async Task PickFile()
        {
            FileSavePicker picker = new FileSavePicker();
            picker.FileTypeChoices.Add(TargetItem.ContentType, new List<string>() { Path.GetExtension(TargetItem.DisplayName) });
            picker.SuggestedFileName = TargetItem.DisplayName;
            picker.SuggestedStartLocation = PickerLocationId.Downloads;
            _file = await picker.PickSaveFileAsync();
        }

        private void StartDownload()
        {
            var operation = WebDavItemService.GetDefault().CreateDownload(TargetItem, _file);
            Progress<DownloadOperation> progressCallback = new Progress<DownloadOperation>(OnDownloadProgressChanged);
            var task = operation.StartAsync().AsTask(_token.Token, progressCallback);
            task.ContinueWith(OnDownloadCompleted);
        }

        private void OnDownloadProgressChanged(DownloadOperation download)
        {
            _percent = Math.Round((decimal) (100*download.Progress.BytesReceived)/download.Progress.TotalBytesToReceive);
            BytesInTotal = download.Progress.TotalBytesToReceive;
            BytesReceived = download.Progress.BytesReceived;
            RaisePropertyChanged("PercentText");
            RaisePropertyChanged("Percent");
        }

        private async  Task OnDownloadCompleted(Task<DownloadOperation> task)
        {
            var operation = await task;
            if (operation.Progress.Status == BackgroundTransferStatus.Completed)
                DownloadCompleted = true;

        }

        private async Task OpenFile()
        {
            await Windows.System.Launcher.LaunchFileAsync(_file);
        }
    }
}
