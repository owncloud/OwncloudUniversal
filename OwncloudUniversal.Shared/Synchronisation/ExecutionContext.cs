using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
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
                if(value != Status)
                    OnPropertyChanged(nameof(Status));
                _status = value;
            }
        }

        public string CurrentFileName
        {
            get { return _currentFileName; }
            set
            {
                if(value != _currentFileName)
                    OnPropertyChanged("CurrentFileName");
                _currentFileName = value;
            }
        }

        public int CurrentFileNumber
        {
            get { return _currentFileNumber; }
            set
            {
                if (value != _currentFileNumber)
                {
                    OnPropertyChanged("FileText");
                    OnPropertyChanged("CurrentFileNumber");
                }
                _currentFileNumber = value;
            }
        }

        public int TotalFileCount
        {
            get { return _totalFileCount; }
            set
            {
                if (value != _totalFileCount)
                {
                    OnPropertyChanged("TotalFileCount");
                    OnPropertyChanged("FileText");
                }
                _totalFileCount = value;
            }  
        }

        public ConnectionProfile ConnectionProfile
        {
            get { return _connectionProfile; }
            set
            {
                if(value != _connectionProfile)
                    OnPropertyChanged("ConnectionProfile");
                _connectionProfile = value;
            }
        }

        public string FileText => $"{CurrentFileNumber} / {TotalFileCount} Files";

        //public event PropertyChangedEventHandler PropertyChanged;

        //private void OnPropertyChanged([CallerMemberName] string propertyName = null)
        //{
        //    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        //}
        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
