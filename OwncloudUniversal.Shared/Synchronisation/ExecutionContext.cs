using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.Resources;

namespace OwncloudUniversal.Shared.Synchronisation
{
    public class ExecutionContext : INotifyPropertyChanged
    {
        private int _totalFileCount;
        private int _currentFileNumber;
        private string _currentFileName;
        private ExecutionStatus _status;
        private static ExecutionContext _instance;

        public static ExecutionContext Instance => _instance ?? (_instance = new ExecutionContext());

        private ExecutionContext()
        {
            Status = ExecutionStatus.Stopped;
            CurrentFileName = string.Empty;
            CurrentFileNumber = 0;
            TotalFileCount = 0;
        }

        public string StatusMessage => ResourceLoader.GetForCurrentView("OwncloudUniversal.Shared/Resources").GetString(_status.ToString());

        public ExecutionStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
                OnPropertyChanged("StatusMessage");
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



        public string FileText => $"{CurrentFileNumber} / {TotalFileCount} {ResourceLoader.GetForCurrentView("OwncloudUniversal.Shared/Resources").GetString("Files")}";

        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
