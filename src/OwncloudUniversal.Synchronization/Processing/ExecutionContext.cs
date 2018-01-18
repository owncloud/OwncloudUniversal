using System.ComponentModel;
using System.Net;
using System.Runtime.CompilerServices;
using Windows.Web.Http;
using OwncloudUniversal.Synchronization.Model;


namespace OwncloudUniversal.Synchronization.Processing
{
    public class ExecutionContext : INotifyPropertyChanged
    {
        private int _totalFileCount;
        private int _currentFileNumber;
        private string _currentFileName;
        private ExecutionStatus _status;
        private TransferOperationInfo _transferOperation;
        private static ExecutionContext _instance;
        private bool _isPaused;

        public static ExecutionContext Instance => _instance ?? (_instance = new ExecutionContext());

        private ExecutionContext()
        {
            Status = ExecutionStatus.Ready;
            CurrentFileName = string.Empty;
            CurrentFileNumber = 0;
            TotalFileCount = 0;
        }

        public ExecutionStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged("StatusMessage");
                OnPropertyChanged("IsActive");
                OnPropertyChanged("ShowProgress");
            }
        }

        public string CurrentFileName
        {
            get { return WebUtility.UrlDecode(_currentFileName); }
            set
            {
                _currentFileName = value;
                OnPropertyChanged();
            }
        }

        public int CurrentFileNumber
        {
            get { return _currentFileNumber; }
            set
            {
                _currentFileNumber = value;
                OnPropertyChanged();
                OnPropertyChanged("FileText");
            }
        }

        public int TotalFileCount
        {
            get { return _totalFileCount; }
            set
            {
                _totalFileCount = value;
                OnPropertyChanged();
                OnPropertyChanged("FileText");
            }
        }

        public TransferOperationInfo TransferOperation
        {
            get { return _transferOperation; }
            set
            {
                _transferOperation = value;
                OnPropertyChanged();
            }
        }

        public bool IsActive => !(Status == ExecutionStatus.Finished || Status == ExecutionStatus.Ready || Status == ExecutionStatus.Stopped || Status == ExecutionStatus.Error || IsPaused);

        public bool ShowProgress => Status == ExecutionStatus.Sending || Status == ExecutionStatus.Receiving;

        public string FileText => $"{CurrentFileNumber} / {TotalFileCount}";

        public bool IsBackgroundTask { get; set; }

        public bool IsPaused
        {
            get => _isPaused;
            set
            {
                _isPaused = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
