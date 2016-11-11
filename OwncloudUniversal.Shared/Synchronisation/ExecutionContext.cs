using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace OwncloudUniversal.Shared.Synchronisation
{
    public class ExecutionContext : INotifyPropertyChanged
    {
        private int _totalFileCount;
        private int _currentFileNumber;
        private string _currentFileName;
        private ExecutionStatus _status;
        private ConnectionProfile _connectionProfile;

        public ExecutionContext()
        {
            Status = ExecutionStatus.Stopped;
            CurrentFileName = string.Empty;
            CurrentFileNumber = 0;
            TotalFileCount = 0;
            ConnectionProfile = ConnectionProfile.WiFi;
        }

        public ExecutionStatus Status
        {
            get { return _status; }
            set
            {
                _status = value;
                OnPropertyChanged();
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

        public ConnectionProfile ConnectionProfile
        {
            get { return _connectionProfile; }
            set
            {
                _connectionProfile = value;
                OnPropertyChanged();
            }
        }

        public string FileText => $"{CurrentFileNumber} / {TotalFileCount} Files";

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
